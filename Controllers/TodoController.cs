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
[Authorize] // all endpoints require login
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

    // Get tasks for the logged-in user and admin
    [HttpGet]
    public async Task<IActionResult> GetUserTasks()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // get user ID 

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

    // User adds a new task for themselves
    [HttpPost]
    [Authorize]
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> AddTask([FromBody] TodoPostViewModel model)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return Unauthorized();

        // Sanitize input
        var sanitizedTask = _htmlSanitizer.Sanitize(model.Task);

        // Check if sanitized task is empty
        if (string.IsNullOrWhiteSpace(sanitizedTask))
        {
            ModelState.AddModelError(nameof(model.Task), "Task cannot be empty.");
            return ValidationProblem(ModelState);
        }

        var task = new TodoItem
        {
            Task = sanitizedTask,
            DueDate = model.DueDate ?? DateTime.UtcNow.AddDays(7),
            Complete = false,
            UserId = userId
        };

        _context.TodoItems.Add(task);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            task.Id,
            task.Task,
            task.DueDate,
            task.Complete,
            User = new { user.Id, user.Email }
        });
    }
    // For admins to assign tasks to any user
    [HttpPost("assign")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AssignTask([FromBody] AssignTaskViewModel model)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        var user = await _userManager.FindByEmailAsync(model.Username);
        if (user == null) return NotFound("User not found");

        var sanitizedTask = _htmlSanitizer.Sanitize(model.Task);

        var task = new TodoItem
        {
            Task = sanitizedTask,
            DueDate = model.DueDate ?? DateTime.UtcNow.AddDays(7), // default due date like before
            Complete = false,
            UserId = user.Id
        };

        _context.TodoItems.Add(task);
        await _context.SaveChangesAsync();

        return Ok(task);
    }

    // Update a task
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTask(string id, [FromBody] TodoPostViewModel updatedTask)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var task = await _context.TodoItems
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

        if (task == null) return NotFound();

        // sanitize updates
        task.Task = _htmlSanitizer.Sanitize(updatedTask.Task);
        task.DueDate = updatedTask.DueDate?? DateTime.UtcNow.AddDays(7);

        await _context.SaveChangesAsync();
        return Ok();
    }

    // Delete a task
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

    // For admins to delete a task for any user
    [HttpDelete("admin-remove/{taskId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AdminRemoveTask(string taskId)
    {
        var task = await _context.TodoItems.FindAsync(taskId);
        if (task == null) return NotFound("Task not found");

        _context.TodoItems.Remove(task);
        await _context.SaveChangesAsync();

        return Ok($"Task '{task.Task}' (ID: {task.Id}) removed successfully.");
    }
}
