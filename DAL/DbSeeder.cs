using Microsoft.EntityFrameworkCore;
using Poll.DAL.Entities;

namespace Poll.DAL;

public class DbSeeder(IServiceProvider svc, ILogger<DbSeeder> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = svc.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<PollContext>();
        var isSeeded = await db.Questions.AnyAsync(cancellationToken: cancellationToken);

        if (!isSeeded)
        {
            await Seed(db);
        }
        else
        {
            logger.LogInformation("Database is already seeded");
        }
    }

    private async Task Seed(PollContext pollContext)
    {
        logger.LogInformation("Starting seeding");
        await pollContext.Questions.ExecuteDeleteAsync();

        var question1 = new Question()
        {
            Content = "Test Question 1",
            Index = 1,
        };

        var choice1 = new QuestionChoice()
        {
            Content = "Choix 1",
            Index = 1,
            Question = question1,
        };

        var choice2 = new QuestionChoice()
        {
            Content = "Choix 2",
            Index = 2,
            Question = question1,
        };
        
        var question2 = new Question()
        {
            Content = "Test Question 2",
            Index = 2,
        };

        var choice3 = new QuestionChoice()
        {
            Content = "Choix 1",
            Index = 1,
            Question = question2,
        };

        var choice4 = new QuestionChoice()
        {
            Content = "Choix 2",
            Index = 2,
            Question = question2,
        };
        
        pollContext.Add(question1);
        pollContext.Add(choice1);
        pollContext.Add(choice2);
        pollContext.Add(question2);
        pollContext.Add(choice3);
        pollContext.Add(choice4);

        await pollContext.SaveChangesAsync();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}