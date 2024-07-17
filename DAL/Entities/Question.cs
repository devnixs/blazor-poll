using System.ComponentModel.DataAnnotations;

namespace Poll.DAL.Entities;

public class Question
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [StringLength(512)]
    public required string Content { get; set; }
    
    public GameTemplate? GameTemplate { get; set; }
    public int GameTemplateId { get; set; }

    public int Index { get; set; }

    public ICollection<QuestionChoice> Choices { get; set; } = new List<QuestionChoice>();

    public Guid? AskingQuestionImageId { get; set; }
    
    public Guid? PresentingAnswerImageId { get; set; }

    public override string ToString()
    {
        return $"{Content} (#{Index})";
    }
}