using System.ComponentModel.DataAnnotations;

namespace Poll.DAL.Entities;

public class RunHistory
{
    public Guid Id { get; set; }
    public int TemplateId { get; set; }

    [MaxLength(255)]
    [Required]
    public string Player { get; set; }

    public int Points { get; set; }
    public DateTimeOffset Date { get; set; } = DateTimeOffset.UtcNow;
    
    [MaxLength(255)]
    public string Game { get; set; }
}