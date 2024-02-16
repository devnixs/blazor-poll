using System.ComponentModel;
using System.Threading.Channels;
using Microsoft.EntityFrameworkCore;
using Poll.DAL;
using Poll.DAL.Entities;
using Poll.DAL.Services;
using Poll.Events;
using Poll.Services.Abstractions;

namespace Poll.Services;

public class GameStateCache : IInitializer, IDisposable
{
    private readonly DatabaseReadContextProvider _databaseReadContextProvider;
    private readonly DatabaseWriteContextProvider _databaseWriteContextProvider;
    private readonly ILogger<GameStateCache> _logger;

    private Question? _question;
    private Game? _game; 
    private Answer[] _answers = Array.Empty<Answer>(); 
    private Player[] _players = Array.Empty<Player>();

    private Timer? _heartBeatSaver; 

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
            _answers = _question != null ? await db.Answers.Where(i => i.QuestionId == _question.Id).ToArrayAsync() : Array.Empty<Answer>();
            _players = await db.Players.ToArrayAsync();
        
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
    
    public Answer? GetCurrentAnswer(int playerId)
    {
        return _answers.FirstOrDefault(i=>i.PlayerId == playerId);
    }

    public async Task OnInitialize()
    {
        await Refresh();

        _heartBeatSaver = new Timer(_ => SaveHeartBeats(), null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
    }
    
    public Player GetPlayer(int id)
    {
        return _players.Single(i=>i.Id == id);
    }

    public void PlayerHeartBeat(int playerId)
    {
        var player = _players.SingleOrDefault(i => i.Id == playerId);
        if (player == null)
        {
            return;
        }
        player.HeartBeat = DateTimeOffset.UtcNow;
    }
    
    private void SaveHeartBeats()
    {
        _ = _databaseWriteContextProvider.Write<PollContext, int>(async db =>
        {
            _logger.LogInformation("Saving heartbeat of {} players", _players.Length);
            foreach (var player in _players)
            {
                var timestampSqlFormat = player.HeartBeat.ToSqlFormat();
                await db.Database.ExecuteSqlAsync($"""UPDATE "Players" SET "HeartBeat" = {player.HeartBeat} WHERE "Players"."Id" = {player.Id}; """);
            }

            return 0;
        });
    }

    public void Dispose()
    {
        _heartBeatSaver?.Dispose();
    }
}