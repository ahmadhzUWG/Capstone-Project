using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Moq;
using Xunit;
using TaskManagerWebsite.Models;
using Microsoft.EntityFrameworkCore;
using TaskManagerWebsite.Data;

public class DataSeederTests
{
    private readonly Mock<UserManager<User>> _mockUserManager;
    private readonly Mock<RoleManager<IdentityRole<int>>> _mockRoleManager;
    private readonly Mock<IServiceProvider> _mockServiceProvider;
    private readonly ApplicationDbContext _dbContext;

    public DataSeederTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase") 
            .Options;

        _dbContext = new ApplicationDbContext(options); 

        // Setup a mock IUserStore for UserManager.
        var userStoreMock = new Mock<IUserStore<User>>();
        _mockUserManager = new Mock<UserManager<User>>(
            userStoreMock.Object, null, null, null, null, null, null, null, null);

        // Setup a mock IRoleStore for RoleManager.
        var roleStoreMock = new Mock<IRoleStore<IdentityRole<int>>>();
        _mockRoleManager = new Mock<RoleManager<IdentityRole<int>>>(
            roleStoreMock.Object, null, null, null, null);

        // Configure the service provider to use GetService instead of GetRequiredService.
        _mockServiceProvider = new Mock<IServiceProvider>();
        _mockServiceProvider
            .Setup(sp => sp.GetService(typeof(UserManager<User>)))
            .Returns(_mockUserManager.Object);
        _mockServiceProvider
            .Setup(sp => sp.GetService(typeof(RoleManager<IdentityRole<int>>)))
            .Returns(_mockRoleManager.Object);
        _mockServiceProvider
            .Setup(sp => sp.GetService(typeof(ApplicationDbContext)))
            .Returns(_dbContext);

        // Configure RoleManager so that roles do not exist and are then created.
        _mockRoleManager
            .Setup(rm => rm.RoleExistsAsync(It.IsAny<string>()))
            .ReturnsAsync(false);
        _mockRoleManager
            .Setup(rm => rm.CreateAsync(It.IsAny<IdentityRole<int>>()))
            .ReturnsAsync(IdentityResult.Success);
    }

    [Fact]
    public async Task SeedRolesAndUsersAsync_CreatesManagerUser_WhenNotFound()
    {
        // Arrange: Simulate that no user exists for any of the test emails.
        _mockUserManager.Setup(um => um.FindByEmailAsync("admin@gmail.com"))
            .ReturnsAsync((User)null);
        _mockUserManager.Setup(um => um.FindByEmailAsync("manager1@gmail.com"))
            .ReturnsAsync((User)null);
        _mockUserManager.Setup(um => um.FindByEmailAsync("manager2@gmail.com"))
            .ReturnsAsync((User)null);
        _mockUserManager.Setup(um => um.FindByEmailAsync("employee1@gmail.com"))
            .ReturnsAsync((User)null);
        _mockUserManager.Setup(um => um.FindByEmailAsync("employee2@gmail.com"))
            .ReturnsAsync((User)null);
        _mockUserManager.Setup(um => um.FindByEmailAsync("employee3@gmail.com"))
            .ReturnsAsync((User)null);

        _mockUserManager
            .Setup(um => um.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);
        _mockUserManager
            .Setup(um => um.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        // Act:
        await DataSeeder.SeedRolesAndUsersAsync(_mockServiceProvider.Object);

        // Assert:
        // The seeder creates the manager1 user with email "manager1@gmail.com" using password "User1!"
        // and then adds them to the "Employee" role.
        _mockUserManager.Verify(
            um => um.CreateAsync(
                It.Is<User>(u => u.Email == "manager1@gmail.com"),
                "User1!"),
            Times.Once);
        _mockUserManager.Verify(
            um => um.AddToRoleAsync(
                It.Is<User>(u => u.Email == "manager1@gmail.com"),
                "Employee"),
            Times.Once);
    }

    [Fact]
    public async Task SeedRolesAndUsersAsync_AddsEmployeeRole_WhenUserExists()
    {
        // Arrange: Simulate that a user with "manager1@gmail.com" already exists and is not in the "Employee" role.
        var existingUser = new User { Email = "manager1@gmail.com" };
        _mockUserManager.Setup(um => um.FindByEmailAsync("manager1@gmail.com"))
            .ReturnsAsync(existingUser);
        _mockUserManager.Setup(um => um.IsInRoleAsync(existingUser, "Employee"))
            .ReturnsAsync(false);
        _mockUserManager.Setup(um => um.AddToRoleAsync(existingUser, "Employee"))
            .ReturnsAsync(IdentityResult.Success);

        // For other users, simulate they do not exist.
        _mockUserManager.Setup(um => um.FindByEmailAsync("admin@gmail.com"))
            .ReturnsAsync((User)null);
        _mockUserManager.Setup(um => um.FindByEmailAsync("manager2@gmail.com"))
            .ReturnsAsync((User)null);
        _mockUserManager.Setup(um => um.FindByEmailAsync("employee1@gmail.com"))
            .ReturnsAsync((User)null);
        _mockUserManager.Setup(um => um.FindByEmailAsync("employee2@gmail.com"))
            .ReturnsAsync((User)null);
        _mockUserManager.Setup(um => um.FindByEmailAsync("employee3@gmail.com"))
            .ReturnsAsync((User)null);
        _mockUserManager
            .Setup(um => um.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        // Act:
        await DataSeeder.SeedRolesAndUsersAsync(_mockServiceProvider.Object);

        // Assert:
        _mockUserManager.Verify(
            um => um.AddToRoleAsync(existingUser, "Employee"),
            Times.Once);
    }
}
