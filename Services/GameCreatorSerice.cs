using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Poll.Components.Pages.NewGame;
using Poll.DAL;
using Poll.DAL.Entities;
using Poll.DAL.Services;
using Poll.Utils;

namespace Poll.Services;

public class GameCreatorService
{
    private readonly ILogger<GameCreatorService> _logger;
    private readonly FileManager _fileManager;
    private readonly DatabaseWriteContextProvider _databaseWriteContextProvider;
    private readonly IMemoryCache _memoryCache;

    public GameCreatorService(ILogger<GameCreatorService> logger,
        FileManager fileManager,
        DatabaseWriteContextProvider databaseWriteContextProvider,
        IMemoryCache memoryCache)
    {
        _logger = logger;
        _fileManager = fileManager;
        _databaseWriteContextProvider = databaseWriteContextProvider;
        _memoryCache = memoryCache;
    }

    public void SaveFormState(NewGameModel model, Guid identifier)
    {
        _logger.LogInformation("Saving form state : {model}, {identifier}", JsonSerializer.Serialize(model, Serialization.DefaultSerializationOptions), identifier);
        _memoryCache.Set(identifier, model, DateTimeOffset.UtcNow.AddDays(1));
    }

    public NewGameModel? RestoreFormState(Guid identifier)
    {
        if (_memoryCache.TryGetValue(identifier, out var result))
        {
            return (NewGameModel)result!;
        }

        return null;
    }

    public void CleanFormState(Guid identifier)
    {
        _memoryCache.Remove(identifier);
    }

    public async Task<GameTemplate> CreateGame(NewGameModel model)
    {
        _logger.LogInformation("Creating new game : {model}", JsonSerializer.Serialize(model, Serialization.DefaultSerializationOptions));
        var template = await _databaseWriteContextProvider.Write<PollContext, GameTemplate>(async db =>
        {
            var filesIds = model.Questions.SelectMany(q => new[] { q.QuestionImageId, q.ResponseImageId }).Where(i => i.HasValue).Select(i => i!.Value).ToArray();
            var files = await db.Files.Where(i => filesIds.Contains(i.Id)).ToArrayAsync();

            Dictionary<Guid, string> urlMappings = new Dictionary<Guid, string>();
            foreach (var file in files)
            {
                file.Path = _fileManager.MoveFileToFinalLocation(file);
                urlMappings.Add(file.Id, "/file/get/" + file.Id);
            }

            var gameTemplate = new GameTemplate()
            {
                Name = model.Name,
                Questions = model.Questions.Select((i, index1) => new Question()
                {
                    Content = i.Name,
                    Index = index1,
                    PresentingAnswerImageUrl = i.ResponseImageId.HasValue && urlMappings.TryGetValue(i.ResponseImageId.Value, out var mapping1) ? mapping1 : null,
                    AskingQuestionImageUrl = i.QuestionImageId.HasValue && urlMappings.TryGetValue(i.QuestionImageId.Value, out var mapping2) ? mapping2 : null,
                    Choices = i.Choices.Select((c, index2) => new QuestionChoice()
                    {
                        Content = c.Content,
                        IsValid = c.IsValid,
                        Index = index2,
                    }).ToList(),
                }).ToList(),
            };

            foreach (var file in files)
            {
                file.GameTemplate = gameTemplate;
            }

            db.GameTemplates.Add(gameTemplate);
            return gameTemplate;
        });

        return template;
    }
}