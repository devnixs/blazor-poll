using Microsoft.EntityFrameworkCore;
using Poll.DAL;
using Poll.DAL.Entities;

namespace Poll.Services;

public class GameService
{
    private readonly PollContext _pollContext;
    private readonly AppSynchronizer _appSynchronizer;

    public GameService(PollContext pollContext, AppSynchronizer appSynchronizer)
    {
        _pollContext = pollContext;
        _appSynchronizer = appSynchronizer;
    }

    public async Task PlayerSelectsAnswer(int playerId, int questionChoiceId)
    {
        var questionChoice = await _pollContext.QuestionChoices
            .Where(i => i.Id == questionChoiceId)
            .Select(i => new
            {
                GameId = i.Question.GameId,
                QuestionId = i.QuestionId,
                QuestionStartTime = i.Question.StartTime,
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
        _pollContext.Answers.Add(new Answer()
        {
            ChoiceId = questionChoiceId,
            PlayerId = playerId,
            QuestionId = questionChoice.QuestionId,
            AnswerTime = now - questionStartTime,
            GameId = questionChoice.GameId,
            Date = now,
        });
    }

    public async Task QuestionTimerEnds()
    {
        await ValidateQuestion();
    }

    public async Task ValidateQuestion()
    {
        var currentGame = await _pollContext.Games.SingleAsync(i => i.IsCurrent);
        if (currentGame.State != GameState.AskingQuestion)
        {
            return;
        }
        
        currentGame.State = GameState.DisplayQuestionResult;
        var currentQuestion = await _pollContext.Games.SingleAsync(i => i.IsCurrent);
        await ComputeScores(currentQuestion.Id);
    }
    
    public async Task MoveToNextQuestion()
    {
        var currentGame = await _pollContext.Games.Include(i=>i.IsCurrent).SingleAsync(i => i.IsCurrent);
        if (currentGame is null || currentGame.State != GameState.DisplayQuestionResult)
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
            currentGame.State = GameState.AskingQuestion;
        }
    }
    
    public async Task ComputeScores(int questionId)
    {
        // Todo : Compute scores
    }
    
    private async Task FinishGame(Game game)
    {
        var questions = await _pollContext.Questions.Where(i => i.GameId == game.Id).ToArrayAsync();
        foreach (var question in questions)
        {
            question.IsCurrent = false;
        }

        game.State = GameState.Completed;
    }
}