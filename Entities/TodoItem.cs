using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;


namespace api.Entities;

public class TodoItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public required string Task { get; set; }
    public DateTime DueDate { get; set; }
    
    //Tänkar att en ny todo är inte complete när den skapas
    public bool Complete { get; set; } = false;
    public required string UserId { get; set; }

    public User User { get; set; } 
}
