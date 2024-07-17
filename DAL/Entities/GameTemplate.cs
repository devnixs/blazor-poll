using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Poll.DAL.Entities;

[Index(nameof(Identifier))]
public class GameTemplate
{
    [Key]
    public int Id { get; set; }
    
    public string Identifier { get; set; }

    public DateTimeOffset CreationDate { get; set; } = DateTimeOffset.UtcNow;

    [StringLength(256)]
    [Required]
    public string Name { get; set; }

    public ICollection<Question> Questions { get; set; } = new List<Question>();
    
    public ICollection<GameFile> Files { get; set; } = new List<GameFile>();

    public GameTemplate()
    {
        Identifier = Guid.NewGuid().ToString().Substring(0, 6);
        Name = "";
    }

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