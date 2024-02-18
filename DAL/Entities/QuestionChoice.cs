using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Poll.DAL.Entities;

public class QuestionChoice
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [StringLength(512)]
    public required string Content { get; set; }

    public int Index { get; set; }

    public int QuestionId { get; set; }
    
    [ForeignKey(nameof(QuestionId))]
    public Question? Question { get; set; }
    
    public bool IsValid { get; set; }
    
    public override string ToString()
    {
        return $"{Content} (#{Index})";
    }
}