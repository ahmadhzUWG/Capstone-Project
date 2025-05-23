﻿using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace TaskManagerData.Models;

/// <summary>
/// Provides methods for seeding default roles, users, groups, and projects into the database.
/// </summary>
public static class DataSeeder
{
    /// <summary>
    /// Seeds predefined roles, users, groups, and projects into the database if they do not already exist.
    /// </summary>
    /// <param name="serviceProvider">The service provider used to resolve dependencies.</param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// </returns>
    public static async System.Threading.Tasks.Task SeedRolesAndUsersAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<User>>();
        var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

        string[] roles = { "Admin", "Employee" };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole<int>(role));
            }
        }

        var testUsers = new List<(string UserName, string Email, string Role, string Password)>
        {
            ("Admin", "admin@gmail.com", "Admin", "Admin1!"),
            ("Manager1", "manager1@gmail.com", "Employee", "User1!"),
            ("Manager2", "manager2@gmail.com", "Employee", "User1!"),
            ("Employee1", "employee1@gmail.com", "Employee", "User1!"),
            ("Employee2", "employee2@gmail.com", "Employee", "User1!"),
            ("Employee3", "employee3@gmail.com", "Employee", "User1!"),
        };

        foreach (var (userName, email, role, password) in testUsers)
        {
            var existingUser = await userManager.FindByEmailAsync(email);
            if (existingUser == null)
            {
                var newUser = new User
                {
                    UserName = userName,
                    Email = email,
                    EmailConfirmed = true
                };

                var createUserResult = await userManager.CreateAsync(newUser, password);
                if (createUserResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(newUser, role);
                }

                if (role.Equals("Admin"))
                {
                    var admin = new Admin
                    {
                        UserId = newUser.Id
                    };

                    context.Admins.Add(admin);
                    await context.SaveChangesAsync();
                }
            }
            else
            {
                if (!await userManager.IsInRoleAsync(existingUser, role))
                {
                    await userManager.AddToRoleAsync(existingUser, role);
                }

                if (role.Equals("Admin") && !context.Admins.Any(a => a.UserId == existingUser.Id))
                {
                    var admin = new Admin
                    {
                        UserId = existingUser.Id
                    };

                    context.Admins.Add(admin);
                    await context.SaveChangesAsync();
                }
            }
        }

        if (!context.Groups.Any())
        {
            var groups = new List<Group>
            {
                new() { Name = "Development Team", Description = "We develop really good stuff", ManagerId = context.Users.First(user => user.UserName == "Manager1").Id},
                new() { Name = "Design Team", Description = "We design really good stuff", ManagerId = context.Users.First(user => user.UserName == "Manager2").Id},
                new() { Name = "QA Team", Description = "We test really good stuff", ManagerId = context.Users.First(user => user.UserName == "Manager1").Id}
            };

            context.Groups.AddRange(groups);
            await context.SaveChangesAsync();

            var userGroups = new List<UserGroup>
            {
                new() { GroupId = context.Groups.First(g => g.Name == "Development Team").Id, UserId = context.Users.First(user => user.UserName == "Manager1").Id, Role = "Manager"},
                new() { GroupId = context.Groups.First(g => g.Name == "Design Team").Id, UserId = context.Users.First(user => user.UserName == "Manager2").Id, Role = "Manager" },
                new() { GroupId = context.Groups.First(g => g.Name == "QA Team").Id, UserId = context.Users.First(user => user.UserName == "Manager1").Id, Role = "Manager" }
            };

            context.UserGroups.AddRange(userGroups);
            await context.SaveChangesAsync();

            var groupManagers = new List<GroupManager>
            {
                new() {GroupId = context.Groups.First(g => g.Name == "Development Team").Id, UserId = context.Users.First(user => user.UserName == "Manager1").Id},
                new() {GroupId = context.Groups.First(g => g.Name == "Design Team").Id, UserId = context.Users.First(user => user.UserName == "Manager2").Id},
                new() {GroupId = context.Groups.First(g => g.Name == "QA Team").Id, UserId = context.Users.First(user => user.UserName == "Manager1").Id}
            };

            context.GroupManagers.AddRange(groupManagers);
            await context.SaveChangesAsync();
        }

        if (!context.Projects.Any())
        {
            var adminUser = await userManager.FindByEmailAsync("admin@gmail.com");
            if (adminUser != null)
            {
                var projects = new List<Project>
                {
                    new()
                    {
                        Name = "Website Revamp", Description = "We revamp really good stuff",
                        ProjectLeadId = adminUser.Id
                    },
                    new()
                    {
                        Name = "Mobile App Launch", Description = "We launch really good stuff",
                        ProjectLeadId = adminUser.Id
                    }
                };

                context.Projects.AddRange(projects);
                await context.SaveChangesAsync();
            }
        }
    }
}