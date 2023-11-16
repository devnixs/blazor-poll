using Blazored.LocalStorage;
using Microsoft.EntityFrameworkCore;
using Poll.DAL;
using Poll.DAL.Entities;

namespace Poll.Services;

public class PlayerService(ILocalStorageService localStorage, PollContext pollContext)
{
    private string Key = "PlayerId";

    public async Task<Player?> GetPlayer()
    {
        if (!await localStorage.ContainKeyAsync(Key)) return null;

        var playerId = await localStorage.GetItemAsync<int>(Key);
        return await pollContext.Players.SingleOrDefaultAsync(i => i!.Id == playerId);
    }

    public async Task<Player> SetPlayerName(string name)
    {
        if (!await localStorage.ContainKeyAsync(Key))
        {
            var entity = new Player()
            {
                Name = name,
            };

            pollContext.Players.Add(entity);
            await pollContext.SaveChangesAsync();
            await localStorage.SetItemAsync(Key, entity.Id);
            return entity;
        }
        else
        {
            var playerId = await localStorage.GetItemAsync<int>(Key);
            var player = await pollContext.Players.SingleOrDefaultAsync(i => i!.Id == playerId);
            if (player is null)
            {
                player = new Player()
                {
                    Name = name,
                };
                pollContext.Add(player);
            }
            
            await pollContext.SaveChangesAsync();
            return player;
        }
    }
}