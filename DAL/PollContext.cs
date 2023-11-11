using Microsoft.EntityFrameworkCore;
using Poll.DAL.Entities;

namespace Poll.DAL;

public class PollContext : DbContext
{
    public DbSet<Question> Questions { get; set; }
    public DbSet<QuestionChoice> QuestionChoices { get; set; }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(@"Host=localhost;Username=admin;Password=password;Database=Poll");
        
        base.OnConfiguring(optionsBuilder);
    }
}