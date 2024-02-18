using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Poll.DAL.Entities;

public class Answer
{
    [Key]
    public int Id { get; set; }

    public DateTimeOffset Date { get; set; }
    public TimeSpan AnswerTime { get; set; }
    
    public int Score { get; set; }

    public int QuestionId { get; set; }

    [ForeignKey(nameof(QuestionId))]
    public Question? Question { get; set; }
    
    public int ChoiceId { get; set; }

    [ForeignKey(nameof(QuestionId))]
    public QuestionChoice? Choice { get; set; }
    
    public bool IsValid { get; set; }
    
    public Guid GameId { get; set; }
    
    public Guid PlayerId { get; set; }
    
    [ForeignKey(nameof(PlayerId))]
    public Player? Player { get; set; }
    
    public override string ToString()
    {
        return $"{Player?.Name} -> {Question?.Content} -> {Choice?.Content}";
    }
}