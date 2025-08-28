using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;


namespace api.Entities;

//creating an entity for TodoItem
//using UserId as foreign key to link TodoItems to users
public class TodoItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public required string Task { get; set; }
    public DateTime DueDate { get; set; }
    public bool Complete { get; set; } = false;
    public required string UserId { get; set; }

    public User User { get; set; } 
}
