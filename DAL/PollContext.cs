﻿using Microsoft.EntityFrameworkCore;
using Poll.DAL.Entities;
#pragma warning disable CS8618

namespace Poll.DAL;

public class PollContext : DbContext
{
    public DbSet<Question> Questions { get; set; }
    public DbSet<GameTemplate> GameTemplates { get; set; }

    
    public PollContext(DbContextOptions<PollContext> options) : base(options)
    {
        
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(@"Host=localhost;Username=admin;Password=password;Database=Poll");
        
        base.OnConfiguring(optionsBuilder);
    }
}