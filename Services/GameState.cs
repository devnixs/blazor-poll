using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Poll.DAL;
using Poll.DAL.Entities;
using Poll.DAL.Services;
using Poll.Utils;

namespace Poll.Services;

public class GameState : IDisposable
{
    private readonly DatabaseReadContextProvider _databaseReadContextProvider;
    private readonly ILogger<GameState> _logger;

    public Question[] Questions { get; private set; } = Array.Empty<Question>();
    public Question? CurrentQuestion { get; private set; }
    public GameTemplate Template { get; private set; } = null!;

    private readonly List<Answer> _answers = new List<Answer>();
    private readonly Object _answerLocker = new object();

    public GameStatus Status { get; private set; }


    public IEnumerable<Answer> Answers
    {
        get
        {
            lock (_answerLocker)
            {
                return _answers.ToArray();
            }
        }
    }

    private readonly ConcurrentDictionary<Guid, Player> _players = new();
    public IEnumerable<Player> Players => _players.Values;

    private Timer? _playerPurger;
    public DateTimeOffset CreationDate { get; private set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? QuestionStartTime { get; private set; }
    public Guid Id { get; private set; }
    private int GameTemplateId { get; set; }
    public int? QuestionDelaySeconds { get; set; }
    public decimal QuestionCurrentProgress { get; set; }

    public GameState(
        Guid id,
        int gameTemplateId,
        DatabaseReadContextProvider databaseReadContextProvider,
        ILogger<GameState> logger)
    {
        _databaseReadContextProvider = databaseReadContextProvider;
        _logger = logger;
        Id = id;
        GameTemplateId = gameTemplateId;
    }

    public async Task LoadData()
    {
        _logger.LogInformation("Loading template data");
        await _databaseReadContextProvider.Read<PollContext, int>(async db =>
        {
            Template = await db.GameTemplates
                .Include(i => i.Questions)
                .ThenInclude(i => i.Choices)
                .SingleAsync(i => i.Id == GameTemplateId);
            Questions = Template.Questions.ToArray();

            return 0;
        });
    }

    public GameTemplate GetCurrentGame()
    {
        return Template;
    }

    public Answer? GetCurrentAnswer(Guid playerId)
    {
        var currentQuestion = CurrentQuestion;
        if (currentQuestion is null)
        {
            return null;
        }

        lock (_answerLocker)
        {
            return _answers.FirstOrDefault(i => i.PlayerId == playerId && i.QuestionId == currentQuestion.Id);
        }
    }

    public async Task Initialize()
    {
        await LoadData();

        Status = GameStatus.WaitingForPlayers;
        _playerPurger = new Timer(_ => PurgeDisconnectedPlayers(), null, TimeSpan.Zero, TimeSpan.FromSeconds(10));
    }

    public Player? GetPlayer(Guid id)
    {
        _players.TryGetValue(id, out var player);
        return player;
    }

    public void PlayerHeartBeat(Player player)
    {
        _logger.LogInformation("Player heartbeat: {player}", JsonSerializer.Serialize(player, Serialization.DefaultSerializationOptions));
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

    private void PurgeDisconnectedPlayers()
    {
        var oneRemoved = false;
        foreach (var player in _players)
        {
            var ellapsed = DateTimeOffset.UtcNow - player.Value.HeartBeat;
            if (ellapsed > TimeSpan.FromSeconds(60))
            {
                _logger.LogInformation("Player {} has been disconnected", player.Value.Name);
                oneRemoved = oneRemoved || _players.TryRemove(player);
            }
        }

        if (oneRemoved)
        {
            OnStateChanged();
        }
    }

    public void Dispose()
    {
        _playerPurger?.Dispose();
    }

    public void SetPlayer(Player player)
    {
        var added = _players.TryAdd(player.Id, player);
        if (added)
        {
            _logger.LogInformation("Player set: {player}", JsonSerializer.Serialize(player, Serialization.DefaultSerializationOptions));
            OnStateChanged();
        }
    }

    public Player[] GetAllPlayers()
    {
        return _players.Values.OrderByDescending(i => i.Score).ToArray();
    }

    public int GetAnswersCount()
    {
        lock (_answerLocker)
        {
            return _answers.Count;
        }
    }

    public void PlayerNameChanged(Player eventDataPlayer)
    {
        if (_players.TryGetValue(eventDataPlayer.Id, out var player))
        {
            player.Name = eventDataPlayer.Name;
        }

        OnStateChanged();
    }

    public void AddAnswer(Answer answer)
    {
        lock (_answerLocker)
        {
            var existingItems = _answers.Where(i =>
                    i.GameId == answer.GameId
                    && i.PlayerId == answer.PlayerId
                    && i.QuestionId == answer.QuestionId)
                .ToArray();

            foreach (var a in existingItems)
            {
                _answers.Remove(a);
            }

            _logger.LogInformation("Answer added : {answer}", JsonSerializer.Serialize(answer, Serialization.DefaultSerializationOptions));
            _answers.Add(answer);
        }

        OnStateChanged();
    }

    public void SetState(GameStatus newStatus)
    {
        switch (newStatus)
        {
            case GameStatus.AskingQuestion when Status is GameStatus.WaitingForPlayers or GameStatus.DisplayQuestionResult:
            case GameStatus.DisplayQuestionResult when Status == GameStatus.AskingQuestion:
            case GameStatus.Completed when Status == GameStatus.DisplayQuestionResult:
                Status = newStatus;
                break;
            default:
                _logger.LogInformation("Invalid status switch from {} to {}", Status, newStatus);
                break;
        }

        _logger.LogInformation("Switched to new state : {newStatus}", newStatus);
        OnStateChanged();
    }

    public void SetCurrentQuestion(Question? question)
    {
        CurrentQuestion = question;
        QuestionStartTime = DateTimeOffset.UtcNow;
        QuestionCurrentProgress = 0;
        lock (_answerLocker)
        {
            _answers.Clear();
        }

        if (question is not null)
        {
            _logger.LogInformation("Switched to new question : {question}", JsonSerializer.Serialize(question, Serialization.DefaultSerializationOptions));
            SetState(GameStatus.AskingQuestion);
        }
    }

    private readonly List<Action> _stateChangedHandlers = new();

    public void SubscribeStateChanged(Action handler)
    {
        lock (_stateChangedHandlers)
        {
            _stateChangedHandlers.Add(handler);
        }
    }

    public void UnsubscribeStateChanged(Action handler)
    {
        lock (_stateChangedHandlers)
        {
            _stateChangedHandlers.Remove(handler);
        }
    }

    public virtual void OnStateChanged()
    {
        lock (_stateChangedHandlers)
        {
            foreach (var handlers in _stateChangedHandlers)
            {
                handlers();
            }
        }
    }

    public void Tick()
    {
        if (Status != GameStatus.AskingQuestion)
        {
            return;
        }

        if (QuestionDelaySeconds is null || QuestionStartTime is null)
        {
            return;
        }

        QuestionCurrentProgress = (decimal)(DateTimeOffset.UtcNow - QuestionStartTime.Value).TotalSeconds / QuestionDelaySeconds.Value;
        OnStateChanged();
    }
}