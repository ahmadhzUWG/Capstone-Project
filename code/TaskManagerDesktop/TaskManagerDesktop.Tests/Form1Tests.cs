using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Moq;
using Xunit;
using Microsoft.EntityFrameworkCore;
using TaskManagerDesktop.Data;

using TaskManagerDesktop;
using TaskManagerDesktop.Models;

public class Form1Tests
{


    private async Task<ApplicationDbContext> GetMockDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb")
            .Options;

        var dbContext = new ApplicationDbContext(options);

        // Create a fake RoleManager (optional, if you use roles)
        var roleStore = new RoleStore<IdentityRole<int>, ApplicationDbContext, int>(dbContext);
        var roleManager = new RoleManager<IdentityRole<int>>(roleStore, null, null, null, null);

        // Create a fake UserManager
        var userStore = new UserStore<User, IdentityRole<int>, ApplicationDbContext, int>(dbContext);
        var userManager = new UserManager<User>(userStore, null, null, null, null, null, null, null, null);

        // Seed users
        var users = new List<User>
        {
            new User { Id = 1, UserName = "Alice", Email = "alice@example.com", Role = "Admin" },
            new User { Id = 2, UserName = "Bob", Email = "bob@example.com", Role = "User" }
        };

        foreach (var user in users)
        {
            await userManager.CreateAsync(user);
        }

        await dbContext.SaveChangesAsync();
        return dbContext;
    }

    [Fact]
    public void UserConstructor()
    {
        var user = new User
        {
            Id = 1,
            UserName = "Alice",
            Email = "@gmail.com",
            Role = "Admin"
        };

        var role = user.Role;

        Assert.Equal("Admin", role);
        Debug.Assert(user != null, nameof(user) + " != null");
        Assert.NotNull(user.Id);
        Assert.NotNull(user.UserName);
        Assert.NotNull(user.Email);
        Assert.NotNull(user.Role);
    }

    [Fact]
    public async Task GetUsersAsync_ReturnsExpectedUsers()
    {
        // Arrange
        var dbContext = await GetMockDbContext();
        var form = new Form1(dbContext);

        // Act
        var users = await form.GetUsersAsync();

        // Assert
        Assert.Equal(2, users.Count);
        Assert.Contains(users, u => u.UserName == "Alice" && u.Role == "Admin");
        Assert.Contains(users, u => u.UserName == "Bob" && u.Role == "User");
    }

    [Fact]
    public async Task GetUsersAsync_ReturnsEmptyList_WhenNoUsersExist()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "EmptyDb")
            .Options;
        var emptyDbContext = new ApplicationDbContext(options);
        var form = new Form1(emptyDbContext);

        // Act
        var users = await form.GetUsersAsync();

        // Assert
        Assert.Empty(users);
    }
}