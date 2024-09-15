using Blazored.LocalStorage;
using Poll.DAL.Entities;
using Poll.DAL.Services;

namespace Poll.Services;

public class PlayerService
{
    private readonly ILocalStorageService _localStorage;
    private readonly GameStateAccessor _gameStateAccessor;
    private readonly ILogger<PlayerService> _logger;

    public PlayerService(ILocalStorageService localStorage,
        GameStateAccessor gameStateAccessor,
        ILogger<PlayerService> logger)
    {
        _localStorage = localStorage;
        _gameStateAccessor = gameStateAccessor;
        _logger = logger;
    }

    private string Key = "PlayerId";
    private string AdminKey = "Admin_";
    private List<Action> NameChangedHandlers = new();

    public void SubscribeNameChanged(Action handler)
    {
        _logger.LogInformation("Subscribed to named changed");
        lock (NameChangedHandlers)
        {
            NameChangedHandlers.Add(handler);
        }
    }

    private void TriggerNameChanged()
    {
        _logger.LogInformation("Triggering named changed");
        lock (NameChangedHandlers)
        {
            foreach (var handler in NameChangedHandlers)
            {
                handler.Invoke();
            }
        }
    }

    public void UnsubscribeNameChanged(Action handler)
    {
        _logger.LogInformation("Unsubscribed to named changed");
        lock (NameChangedHandlers)
        {
            NameChangedHandlers.Remove(handler);
        }
    }

    public async Task<Player?> GetPlayer(Guid gameId)
    {
        if (!await _localStorage.ContainKeyAsync(Key))
        {
            return null;
        }

        var game = _gameStateAccessor.GetGame(gameId);
        
        var playerId = await _localStorage.GetItemAsync<Guid>(Key);
        return game?.GetPlayer(playerId);
    }

    public async Task<bool> IsAdmin(Guid gameId)
    {
        return await _localStorage.ContainKeyAsync(AdminKey + gameId) || await _localStorage.ContainKeyAsync(AdminKey);
    }
    
    public async Task SetAdmin(Guid gameId)
    {
        await _localStorage.SetItemAsStringAsync(AdminKey+gameId, "true");
    }

    public async Task<Player?> SetPlayerName(Guid gameId, string name)
    {
        var game = _gameStateAccessor.GetGame(gameId);
        if (game == null)
        {
            _logger.LogInformation("Could not find game {}", gameId);
            return null;
        }

        Player? player;
        if (!await _localStorage.ContainKeyAsync(Key))
        {
            player = new Player()
            {
                Id = Guid.NewGuid(),
                Name = name,
                HeartBeat = DateTimeOffset.UtcNow,
            };

            game.SetPlayer(player);
            
            await _localStorage.SetItemAsync(Key, player.Id);
        }
        else
        {
            var playerId = await _localStorage.GetItemAsync<Guid>(Key);
            player = game.GetPlayer(playerId);
            if (player is null)
            {
                player = new Player()
                {
                    Id = playerId,
                    Name = name,
                    HeartBeat = DateTimeOffset.UtcNow,
                };
                game.SetPlayer(player);
            }
            else
            {
                if (!player.NameIsLocked)
                {
                    player.Name = name;
                }
            }
        }

        TriggerNameChanged();
        return player;
    }
}