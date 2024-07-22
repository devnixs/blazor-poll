using Microsoft.EntityFrameworkCore;
using Poll.DAL;

namespace Poll.Services;

public class GameTemplateRepository(PollContext db, ILogger<GameTemplateRepository> logger)
{
    public async Task DeleteGame(int id)
    {
        var files = await db.Files.Where(i => i.GameTemplateId == id).ToArrayAsync();
        var template = await db.GameTemplates.SingleAsync(i => i.Id == id);
        db.Remove(template);

        foreach (var file in files)
        {
            db.Remove(file);
            
            try
            {
                File.Delete(file.Path);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Could not delete file {file}", file.Path);
            }
        }
    }
}