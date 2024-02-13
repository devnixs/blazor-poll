using Blazored.LocalStorage;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Poll.DAL;
using Poll.DAL.Entities;
using Poll.DAL.Services;

namespace Poll.Services;

public class PlayerService
{
    private readonly ILocalStorageService _localStorage;
    private readonly DatabaseWriteContextProvider _databaseWriteContextProvider;
    private readonly DatabaseReadContextProvider _databaseReadContextProvider;
    private readonly ILogger<PlayerService> _logger;

    public PlayerService(ILocalStorageService localStorage,
        DatabaseWriteContextProvider databaseWriteContextProvider,
        DatabaseReadContextProvider databaseReadContextProvider,
        ILogger<PlayerService> logger)
    {
        _localStorage = localStorage;
        _databaseWriteContextProvider = databaseWriteContextProvider;
        _databaseReadContextProvider = databaseReadContextProvider;
        _logger = logger;
    }

    private string Key = "PlayerId";
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

    public async Task<Player?> GetPlayer()
    {
        if (!await _localStorage.ContainKeyAsync(Key)) return null;

        var playerId = await _localStorage.GetItemAsync<int>(Key);
        return await _databaseReadContextProvider.Read<PollContext, Player?>(
            async db => await db.Players.SingleOrDefaultAsync(i => i!.Id == playerId));
    }

    public async Task<Player?> SetPlayerName(string name)
    {
        var p = await _databaseWriteContextProvider.Write<PollContext, Player?>(
            async db =>
            {
                if (!await _localStorage.ContainKeyAsync(Key))
                {
                    var entity = new Player()
                    {
                        Name = name,
                    };

                    db.Players.Add(entity);
                    await db.SaveChangesAsync();
                    await _localStorage.SetItemAsync(Key, entity.Id);

                    return entity;
                }
                else
                {
                    var playerId = await _localStorage.GetItemAsync<int>(Key);
                    var player = await db.Players.SingleOrDefaultAsync(i => i!.Id == playerId);
                    if (player is null)
                    {
                        player = new Player()
                        {
                            Name = name,
                        };
                        db.Add(player);
                    }
                    else
                    {
                        player.Name = name;
                    }

                    await db.SaveChangesAsync();

                    return player;
                }
            });

        TriggerNameChanged();
        return p;
    }
}