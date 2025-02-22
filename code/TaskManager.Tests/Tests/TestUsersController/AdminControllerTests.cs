using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TaskManager.Tests;
using TaskManagerWebsite.Controllers;
using TaskManagerWebsite.Data;
using TaskManagerWebsite.Models;

public class AdminControllerTests
{
    private readonly Mock<UserManager<User>> _mockUserManager;
    private readonly Mock<RoleManager<IdentityRole<int>>> _mockRoleManager;
    private readonly ApplicationDbContext _dbContext;

    public AdminControllerTests()
    {
        _mockUserManager = GetMockUserManager();
        _mockRoleManager = GetMockRoleManager();
        _dbContext = TestHelper.GetDbContext();
    }

    private Mock<UserManager<User>> GetMockUserManager()
    {
        var userStoreMock = new Mock<IUserStore<User>>();
        return new Mock<UserManager<User>>(userStoreMock.Object, null, null, null, null, null, null, null, null);
    }

    private Mock<RoleManager<IdentityRole<int>>> GetMockRoleManager()
    {
        var roleStoreMock = new Mock<IRoleStore<IdentityRole<int>>>();
        return new Mock<RoleManager<IdentityRole<int>>>(roleStoreMock.Object, null, null, null, null);
    }

    [Fact]
    public async Task Edit_ReturnsAViewResult_WithAUser()
    {
        _dbContext.Users.Add(new User { Id = 1, UserName = "User1", Email = "user1@example.com" });
        await _dbContext.SaveChangesAsync();
        var controller = new AdminController(_dbContext, _mockUserManager.Object, _mockRoleManager.Object);

        // Convert ID to string if the Edit method expects a string
        var result = await controller.Edit("1");

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<User>(viewResult.Model);
        Assert.Equal("User1", model.UserName);
    }

    [Fact]
    public async Task EditWithUserArg_ReturnsRedirectToActionResult_WithAUser()
    {
        _dbContext.Users.Add(new User { Id = 1, UserName = "User1", Email = "user1@example.com" });
        await _dbContext.SaveChangesAsync();
        var controller = new AdminController(_dbContext, _mockUserManager.Object, _mockRoleManager.Object);

        // Convert ID to string and provide the required Email parameter
        var result = await controller.UserEdit("1", "EditedUser1", "user1@example.com", "Role");

        var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectToActionResult.ActionName);
    }

    [Fact]
    public async Task EditWithUserArg_ReturnsViewResult_WithValidationError()
    {
        _dbContext.Users.Add(new User { Id = 1, UserName = "User1", Email = "user1@example.com" });
        await _dbContext.SaveChangesAsync();
        var controller = new AdminController(_dbContext, _mockUserManager.Object, _mockRoleManager.Object);

        controller.ModelState.AddModelError("UserName", "UserName is required");

        // Convert ID to string and provide the required Email parameter
        var result = await controller.UserEdit("1", "", "user1@example.com", "Role");

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.False(controller.ModelState.IsValid);
    }

    [Fact]
    public async Task EditWithUserArg_ReturnsNotFoundResult_WithNotFoundUser()
    {
        var controller = new AdminController(_dbContext, _mockUserManager.Object, _mockRoleManager.Object);

        // Convert ID to string and provide the required Email parameter
        var result = await controller.UserEdit("999", "User2", "user2@example.com", "Role");

        Assert.NotNull(result);
        Assert.IsType<NotFoundResult>(result);
    }
}
