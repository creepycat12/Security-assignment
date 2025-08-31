using api.Entities;
using Microsoft.AspNetCore.Identity;

namespace api.Data;

public class InitializeDb
{
    public static async Task SeedData(
        AppDbContext context,
        UserManager<User> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        // 1. skapar roles om inte de finns
        var roles = new[] { "Admin", "User" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // 2. Skapar en default admin användare 
        var adminEmail = "olgaadmin@gmail.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser == null)
        {
            adminUser = new User
            {
                UserName = adminEmail,
                Email = adminEmail,
                FirstName = "Olga",
                LastName = "Babkina"
            };

            var result = await userManager.CreateAsync(adminUser, "AdminPassword123!");

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
            else
            {
                throw new Exception("Failed to create admin user");
            }
        }

        // 3. Skapar några vanliga användare
        if (!userManager.Users.Any(u => u.Email != adminEmail)) 
        {
            var users = new List<User>
            {
                new() { UserName = "tatiana@gmail.com", Email = "tatiana@gmail.com", FirstName = "Tatiana", LastName = "Lapina" },
                new() { UserName = "eddie@gmail.com", Email = "eddie@gmail.com", FirstName = "Eddie", LastName = "Lu" },
                new() { UserName = "dmitri@gmail.com", Email = "dmitri@gmail.com", FirstName = "Dmitri", LastName = "Babkin" }
            };

            //De ska alla få samma lösenord och roll "User"
            foreach (var user in users)
            {
                var result = await userManager.CreateAsync(user, "Password123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "User");
                }
            }
        }

        // 4. Skapar några todo items för varje användare
        if (!context.TodoItems.Any())
        {
            var users = userManager.Users.Where(u => u.Email != adminEmail).ToList();
            var todoItems = new List<TodoItem>();

            foreach (var user in users)
            {
                todoItems.AddRange(new List<TodoItem>
                {
                    new() { Task = $"Task 1 for {user.FirstName}", DueDate = DateTime.Now.AddDays(7), Complete = false, UserId = user.Id },
                    new() { Task = $"Task 2 for {user.FirstName}", DueDate = DateTime.Now.AddDays(14), Complete = false, UserId = user.Id }
                });
            }

            context.TodoItems.AddRange(todoItems);
            await context.SaveChangesAsync();
        }
    }
}
