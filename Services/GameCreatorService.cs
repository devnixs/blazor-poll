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
        _logger.LogInformation("Saving form state : {model}, {identifier}", JsonSerializer.Serialize(model, Serialization.DefaultSerializationOptions),
            identifier);
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
            var filesIds = model.Questions.SelectMany(q => new[] { q.QuestionImageId, q.ResponseImageId }).Where(i => i.HasValue).Select(i => i!.Value)
                .ToArray();
            var files = await db.Files.Where(i => filesIds.Contains(i.Id)).ToArrayAsync();

            foreach (var file in files)
            {
                file.Path = _fileManager.MoveFileToFinalLocation(file);
            }

            var gameTemplate = new GameTemplate()
            {
                Name = model.Name,
                Questions = MapQuestionModelToQuestions(model),
                WaitingImageId = model.WaitingImageId,
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

    private static List<Question> MapQuestionModelToQuestions(NewGameModel model)
    {
        return model.Questions.Select((i, index1) => new Question()
        {
            Content = i.Name,
            Index = index1,
            PresentingAnswerImageId = i.ResponseImageId,
            AskingQuestionImageId = i.QuestionImageId,
            Choices = i.Choices.Select((c, index2) => new QuestionChoice()
            {
                Content = c.Content,
                IsValid = c.IsValid,
                Index = index2,
            }).ToList(),
        }).ToList();
    }

    public async Task<GameTemplate> UpdateGame(string existingTemplateIdentifier, NewGameModel model)
    {
        return await _databaseWriteContextProvider.Write<PollContext, GameTemplate>(async db =>
        {
            var existing = await db.GameTemplates
                .Include(i => i.Files)
                .Include(i => i.Questions)
                .ThenInclude(i => i.Choices)
                .SingleAsync(i => i.Identifier == existingTemplateIdentifier);

            existing.Name = model.Name;
            if (model.WaitingImageId.HasValue)
            {
                existing.WaitingImageId = model.WaitingImageId;
            }
            else
            {
                existing.WaitingImageId = null;
            }

            var choices = existing.Questions.SelectMany(i => i.Choices);
            db.RemoveRange(choices);
            db.RemoveRange(existing.Questions);

            // move newly created files
            var filesIds = model.Questions
                .SelectMany(q => new[] { q.QuestionImageId, q.ResponseImageId })
                .Concat(new[] { model.WaitingImageId })
                .Where(i => i.HasValue)
                .Select(i => i!.Value)
                .ToArray();
            var files = await db.Files.Where(i => filesIds.Contains(i.Id)).ToArrayAsync();

            foreach (var file in files)
            {
                if (file.GameTemplateId is null)
                {
                    file.Path = _fileManager.MoveFileToFinalLocation(file);
                    file.GameTemplateId = existing.Id;
                }
            }

            existing.Questions = MapQuestionModelToQuestions(model);

            return existing;
        });
    }
}