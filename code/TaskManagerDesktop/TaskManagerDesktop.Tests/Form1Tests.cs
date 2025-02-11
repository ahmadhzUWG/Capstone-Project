using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using Xunit;
using Microsoft.EntityFrameworkCore;
using TaskManagerWebsite.Data;
using TaskManagerWebsite.Models;
using TaskManagerDesktop;

public class Form1Tests
{
    private async Task<ApplicationDbContext> GetMockDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb")
            .Options;

        var dbContext = new ApplicationDbContext(options);

        dbContext.Users.AddRange(new List<User>
        {
            new User { Id = 1, UserName = "Alice", Email = "alice@example.com", Role = "Admin" },
            new User { Id = 2, UserName = "Bob", Email = "bob@example.com", Role = "User" }
        });

        await dbContext.SaveChangesAsync();
        return dbContext;
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