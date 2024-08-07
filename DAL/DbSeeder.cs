﻿using Microsoft.EntityFrameworkCore;
using Poll.DAL.Entities;
using Poll.Services.Abstractions;

namespace Poll.DAL;

public class DbSeeder : IInitializer
{
    private readonly IServiceProvider _svc;
    private readonly ILogger<DbSeeder> _logger;

    public DbSeeder(IServiceProvider svc, ILogger<DbSeeder> logger)
    {
        _svc = svc;
        _logger = logger;
    }

    public async Task OnInitialize()
    {
        await using var scope = _svc.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<PollContext>();
        var isSeeded = await db.Questions.AnyAsync();

        if (!isSeeded)
        {
            await Seed(db);
        }
        else
        {
            _logger.LogInformation("Database is already seeded");
        }
    }

    private async Task Seed(PollContext pollContext)
    {
        _logger.LogInformation("Starting seeding");

        var question0 = new Question()
        {
            Content = "Au sud de quel continent est situé le cap de Bonne-Espérance ?",
            Choices = new List<QuestionChoice>()
            {
                new QuestionChoice()
                {
                    Content = "Le continent africain",
                    IsValid = true,
                },
                new QuestionChoice()
                {
                    Content = "Le continent australien",
                },
                new QuestionChoice()
                {
                    Content = "Le continent américain",
                },
            },
        };

        var question1 = new Question()
        {
            Content = "Quelle est la ville la plus peuplée du monde ?",
            Choices = new List<QuestionChoice>()
            {
                new QuestionChoice()
                {
                    Content = "New Delhi",
                },
                new QuestionChoice()
                {
                    Content = "Shanghai",
                },
                new QuestionChoice()
                {
                    Content = "Tokyo",
                    IsValid = true,
                },
                new QuestionChoice()
                {
                    Content = "Sao Paulo",
                },
            },
        };

        var question2 = new Question()
        {
            Content = "Quelle est la ville la plus septentrionale (la plus au nord) de France ?",
            Choices = new List<QuestionChoice>()
            {
                new QuestionChoice()
                {
                    Content = "Dunkerques",
                },
                new QuestionChoice()
                {
                    Content = "Bray-Dunes",
                    IsValid = true,
                },
                new QuestionChoice()
                {
                    Content = "Wattrelos",
                },
                new QuestionChoice()
                {
                    Content = "Zuydcoote",
                },
            },
        };

        var question3 = new Question()
        {
            Content = "Quel est le plus haut sommet d’Afrique ?",
            Choices = new List<QuestionChoice>()
            {
                new QuestionChoice()
                {
                    Content = "Le Mont Elbrouz",
                },
                new QuestionChoice()
                {
                    Content = "Le Kilimandjaro",
                    IsValid = true,
                },
                new QuestionChoice()
                {
                    Content = "L'Aconcagua",
                },
                new QuestionChoice()
                {
                    Content = "Le Denali (Mont McKinley)",
                },
            },
        };

        var question4 = new Question()
        {
            Content = "Dans quelle ville peut-on photographier la statue du Mannekenpis ?",
            Choices = new List<QuestionChoice>()
            {
                new QuestionChoice()
                {
                    Content = "Anvers",
                },
                new QuestionChoice()
                {
                    Content = "Amsterdam",
                },
                new QuestionChoice()
                {
                    Content = "Tournai",
                },
                new QuestionChoice()
                {
                    Content = "Bruxelles",
                    IsValid = true,
                },
            },
        };

        var question5 = new Question()
        {
            Content = "Avec plus de 1 600 m de profondeur, quel est le lac le plus profond du monde ?",
            Choices = new List<QuestionChoice>()
            {
                new QuestionChoice()
                {
                    Content = "Le lac Supérieur (Canada et Etats-Unis)",
                },
                new QuestionChoice()
                {
                    Content = "Le lac Victoria (Kenya, Tanzanie et Ouganda)",
                },
                new QuestionChoice()
                {
                    Content = "Le lac Michigan (États-Unis)",
                },
                new QuestionChoice()
                {
                    Content = "Le lac Baïkal (Sibérie)",
                    IsValid = true,
                },
            },
        };

        var questions = new[]
        {
            question0,
            question1,
            question2,
            question3,
            question4,
            question5,
        };

        var game = new GameTemplate()
        {
            CreationDate = DateTimeOffset.UtcNow,
            Name = "Quizz Géographie"
        };

        for (var index = 0; index < questions.Length; index++)
        {
            var question = questions[index];
            question.Index = index;
            question.GameTemplate = game;

            for (var index2 = 0; index2 < question.Choices.ToArray().Length; index2++)
            {
                var questionChoice = question.Choices.ToArray()[index2];
                questionChoice.Index = index2;
            }
        }

        pollContext.AddRange(questions);

        await pollContext.SaveChangesAsync();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}