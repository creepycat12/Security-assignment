using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using api.Entities;
using api.ViewModels;
using Ganss.Xss;
using Microsoft.EntityFrameworkCore;

namespace api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController(SignInManager<User> signInManager) : ControllerBase
{
    private readonly HtmlSanitizer _htmlSanitizer = new();

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginViewModel model)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        model.Email = _htmlSanitizer.Sanitize(model.Email);
        model.Password = _htmlSanitizer.Sanitize(model.Password);

        ModelState.Clear();
        TryValidateModel(model);

        if (!ModelState.IsValid) return ValidationProblem();

        // Try to sign the user in (this will issue the auth cookie if valid)
        var result = await signInManager.PasswordSignInAsync(
            model.Email,
            model.Password,
            isPersistent: false, // session cookie, expires when browser closes
            lockoutOnFailure: false
        );

        if (result.Succeeded) return Ok("Login successful! (cookie issued)");

        return Unauthorized("Invalid login attempt");
    }


    [HttpPost("register")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> RegisterUser([FromBody] UserRegisterViewModel model)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        model.FirstName = _htmlSanitizer.Sanitize(model.FirstName);
        model.LastName = _htmlSanitizer.Sanitize(model.LastName);
        model.Email = _htmlSanitizer.Sanitize(model.Email);

        ModelState.Clear();
        TryValidateModel(model);

        if (!ModelState.IsValid) return ValidationProblem();

        var user = new User
        {
            UserName = model.Email,
            Email = model.Email,
            FirstName = model.FirstName,
            LastName = model.LastName
        };

        var result = await signInManager.UserManager.CreateAsync(user, model.Password);

        if (result.Succeeded) return Ok("User registered successfully!");

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return ValidationProblem();
    }


   [HttpGet("all-users")]
[Authorize(Roles = "Admin")]
public async Task<IActionResult> GetAllUsers()
{
    var users = await signInManager.UserManager.Users
        .Include(u => u.Tasks) // <-- eager load the tasks
        .Select(u => new
        {
            u.Id,
            u.FirstName,
            u.LastName,
            u.Email,
            Tasks = u.Tasks.Select(t => new 
            {
                t.Id,
                t.Task,
                t.DueDate,
                t.Complete
            }).ToList()
        })
        .ToListAsync();

    return Ok(users);
}

    // ADMIN: delete a user
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteUser(string id)
    {
        var user = await signInManager.UserManager.FindByIdAsync(id);
        if (user == null) return NotFound("User not found");

        var result = await signInManager.UserManager.DeleteAsync(user);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return ValidationProblem(ModelState);
        }

        return Ok("User deleted successfully");
    }

    [HttpPost("logout")]
    public async Task<ActionResult> Logout()
    {
        await signInManager.SignOutAsync();
        return Ok("Logged out (cookie cleared)");
    }
}
