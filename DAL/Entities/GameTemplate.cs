using System.ComponentModel.DataAnnotations;

namespace Poll.DAL.Entities;

public class GameTemplate
{
    [Key]
    public int Id { get; set; }
    
    public DateTimeOffset CreationDate { get; set; }

    public string Name { get; set; }
    
    public ICollection<Question> Questions { get; set; }
    
    public override string ToString()
    {
        return $"{Name}";
    }
}

public enum GameStatus
{
    WaitingForPlayers = 1,
    AskingQuestion = 2,
    DisplayQuestionResult = 3,
    Completed = 4,
}