using System.ComponentModel;
using System.Threading.Channels;
using Microsoft.EntityFrameworkCore;
using Poll.DAL;
using Poll.DAL.Entities;
using Poll.DAL.Services;
using Poll.Events;
using Poll.Services.Abstractions;

namespace Poll.Services;

public class GameStateCache : IInitializer
{
    private readonly DatabaseReadContextProvider _databaseReadContextProvider;
    private readonly DatabaseWriteContextProvider _databaseWriteContextProvider;
    private readonly ILogger<GameStateCache> _logger;

    private Question? _question;
    private Game? _game; 

    public GameStateCache(
        DatabaseReadContextProvider databaseReadContextProvider,
        DatabaseWriteContextProvider databaseWriteContextProvider,
        ILogger<GameStateCache> logger)
    {
        _databaseReadContextProvider = databaseReadContextProvider;
        _databaseWriteContextProvider = databaseWriteContextProvider;
        _logger = logger;
    }

    public async Task Refresh()
    {
        _logger.LogInformation("Refreshing cache");
        await _databaseReadContextProvider.Read<PollContext, int>(async db =>
        {
            _game = await db.Games.Include(i=>i.Questions)
                .ThenInclude(i=>i.Choices)
                .FirstOrDefaultAsync(i => i.IsCurrent);
            
            _question = _game?.Questions.FirstOrDefault(i => i.IsCurrent);
        
            return 0;
        });
        
        await _databaseWriteContextProvider.Write<DomainEvents, int>(async db =>
        {
            await db.TriggerEvent(new CacheRefreshedEvent());
        
            return 0;
        });
    }

    public Game? GetCurrentGame()
    {
        return _game;
    }

    public Question? GetCurrentQuestion()
    {
        return _question;
    }

    public async Task OnInitialize()
    {
        await Refresh();
    }
}