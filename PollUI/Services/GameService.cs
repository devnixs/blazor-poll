using MoreLinq;
using Poll.DAL.Entities;

namespace Poll.Services;

public class GameService
{
    private readonly GameStateAccessor _gameStateAccessor;
    private readonly ILogger<GameService> _logger;

    public GameService(
        GameStateAccessor gameStateAccessor,
        ILogger<GameService> logger
        )
    {
        _gameStateAccessor = gameStateAccessor;
        _logger = logger;
    }

    public Answer? PlayerSelectsAnswer(Guid gameId, Guid playerId, int questionChoiceId)
    {
        var game = _gameStateAccessor.GetGame(gameId);
        if (game?.CurrentQuestion is null)
        {
            _logger.LogWarning("Tried to selected answer but either game or question is undefined, {game}, {currentQuestion}", game, game?.CurrentQuestion);
            return null;
        }

        if (!game.QuestionStartTime.HasValue)
        {
            _logger.LogWarning("Question didn't have a start time, {game}, {currentQuestion}", game, game?.CurrentQuestion);
            return null;
        }

        var questionChoice = game.CurrentQuestion.Choices
            .Single(i => i.Id == questionChoiceId);
        
        var now = DateTimeOffset.UtcNow;
        
        var answer = new Answer()
        {
            ChoiceId = questionChoiceId,
            PlayerId = playerId,
            QuestionId = questionChoice.QuestionId,
            AnswerTime = now - game.QuestionStartTime.Value,
            GameId = gameId,
            Date = now,
            IsValid = questionChoice.IsValid,
        };
        game.AddAnswer(answer);

        return answer;
    }
    
    public void StartGame(Guid gameId)
    {
        var game = _gameStateAccessor.GetGame(gameId);
        if (game is null)
        {
            return;
        }

        var players = game.Players;
        players.ForEach(i=>i.Score = 0);
        game.SetState(GameStatus.AskingQuestion);
        
        
        var firstQuestion = game.Questions.OrderBy(i => i.Index).First();
        game.SetCurrentQuestion(firstQuestion);
    }
    
    public Task<string?> ValidateGame(int templateId)
    {
        var template = new GameTemplate();
        if (template.Questions.Count == 0)
        {
            return Task.FromResult("Il faut au moins une question")!;
        }
        
        // Ensure all questions are valid
        for (var i = 1; i <= template.Questions.ToArray().Length; i++)
        {
            var question = template.Questions.ToArray()[i-1];
            if (question.Choices.Count > 4)
            {
                return Task.FromResult($"Question {i} doit avoir au moins une réponse")!;
            }
            if (question.Choices.Count > 4)
            {
                return Task.FromResult($"Question {i} doit avoir au maximum 4 réponses")!;
            }
            if (!question.Choices.Any(c=>c.IsValid))
            {
                return Task.FromResult($"Question {i} doit avoir une réponse valide")!;
            }
        }

        return Task.FromResult<string?>(null);
    }
    
    public void ValidateQuestion(Guid gameId)
    {
        var game = _gameStateAccessor.GetGame(gameId);
        if (game is null)
        {
            return;
        }
        
        if (game.Status != GameStatus.AskingQuestion)
        {
            return;
        }
        
        game.SetState(GameStatus.DisplayQuestionResult);
        ComputeScores(game);
        game.OnStateChanged();
    }
    
    public void MoveToNextQuestion(Guid gameId)
    {
        var game = _gameStateAccessor.GetGame(gameId);
        if (game is null)
        {
            _logger.LogWarning("Could not find game {}", gameId);
            return;
        }

        var current = game.CurrentQuestion;
        if (current is null)
        {
            var first = game.Questions.OrderBy(i => i.Index).First();
            game.SetCurrentQuestion(first);
        }
        else
        {
            var currentIndex = game.Questions.FindIndex(i => i.Id == current.Id);

            if (!currentIndex.HasValue || currentIndex == game.Questions.Length - 1)
            {
                FinishGame(game.Id);
            }
            else
            {
                var next = game.Questions.ElementAt(currentIndex.Value + 1);
                game.SetCurrentQuestion(next);
            }
        }
    }
    
    public void ComputeScores(GameState game)
    {
        var answers = game.Answers.ToArray();
        var players = game.Players.ToArray();
        if (answers.Length == 0)
        {
            return;
        }
        
        var maxTime = answers.Max(i => i.AnswerTime);
        var totalSeconds = maxTime.TotalSeconds > 0 ? maxTime.TotalSeconds : 1;

        foreach (var answer in answers)
        {
            if (answer.IsValid)
            {
                answer.Score = (int)Math.Floor(100d + 20d * (1d - answer.AnswerTime.TotalSeconds / totalSeconds));
                var player = players.SingleOrDefault(i => i.Id == answer.PlayerId);
                if (player is not null)
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
    
    private void FinishGame(Guid gameId)
    {
        _logger.LogInformation("Game {gameId} finished", gameId);
        var game = _gameStateAccessor.GetGame(gameId);
        game?.SetCurrentQuestion(null);
        game?.SetState(GameStatus.Completed);
    }
}