using Microsoft.EntityFrameworkCore;
using MoreLinq;
using Poll.DAL;
using Poll.DAL.Entities;
using Poll.DAL.Services;
using Poll.Events;

namespace Poll.Services;

public class GameService
{
    private readonly PollContext _pollContext;
    private readonly DomainEvents _domainEvents;
    private readonly TransactionContext _transactionContext;

    public GameService(PollContext pollContext, DomainEvents domainEvents, TransactionContext transactionContext)
    {
        _pollContext = pollContext;
        _domainEvents = domainEvents;
        _transactionContext = transactionContext;
    }

    public async Task<Answer> PlayerSelectsAnswer(int playerId, int questionChoiceId)
    {
        var questionChoice = await _pollContext.QuestionChoices
            .Where(i => i.Id == questionChoiceId)
            .Select(i => new
            {
                GameId = i.Question.GameId,
                QuestionId = i.QuestionId,
                QuestionStartTime = i.Question.StartTime,
                IsValid = i.IsValid,
            })
            .SingleAsync();
        
        var existingItems = await _pollContext.Answers.Where(i =>
                i.GameId == questionChoice.GameId
                && i.PlayerId == playerId
                && i.QuestionId == questionChoice.QuestionId)
            .ToArrayAsync();

        _pollContext.Answers.RemoveRange(existingItems);

        var now = DateTimeOffset.UtcNow;
        var questionStartTime = questionChoice.QuestionStartTime ?? now;
        var answer = new Answer()
        {
            ChoiceId = questionChoiceId,
            PlayerId = playerId,
            QuestionId = questionChoice.QuestionId,
            AnswerTime = now - questionStartTime,
            GameId = questionChoice.GameId,
            Date = now,
            IsValid = questionChoice.IsValid,
        };
        _pollContext.Answers.Add(answer);

        await _domainEvents.TriggerEvent(new NewAnswerEvent()
        {
            Answer = answer,
        });

        return answer;
    }
    
    public async Task SetGameWaitingForPlayers(int gameId)
    {
        var currentGame = await _pollContext.Games.SingleOrDefaultAsync(i => i.IsCurrent && i.Id != gameId);

        if (currentGame is not null)
        {
            await FinishGame(currentGame);
        }

        var game = await _pollContext.Games.Include(i=>i.Questions).SingleAsync(i => i.Id == gameId);
        game.IsCurrent = true;
        game.State = GameState.WaitingForPlayers;
        await _domainEvents.TriggerEvent(new GameStateChangedEvent());
    }
    
    public async Task StartGame(int gameId)
    {
        var currentGame = await _pollContext.Games.SingleOrDefaultAsync(i => i.IsCurrent && i.Id != gameId);

        if (currentGame is not null)
        {
            await FinishGame(currentGame);
        }

        var players = await _pollContext.Players.ToArrayAsync();
        players.ForEach(i=>i.Score = 0);
        
        var game = await _pollContext.Games.Include(i=>i.Questions).SingleAsync(i => i.Id == gameId);
        game.IsCurrent = true;
        game.State = GameState.AskingQuestion;
        game.Questions.ForEach(i => i.IsCurrent = false);
        var firstQuestion = game.Questions.OrderBy(i => i.Index).First();
        firstQuestion.IsCurrent = true;
        await _domainEvents.TriggerEvent(new QuestionChangedEvent());

    }

    public async Task QuestionTimerEnds()
    {
        await ValidateQuestion();
    }
    
    public async Task<string?> ValidateGame(int gameId)
    {
        var game = await _pollContext.Games.Include(i => i.Questions).SingleAsync(i => i.Id == gameId);

        if (game.Questions.Count == 0)
        {
            return "Il faut au moins une question";
        }
        
        // Ensure all questions are valid
        for (var i = 1; i <= game.Questions.ToArray().Length; i++)
        {
            var question = game.Questions.ToArray()[i-1];
            if (question.Choices.Count > 4)
            {
                return $"Question {i} doit avoir au moins une réponse";
            }
            if (question.Choices.Count > 4)
            {
                return $"Question {i} doit avoir au maximum 4 réponses";
            }
            if (!question.Choices.Any(c=>c.IsValid))
            {
                return $"Question {i} doit avoir une réponse valide";
            }
        }

        return null;
    }
    
    public async Task ValidateQuestion()
    {
        var currentGame = await _pollContext.Games.SingleAsync(i => i.IsCurrent);
        if (currentGame.State != GameState.AskingQuestion)
        {
            return;
        }
        
        currentGame.State = GameState.DisplayQuestionResult;
        var currentQuestion = await _pollContext.Questions.Where(i=>i.GameId == currentGame.Id).SingleAsync(i => i.IsCurrent);
        await ComputeScores(currentQuestion.Id, currentGame.Id);

        await _domainEvents.TriggerEvent(new QuestionValidatedEvent());
    }
    
    public async Task MoveToNextQuestion()
    {
        var currentGame = await _pollContext.Games.SingleAsync(i => i.IsCurrent);
        if (currentGame.State != GameState.DisplayQuestionResult)
        {
            return;
        }

        var questions = await _pollContext.Questions.Where(i => i.GameId == currentGame.Id).OrderBy(i => i.Index)
            .ToArrayAsync();
        var current = questions.Select((question, index) =>(question, index)).FirstOrDefault(i => i.question.IsCurrent);
        
        current.question.IsCurrent = false;

        if (current.index == questions.Length - 1)
        {
            await FinishGame(currentGame);
        }
        else
        {
            questions[current.index + 1].IsCurrent = true;
            questions[current.index +1].StartTime = DateTimeOffset.UtcNow;
            currentGame.State = GameState.AskingQuestion;
            await _domainEvents.TriggerEvent(new QuestionChangedEvent());
        }
    }
    
    public async Task ComputeScores(int questionId, int gameId)
    {
        var answers = await _pollContext.Answers.Where(i => i.QuestionId == questionId && i.GameId == gameId).ToArrayAsync();
        if (answers.Length == 0)
        {
            return;
        }
        
        var maxTime = answers.Max(i => i.AnswerTime);
        var totalSeconds = maxTime.TotalSeconds > 0 ? maxTime.TotalSeconds : 1;
        var playerIds = answers.Select(i => i.PlayerId).ToArray();
        var players = await _pollContext.Players.Where(p=>playerIds.Contains(p.Id)).ToDictionaryAsync(i=>i.Id);

        foreach (var answer in answers)
        {
            if (answer.IsValid)
            {
                answer.Score = (int)Math.Floor(100d + 20d * (1d - answer.AnswerTime.TotalSeconds / totalSeconds));
                if (players.TryGetValue(answer.PlayerId, out var player))
                {
                    player.Score += answer.Score;
                }
            }
            else
            {
                answer.Score = 0;
            }
        }
    }
    
    private async Task FinishGame(Game game)
    {
        var questions = await _pollContext.Questions.Where(i => i.GameId == game.Id).ToArrayAsync();
        foreach (var question in questions)
        {
            question.IsCurrent = false;
        }

        game.State = GameState.Completed;
        await _domainEvents.TriggerEvent(new GameStateChangedEvent());
    }
}