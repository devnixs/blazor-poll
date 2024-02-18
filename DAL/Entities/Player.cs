using System.ComponentModel.DataAnnotations;

namespace Poll.DAL.Entities;

public class Player
{
    public Guid Id { get; set; }

    [Required]
    public required string Name { get; set; }
    
    public int Score { get; set; }
    
    public DateTimeOffset HeartBeat { get; set; }
    
    public override string ToString()
    {
        return $"{Name}";
    }
}