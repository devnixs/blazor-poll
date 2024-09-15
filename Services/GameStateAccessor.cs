using Poll.DAL.Services;

namespace Poll.Services;

public class GameStateAccessor : BackgroundService
{
    private readonly DatabaseReadContextProvider _databaseReadContextProvider;
    private readonly ILogger<GameState> _logger1;
    private readonly ILogger<GameState> _logger2;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public GameStateAccessor(
        DatabaseReadContextProvider databaseReadContextProvider,
        ILogger<GameState> logger1,
        ILogger<GameState> logger2,
        IWebHostEnvironment webHostEnvironment)
    {
        _databaseReadContextProvider = databaseReadContextProvider;
        _logger1 = logger1;
        _logger2 = logger2;
        _webHostEnvironment = webHostEnvironment;
    }

    private readonly Dictionary<Guid, GameState> _games = new();

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await CreateDefaultGame();
        
        while (!stoppingToken.IsCancellationRequested)
        {
            var games = _games.Values;
            foreach (var g in games)
            {
                if (g.CreationDate < DateTimeOffset.UtcNow - TimeSpan.FromDays(200))
                {
                    _logger1.LogInformation("Deleting game {}", g.Id);
                    g.Dispose();
                    _games.Remove(g.Id);
                }
            }

            await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
        }
    }

    public async Task<GameState> CreateGame(CreateGameParameters parameters)
    {
        var id = parameters.GameId ?? Guid.NewGuid();
        var game = new GameState(id, parameters.TemplateId, _databaseReadContextProvider, _logger2)
        {
            QuestionDelaySeconds = parameters.QuestionDelay,
        };
        await game.Initialize();
        _games.Add(id, game);
        return game;
    }

    private async Task CreateDefaultGame()
    {
        if (_webHostEnvironment.IsDevelopment())
        {
            var id = Guid.Empty;
            var game = new GameState(id, 1, _databaseReadContextProvider, _logger2);
            await game.Initialize();
            _games.Add(id, game);
            _logger1.LogInformation("Created default game: {id}", id);
        }
    }

    public GameState? GetGame(Guid id)
    {
        return _games.GetValueOrDefault(id);
    }

    public Guid[] GetGames()
    {
        return _games.Keys.ToArray();
    }
}

public record CreateGameParameters
{
    public int TemplateId { get; set; }
    public Guid? GameId { get; set; }
    public int? QuestionDelay { get; set; }
}
