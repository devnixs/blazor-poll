using System.ComponentModel.DataAnnotations;

namespace Poll.DAL.Entities;

public class Question
{
    [Key]
    public int Id { get; set; }

    public DateTimeOffset? StartTime { get; set; }
    
    [Required]
    public required string Content { get; set; }
    
    public Game? Game { get; set; }
    public int GameId { get; set; }

    public int Index { get; set; }
    public bool IsCurrent { get; set; }
    
    public ICollection<QuestionChoice> Choices { get; set; }
}