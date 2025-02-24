using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TaskManager.Tests;
using TaskManagerWebsite.Controllers;
using TaskManagerWebsite.Data;
using TaskManagerWebsite.Models;
using TaskManagerWebsite.ViewModels;

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

        var mockRoles = new List<IdentityRole<int>>
        {
            new IdentityRole<int> { Id = 1, Name = "Admin" },
            new IdentityRole<int> { Id = 2, Name = "Manager" },
            new IdentityRole<int> { Id = 3, Name = "Employee" }
        }.AsQueryable();

        _mockRoleManager
            .Setup(rm => rm.Roles)
            .Returns(mockRoles);

        _mockUserManager
            .Setup(um => um.GetRolesAsync(It.IsAny<User>()))
            .ReturnsAsync(new List<string> { "Admin", "Manager", "Employee" });
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
    public void Register_ReturnsViewResult()
    {
        var dbContext = TestHelper.GetDbContext();
        _mockUserManager.Setup(um => um.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        var controller = new AdminController(dbContext, _mockUserManager.Object, _mockRoleManager.Object);

        var result = controller.UserAdd();

        Assert.NotNull(result);
        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public async Task RegisterWithModelArg_ReturnsViewResult_WithCreateUserModelStateError()
    {
        var dbContext = TestHelper.GetDbContext();
        _mockUserManager.Setup(um => um.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        var controller = new AdminController(dbContext, _mockUserManager.Object, _mockRoleManager.Object);

        var model = new AdminViewModel
        {
            ConfirmPassword = "password",
            Email = "test@gmail.com",
            Password = "password",
            UserName = ""
        };

        controller.ModelState.AddModelError("UserName", "UserName is required");
        var result = await controller.UserAdd(model);

        Assert.NotNull(result);
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.False(controller.ModelState.IsValid);
        Assert.IsType<AdminViewModel>(viewResult.Model);
    }

    [Fact]
    public async Task RegisterWithModelArg_ReturnsRedirectToActionResult()
    {
        var dbContext = TestHelper.GetDbContext();
        _mockUserManager.Setup(um => um.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);
        _mockUserManager.Setup(um => um.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        var controller = new AdminController(dbContext, _mockUserManager.Object, _mockRoleManager.Object);

        var model = new AdminViewModel
        {
            ConfirmPassword = "password",
            Email = "test@gmail.com",
            Password = "password",
            UserName = "test"
        };

        var result = await controller.UserAdd(model);

        Assert.NotNull(result);
        var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Users", redirectToActionResult.ActionName);
    }

    [Fact]
    public async Task RegisterWithModelArg_ReturnsViewResult_WithFailedUserResult()
    {
        var dbContext = TestHelper.GetDbContext();
        _mockUserManager.Setup(um => um.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError
            {
                Code = "DuplicateUserName",
                Description = "The username is already taken."
            }));

        var controller = new AdminController(dbContext, _mockUserManager.Object, _mockRoleManager.Object);

        var model = new AdminViewModel
        {
            ConfirmPassword = "password",
            Email = "test@gmail.com",
            Password = "password",
            UserName = "test"
        };

        var result = await controller.UserAdd(model);

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(model, viewResult.Model);
        Assert.True(viewResult.ViewData.ModelState.ContainsKey(""));

        var error = viewResult.ViewData.ModelState[""].Errors[0].ErrorMessage;
        Assert.Equal("The username is already taken.", error);
    }

    [Fact]
    public async Task RegisterWithModelArg_ReturnsViewResult_WithFailedUserRoleResult()
    {
        var dbContext = TestHelper.GetDbContext();
        _mockUserManager.Setup(um => um.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);
        _mockUserManager.Setup(um => um.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError
            {
                Code = "RoleNotFound",
                Description = "The role does not exist."
            }));

        var controller = new AdminController(dbContext, _mockUserManager.Object, _mockRoleManager.Object);

        var model = new AdminViewModel
        {
            ConfirmPassword = "password",
            Email = "test@gmail.com",
            Password = "password",
            UserName = "test"
        };

        var result = await controller.UserAdd(model);

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(model, viewResult.Model);
        Assert.True(viewResult.ViewData.ModelState.ContainsKey(""));

        var error = viewResult.ViewData.ModelState[""].Errors[0].ErrorMessage;
        Assert.Equal("The role does not exist.", error);
    }

    [Fact]
    public async Task Edit_ReturnsAViewResult_WithAUser()
    {

        var controller = new AdminController(_dbContext, _mockUserManager.Object, _mockRoleManager.Object);

        var user = new User { Id = 1, UserName = "User1", Email = "user1@example.com" };

        _mockUserManager
            .Setup(um => um.FindByIdAsync("1"))
            .ReturnsAsync(user);

        var result = await controller.UserEdit("1");

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<User>(viewResult.Model);
        Assert.Equal("User1", model.UserName);
    }

    [Fact]
    public async Task Edit_ReturnsNotFoundResult_WithNotFoundUser()
    {
        var controller = new AdminController(_dbContext, _mockUserManager.Object, _mockRoleManager.Object);

        var result = await controller.UserEdit("999");

        Assert.NotNull(result);
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task EditWithUserArg_ReturnsRedirectToActionResult_WithAUser()
    {
        var user = new User { Id = 1, UserName = "User1", Email = "user1@example.com" };
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();
        var controller = new AdminController(_dbContext, _mockUserManager.Object, _mockRoleManager.Object);

        _mockUserManager
            .Setup(um => um.FindByIdAsync("1"))
            .ReturnsAsync(user);

        // Convert ID to string and provide the required Email parameter
        var result = await controller.UserEdit("1", "EditedUser1", "user1@example.com", "Role");

        var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Users", redirectToActionResult.ActionName);
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
