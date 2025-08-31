using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using api.Data;
using api.Entities;
using System.Security.Claims;
using Ganss.Xss;
using api.ViewModels;

namespace api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Alla endpoints behöver autentisering
public class TodoController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly UserManager<User> _userManager;
    private readonly HtmlSanitizer _htmlSanitizer = new();

    public TodoController(AppDbContext context, UserManager<User> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // Admin och användare kan se sina egna uppgifter
    [HttpGet]
    public async Task<IActionResult> GetUserTasks()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // användarens id från token 

        var tasks = await _context.TodoItems
            .Where(t => t.UserId == userId)
            .Select(t => new
            {
                t.Id,
                t.Task,
                t.DueDate,
                t.Complete
            })
            .ToListAsync();

        return Ok(tasks);
    }

    // Admin kan se alla uppgifter för en specifik användare
    [HttpGet("admin-get-tasks/{userId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AdminGetUserTasks(string userId)
    {
        var tasks = await _context.TodoItems
            .Where(t => t.UserId == userId)
            .Select(t => new
            {
                t.Id,
                t.Task,
                t.DueDate,
                t.Complete
            })
            .ToListAsync();

        return Ok(tasks);
    }

    // Admin och användare kan lägga till uppgifter till sig själva
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> AddTask([FromBody] TodoPostViewModel model)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return Unauthorized();

        var sanitizedTask = _htmlSanitizer.Sanitize(model.Task);

        if (string.IsNullOrWhiteSpace(sanitizedTask))
        {
            ModelState.AddModelError(nameof(model.Task), "Task cannot me empty");
            return ValidationProblem(ModelState);
        }
        var task = new TodoItem
        {
            Task = sanitizedTask,
            DueDate = model.DueDate ?? DateTime.UtcNow.AddDays(7), //lägger till 7 dagar som standard ifall ingen förfallodatum anges
            Complete = false, //nya tasks är inte kompletta
            UserId = userId
        };

        _context.TodoItems.Add(task);
        await _context.SaveChangesAsync();

        //Fick göra såhär för jag var fast i en json loop annars
        return Ok(new
        {
            task.Id,
            task.Task,
            task.DueDate,
            task.Complete,
            User = new { user.Id, user.Email }
        });
    }

    // Admin kan tilldela uppgifter till andra användare
    [HttpPost("assign")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AssignTask([FromBody] AssignTaskViewModel model)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null) return NotFound("User not found");

        var sanitizedTask = _htmlSanitizer.Sanitize(model.Task);

        var task = new TodoItem
        {
            Task = sanitizedTask,
            DueDate = model.DueDate ?? DateTime.UtcNow.AddDays(7),
            Complete = false,
            UserId = user.Id
        };

        _context.TodoItems.Add(task);
        await _context.SaveChangesAsync();
        return Ok(task);
    }

    //Admin och användare kan uppdatera sina egna uppgifter
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTask(string id, [FromBody] TodoPutViewModel updatedTask)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var task = await _context.TodoItems
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

        if (task == null) return NotFound();

        task.Task = _htmlSanitizer.Sanitize(updatedTask.Task);
        task.DueDate = updatedTask.DueDate ?? DateTime.UtcNow.AddDays(7);
        task.Complete = updatedTask.Complete;

        await _context.SaveChangesAsync();
        return Ok();
    }

    // Admin kan uppdatera uppgifter för en annan användare
    [HttpPut("admin-update/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AdminUpdateTask(string id, [FromBody] TodoPutViewModel updatedTask)
    {
        var task = await _context.TodoItems.FirstOrDefaultAsync(t => t.Id == id);
        if (task == null) return NotFound();

        task.Task = updatedTask.Task;
        task.DueDate = updatedTask.DueDate ?? DateTime.UtcNow.AddDays(7);
        task.Complete = updatedTask.Complete;

        await _context.SaveChangesAsync();
        return Ok();
    }


    // Admin och användare kan ta bort sina egna uppgifter
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTask(string id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var task = await _context.TodoItems
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

        if (task == null) return NotFound();

        _context.TodoItems.Remove(task);
        await _context.SaveChangesAsync();

        return Ok("Task deleted successfully");
    }

    // Admin kan ta bort tasks för andra användare
    [HttpDelete("admin-remove/{taskId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AdminRemoveTask(string taskId)
    {
        var task = await _context.TodoItems.FindAsync(taskId);
        if (task == null) return NotFound("Task not found");

        _context.TodoItems.Remove(task);
        await _context.SaveChangesAsync();

        return Ok();
    }
}
