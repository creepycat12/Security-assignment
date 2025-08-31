using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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


    //Admin och användare kan logga in
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginViewModel model)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        model.Email = _htmlSanitizer.Sanitize(model.Email);
        model.Password = _htmlSanitizer.Sanitize(model.Password);

        ModelState.Clear();
        TryValidateModel(model);

        if (!ModelState.IsValid) return ValidationProblem();

        //Logga in användaren med cookie authentication
        var result = await signInManager.PasswordSignInAsync(
            model.Email,
            model.Password,
            isPersistent: false, // Cookien går bort när webbläsaren stängs
            lockoutOnFailure: false
        );

        if (result.Succeeded) return Ok();

        return Unauthorized("Invalid login attempt");
    }

    //Admin kan registrera nya användare
    //Jag har valt att göra denna delen utan roles
    //Admin kan "Promota" en användare till Admin senare
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

    //Admin kan promota en användare till Admin
    [HttpPost("make-admin/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> MakeUserAdmin(string id)
    {
        var user = await signInManager.UserManager.FindByIdAsync(id);
        if (user == null) return NotFound("User not found");

        var result = await signInManager.UserManager.AddToRoleAsync(user, "Admin");
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return ValidationProblem(ModelState);
        }

        return Ok("User promoted to admin");
    }


    //Admin kan se alla användare + deras tasks
    [HttpGet("all-users")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await signInManager.UserManager.Users
            .Include(u => u.Tasks)
            .ToListAsync();

        var userList = new List<GetAllUsersViewModel>();

        foreach (var user in users)
        {
            var roles = await signInManager.UserManager.GetRolesAsync(user);
            userList.Add(new GetAllUsersViewModel
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Roles = roles.ToList(),
                Tasks = user.Tasks.Select(t => new TodoGetViewModel
                {
                    Task = t.Task,
                    DueDate = t.DueDate,
                    Complete = t.Complete
                }).ToList()
            });
        }

        return Ok(userList);
    }

    // Admin kan ta bort en användare
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

    //Admin och användare kan logga ut 
    [HttpPost("logout")]
    public async Task<ActionResult> Logout()
    {
        await signInManager.SignOutAsync();
        return Ok("Logged out (cookie cleared)");
    }
}
