using Microsoft.EntityFrameworkCore;
using Poll.DAL.Entities;
#pragma warning disable CS8618

namespace Poll.DAL;

public class PollContext : DbContext
{
    public DbSet<Question> Questions { get; set; }
    public DbSet<QuestionChoice> QuestionChoices { get; set; }
    public DbSet<Game> Games { get; set; }
    public DbSet<Player> Players { get; set; }
    public DbSet<Answer> Answers { get; set; }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(@"Host=localhost;Username=admin;Password=password;Database=Poll");
        
        base.OnConfiguring(optionsBuilder);
    }
}