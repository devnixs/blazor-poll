using System.ComponentModel.DataAnnotations;

namespace Poll.DAL.Entities;

public class Question
{
    [Key]
    public int Id { get; set; }

    [Required]
    public required string Content { get; set; }

    public int Index { get; set; }
    public bool IsCurrent { get; set; }
}