using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Poll.DAL.Entities;

public class Answer
{
    [Key]
    public int Id { get; set; }

    public string Name { get; set; }
    public DateTimeOffset Date { get; set; }

    public int QuestionId { get; set; }

    [ForeignKey(nameof(QuestionId))]
    public Question Question { get; set; }
}