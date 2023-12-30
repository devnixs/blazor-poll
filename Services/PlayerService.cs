using Blazored.LocalStorage;
using Microsoft.EntityFrameworkCore;
using Poll.DAL;
using Poll.DAL.Entities;

namespace Poll.Services;

public class PlayerService(ILocalStorageService localStorage, DbContextProvider ctxProvider,  ILogger<PlayerService> logger)
{
    private string Key = "PlayerId";
    private List<Action> NameChangedHandlers = new();

    public void SubscribeNameChanged(Action handler)
    {
        logger.LogInformation("Subscribed to named changed");
        lock (NameChangedHandlers)
        {
            NameChangedHandlers.Add(handler);
        }
    }

    private void TriggerNameChanged()
    {
        logger.LogInformation("Triggering named changed");
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
        logger.LogInformation("Unsubscribed to named changed");
        lock (NameChangedHandlers)
        {
            NameChangedHandlers.Remove(handler);
        }
    }
    
    public async Task<Player?> GetPlayer()
    {
        if (!await localStorage.ContainKeyAsync(Key)) return null;

        var playerId = await localStorage.GetItemAsync<int>(Key);
        using (ctxProvider.ProvidePollContext(out var db))
        {
            return await db.Players.SingleOrDefaultAsync(i => i!.Id == playerId);
        }
    }

    public async Task<Player> SetPlayerName(string name)
    {
        using var ctx = ctxProvider.ProvidePollContext(out var db);
        
        if (!await localStorage.ContainKeyAsync(Key))
        {
            var entity = new Player()
            {
                Name = name,
            };

            db.Players.Add(entity);
            await db.SaveChangesAsync();
            await localStorage.SetItemAsync(Key, entity.Id);

            TriggerNameChanged();
            
            return entity;
        }
        else
        {
            var playerId = await localStorage.GetItemAsync<int>(Key);
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
            
            TriggerNameChanged();
            
            return player;
        }

    }
}