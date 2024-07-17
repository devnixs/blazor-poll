using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Poll.DAL.Entities;

public class GameFile
{
    public Guid Id { get; set; }
    public DateTimeOffset CreationDate { get; set; }
    
    [StringLength(512)]
    public string Path { get; set; } = "";
    
    [StringLength(512)]
    public string OriginalFilename { get; set; } = "";
    public long Size { get; set; }
    
    [StringLength(64)]
    public string ContentType { get; set; } = "";
    
    public int? GameTemplateId { get; set; }
    
    [ForeignKey(nameof(GameTemplateId))]
    public GameTemplate? GameTemplate { get; set; } 
}