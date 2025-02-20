using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using TaskManagerWebsite.Models;

namespace TaskManagerWebsite.Models;

public static class DataSeeder
{
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