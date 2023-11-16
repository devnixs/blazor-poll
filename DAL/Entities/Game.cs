using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Poll.DAL.Entities;

[Index(nameof(StartDate))]
public class Game
{
    [Key]
    public int Id { get; set; }
    
    public DateTimeOffset StartDate { get; set; }
}