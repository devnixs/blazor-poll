using MoreLinq;
using Poll.DAL;
using Poll.DAL.Entities;
using Poll.DAL.Services;

namespace Poll.Services;

public class GameService
{
    private readonly GameStateAccessor _gameStateAccessor;
    private readonly ILogger<GameService> _logger;
    private readonly DatabaseWriteContextProvider _databaseWriteContextProvider;

    public GameService(
        GameStateAccessor gameStateAccessor,
        ILogger<GameService> logger,
        DatabaseWriteContextProvider databaseWriteContextProvider
    )
    {
        _gameStateAccessor = gameStateAccessor;
        _logger = logger;
        _databaseWriteContextProvider = databaseWriteContextProvider;
    }

    public Answer? PlayerSelectsAnswer(Guid gameId, Guid playerId, int questionChoiceId)
    {
        var game = _gameStateAccessor.GetGame(gameId);
        if (game?.CurrentQuestion is null)
        {
            _logger.LogWarning(
                "Tried to selected answer but either game or question is undefined, {game}, {currentQuestion}", game,
                game?.CurrentQuestion);
            return null;
        }

        if (!game.QuestionStartTime.HasValue)
        {
            _logger.LogWarning("Question didn't have a start time, {game}, {currentQuestion}", game,
                game?.CurrentQuestion);
            return null;
        }

        if (game.Status != GameStatus.AskingQuestion)
        {
            _logger.LogWarning("Bad state to select answer, {status}", game.Status);
            return null;
        }


        var questionChoice = game.CurrentQuestion.Choices
            .Single(i => i.Id == questionChoiceId);

        var now = DateTimeOffset.UtcNow;

        var player = game.Players.SingleOrDefault(i => i.Id == playerId);
        if (player is not null)
        {
            player.HasAnswered = true;
        }

        var answer = new Answer()
        {
            ChoiceId = questionChoiceId,
            Choice = questionChoice,
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
        players.ForEach(i => i.Score = 0);
        game.SetState(GameStatus.AskingQuestion);


        var firstQuestion = game.Questions.OrderBy(i => i.Index).First();
        game.SetCurrentQuestion(firstQuestion);
    }


    public void SkipAnswerCountdown(Guid gameId)
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

        game.SkipAnswerCountdown();
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

    public async Task MoveToNextQuestion(Guid gameId)
    {
        var game = _gameStateAccessor.GetGame(gameId);
        if (game is null)
        {
            _logger.LogWarning("Could not find game {}", gameId);
            return;
        }

        foreach (var player in game.Players)
        {
            player.LastQuestionSuccess = false;
            player.HasAnswered = false;
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
                await FinishGame(game.Id);
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

        var scoreMultiplier = 1;
        if (game.CurrentQuestion is not null && game.CurrentQuestion.QuestionDoesNotHaveRewards)
        {
            scoreMultiplier = 0;
        }

        foreach (var player in players)
        {
            player.LastQuestionSuccess = false;
            player.HasAnswered = false;
        }

        if (answers.Length == 0)
        {
            return;
        }

        var validAnswers = answers.Where(i => i.IsValid).OrderBy(i => i.AnswerTime).ToArray();

        var winnerPoolBonus = game.Players.Count() * 6;
        var winnerPoolValuePerPlayer = validAnswers.Length > 0 ? (int) Math.Round((decimal) winnerPoolBonus / validAnswers.Length) : 0;
        foreach (var answer in answers)
        {
            if (answer.IsValid)
            {
                var index = validAnswers.FindIndex(i => i == answer);
                int bonus;

                // Si l'option "Pas de bonus de rapidité" est activée, on n'applique pas le bonus
                if (game.Template.NoSpeedBonus)
                {
                    bonus = 0;
                }
                else if (index is null)
                {
                    bonus = 0;
                }
                else if (validAnswers.Length == 1)
                {
                    bonus = 20;
                }
                else
                {
                    bonus = (int) Math.Floor(20 * (1 - ((double) index.Value / (validAnswers.Length - 1))));
                }

                answer.Score = (100 + bonus);

                if (answer.IsValid)
                {
                    // Add a bonus shared across all the winners, to reward answers when few people won.
                    answer.Score += winnerPoolValuePerPlayer;
                }

                answer.Score *= scoreMultiplier;

                var player = players.SingleOrDefault(i => i.Id == answer.PlayerId);
                if (player is not null)
                {
                    player.Score += answer.Score;
                    player.LastQuestionSuccess = true;
                }
            }
            else
            {
                answer.Score = 0 * scoreMultiplier;
                var player = players.SingleOrDefault(i => i.Id == answer.PlayerId);
                if (player is not null)
                {
                    player.LastQuestionSuccess = false;
                }
            }
        }
    }

    private async Task FinishGame(Guid gameId)
    {
        _logger.LogInformation("Game {gameId} finished", gameId);
        var game = _gameStateAccessor.GetGame(gameId);

        if (game is null)
        {
            _logger.LogWarning("Could not find game {gameId} to finish", gameId);
            return;
        }

        game.SetCurrentQuestion(null);
        game.SetState(GameStatus.Completed);
        try
        {
            await _databaseWriteContextProvider.Write<PollContext, int>(async db =>
            {
                foreach (var player in game.GetAllPlayers())
                {
                    db.RunHistories.Add(new RunHistory()
                    {
                        TemplateId = game.Template.Id,
                        Player = player.Name,
                        Game = game.Template.Name,
                        Points = player.Score,
                    });
                }

                await db.SaveChangesAsync();
                return 0;
            });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to save history");
        }
    }
}