using Microsoft.AspNetCore.Identity;

namespace TaskManagerWebsite.Models;

/// <summary>
/// Provides methods for seeding default roles and an administrative user into the database.
/// </summary>
public static class DataSeeder
{
    /// <summary>
    /// Seeds predefined roles and a default admin user into the database if they do not already exist.
    /// </summary>
    /// <param name="serviceProvider">The service provider used to resolve dependencies.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public static async Task SeedRolesAndAdminAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<User>>();

        string[] roles = { "Admin", "Manager", "Employee" };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole<int>(role));
            }
        }

        string adminEmail = "manager@gmail.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser == null)
        {
            var newAdmin = new User
            {
                UserName = "Manager",
                Email = adminEmail,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(newAdmin, "Manager1!");

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(newAdmin, "Manager");
            }
        }
        else
        {
            await userManager.AddToRoleAsync(adminUser, "Manager");
        }
    }
}