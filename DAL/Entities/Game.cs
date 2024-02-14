﻿using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Poll.DAL.Entities;

[Index(nameof(StartDate))]
public class Game
{
    [Key]
    public int Id { get; set; }
    
    public DateTimeOffset StartDate { get; set; }

    public string Name { get; set; }
    
    public GameState State { get; set; }
    public bool IsCurrent { get; set; }
    
    public ICollection<Question> Questions { get; set; }
    public ICollection<Player> Players { get; set; }
    
    public override string ToString()
    {
        return $"{Name}";
    }
}

public enum GameState
{
    InPreparation = 1,
    WaitingForPlayers = 2,
    AskingQuestion = 3,
    DisplayQuestionResult = 4,
    Completed = 5,
}