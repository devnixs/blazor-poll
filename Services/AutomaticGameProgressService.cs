using Poll.DAL.Entities;

namespace Poll.Services;

public class AutomaticGameProgressService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AutomaticGameProgressService> _logger;

    public AutomaticGameProgressService(IServiceProvider serviceProvider, ILogger<AutomaticGameProgressService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var gameStateAccessor = _serviceProvider.GetRequiredService<GameStateAccessor>();
            var gameService = _serviceProvider.GetRequiredService<GameService>();
            var gameIds = gameStateAccessor.GetGames();

            foreach (var gameId in gameIds)
            {
                var game = gameStateAccessor.GetGame(gameId);
                if (game is null)
                {
                    continue;
                }

                if (game.Status != GameStatus.AskingQuestion)
                {
                    continue;
                }
                
                _logger.LogInformation("Ticking game {game}", gameId);
                game.Tick();
                if (game.QuestionCurrentProgress >= 1)
                {
                    gameService.ValidateQuestion(gameId);
                }
            }

            await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
        }
    }
}