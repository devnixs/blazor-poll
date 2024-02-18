using System.Collections.Concurrent;
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
    private readonly DomainEvents _domainEvents;
    private readonly ILogger<GameStateCache> _logger;

    private Question? _question;
    private Game? _game;
    private ConcurrentDictionary<int, Answer> _answers = new();
    private ConcurrentDictionary<int, Player> _players = new();

    private Object _answerLocker = new object();

    private Timer? _heartBeatSaver; 
    private Timer? _playerPurger; 

    public GameStateCache(
        DatabaseReadContextProvider databaseReadContextProvider,
        DatabaseWriteContextProvider databaseWriteContextProvider,
        DomainEvents domainEvents,
        ILogger<GameStateCache> logger)
    {
        _databaseReadContextProvider = databaseReadContextProvider;
        _databaseWriteContextProvider = databaseWriteContextProvider;
        _domainEvents = domainEvents;
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
            var answersFrombDb = _question != null ? await db.Answers.Where(i => i.QuestionId == _question.Id).ToArrayAsync() : Array.Empty<Answer>();

            lock (_answerLocker)
            {
                _answers.Clear();
                foreach (var answerFromDb in answersFrombDb)
                {
                    _answers.TryAdd(answerFromDb.PlayerId, answerFromDb);
                }
            }
            
            // Load scores from DB
            var playersFromDb = await db.Players.ToArrayAsync();
            foreach (var playerFromDb in playersFromDb)
            {
                if (_players.TryGetValue(playerFromDb.Id, out var p))
                {
                    p.Score = playerFromDb.Score;
                }
            }
            _logger.LogInformation("Loaded {} players from DB", playersFromDb.Length);
            
            return 0;
        });
        
        await _databaseWriteContextProvider.Write<DomainEvents, int>(async db =>
        {
            await db.TriggerEvent(new CacheRefreshedEvent());
        
            return 0;
        });
    }

    public async Task LoadPlayers()
    {
        await _databaseReadContextProvider.Read<PollContext, int>(async db =>
        {
            var minHeartBeat = DateTimeOffset.UtcNow.AddMinutes(-1);
            var playersFromDb = await db.Players.Where(i=>i.HeartBeat > minHeartBeat).ToArrayAsync();

            foreach (var player in playersFromDb)
            {
                _players.TryAdd(player.Id, player);
            }
            _logger.LogInformation("Loaded {} players from DB", playersFromDb.Length);

            return 0;
        });
        _ = _domainEvents.TriggerEvent(new PlayersCountChangedEvent());
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
        lock (_answerLocker)
        {
            _answers.TryGetValue(playerId, out var player);
            return player;
        }
    }

    public async Task OnInitialize()
    {
        await Refresh();

        _heartBeatSaver = new Timer(_ => SaveHeartBeats(), null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
        _playerPurger = new Timer(_ => PurgeDisconnectedPlayers(), null, TimeSpan.Zero, TimeSpan.FromSeconds(10));

        await LoadPlayers();
    }
    
    public Player? GetPlayer(int id)
    {
        _players.TryGetValue(id, out var player);
        return player;
    }

    public void PlayerHeartBeat(Player player)
    {
        if (_players.TryGetValue(player.Id, out var p))
        {
            p.HeartBeat = DateTimeOffset.UtcNow;
        }
        else
        {
            _players.TryAdd(player.Id, player);
            player.HeartBeat = DateTimeOffset.UtcNow;
        }
    }
    
    private void SaveHeartBeats()
    {
        _ = _databaseWriteContextProvider.Write<PollContext, int>(async db =>
        {
            var players = _players.Values;
            _logger.LogInformation("Saving heartbeat of {} players", players.Count);
            foreach (var player in players)
            {
                await db.Database.ExecuteSqlAsync($"""UPDATE "Players" SET "HeartBeat" = {player.HeartBeat} WHERE "Players"."Id" = {player.Id}; """);
            }

            return 0;
        });
    }
    
    private void PurgeDisconnectedPlayers()
    {
        var oneRemoved = false;
        foreach (var player in _players)
        {
            var ellapsed = DateTimeOffset.UtcNow - player.Value.HeartBeat;
            if (ellapsed > TimeSpan.FromSeconds(20))
            {
                _logger.LogInformation("Player {} has been disconnected", player.Value.Name);
                oneRemoved = oneRemoved || _players.TryRemove(player);
            }
        }

        if (oneRemoved)
        {
            _ = _domainEvents.TriggerEvent(new PlayersCountChangedEvent());
        }
    }

    public void Dispose()
    {
        _heartBeatSaver?.Dispose();
        _playerPurger?.Dispose();
    }

    public void SetPlayer(Player player)
    {
        var added = _players.TryAdd(player.Id, player);
        if (added)
        {
            _ = _domainEvents.TriggerEvent(new PlayersCountChangedEvent());
        }
    }

    public Player[] GetAllPlayers()
    {
        return _players.Values.OrderByDescending(i=>i.Score).ToArray();
    }

    public void OnNewAnswer(Answer newAnswer)
    {
        lock(_answerLocker)
        {
            _answers.TryAdd(newAnswer.PlayerId, newAnswer);
        }
    }

    public int GetAnswersCount()
    {
        lock (_answerLocker)
        {
            return _answers.Count;
        }
    }

    public async Task PlayerNameChanged(Player eventDataPlayer)
    {
        if (_players.TryGetValue(eventDataPlayer.Id, out var player))
        {
            player.Name = eventDataPlayer.Name;
            await _domainEvents.TriggerEvent(new PlayersCountChangedEvent());
        }
    }
}