using System.ComponentModel.DataAnnotations;

namespace Poll.DAL.Entities;

public class Player
{
    public int Id { get; set; }

    [Required]
    public required string Name { get; set; }
}