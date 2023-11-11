using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Poll.DAL.Entities;

public class QuestionChoice
{
    [Key]
    public int Id { get; set; }
    
    public string Content { get; set; }

    public int Index { get; set; }

    public int QuestionId { get; set; }
    
    [ForeignKey(nameof(QuestionId))]
    public Question Question { get; set; }
}