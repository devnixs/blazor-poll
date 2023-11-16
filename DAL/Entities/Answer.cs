using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Poll.DAL.Entities;

public class Answer
{
    [Key]
    public int Id { get; set; }

    public DateTimeOffset Date { get; set; }

    public int QuestionId { get; set; }

    [ForeignKey(nameof(QuestionId))]
    public Question? Question { get; set; }
    
    public int ChoiceId { get; set; }

    [ForeignKey(nameof(QuestionId))]
    public QuestionChoice? Choice { get; set; }
    
    public int GameId { get; set; }
    
    [ForeignKey(nameof(GameId))]
    public Game? Game { get; set; }
    
    public int PlayerId { get; set; }
    
    [ForeignKey(nameof(PlayerId))]
    public Player? Player { get; set; }
}