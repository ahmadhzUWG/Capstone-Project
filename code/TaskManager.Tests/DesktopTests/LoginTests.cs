using Microsoft.AspNetCore.Identity;
using Moq;
using TaskManagerData.Models;
using TaskManagerDesktop.ViewModels;
using Task = System.Threading.Tasks.Task;

namespace TaskManager.Tests.DesktopTests;

public class LoginTests
{
    private readonly Mock<UserManager<User>> _mockUserManager;
    private readonly ApplicationDbContext _dbContext;

    public LoginTests()
    {
        _mockUserManager = GetMockUserManager();
        _dbContext = TestHelper.GetDbContext();

        _mockUserManager.Setup(um => um.GetRolesAsync(It.IsAny<User>()))
            .ReturnsAsync(new List<string> { "Admin", "Employee" });
    }

    public Mock<UserManager<User>> GetMockUserManager()
    {
        var userStoreMock = new Mock<IUserStore<User>>();
        return new Mock<UserManager<User>>(userStoreMock.Object,
            null, null, null, null, null, null, null, null);
    }

    [Fact]
    public async Task Login_NullUsername_ReturnsFalse()
    {
        var loginViewModel = new LoginViewModel(_mockUserManager.Object)
        {
            Username = null,
            Password = "password"
        };
        var result = await loginViewModel.Login();
        Assert.False(result);
        Assert.True(loginViewModel.ShowErrorUsername);
    }

    [Fact]
    public async Task Login_NullPassword_ReturnsFalse()
    {
        var loginViewModel = new LoginViewModel(_mockUserManager.Object)
        {
            Username = "username",
            Password = null
        };
        var result = await loginViewModel.Login();
        Assert.False(result);
        Assert.True(loginViewModel.ShowErrorPassword);
    }

    [Fact]
    public async Task Login_NullUser_ReturnsFalse()
    {
        var loginViewModel = new LoginViewModel(_mockUserManager.Object)
        {
            Username = "username",
            Password = "password"
        };
        _mockUserManager.Setup(um => um.FindByNameAsync(It.IsAny<string>()))
            .ReturnsAsync((User)null);
        var result = await loginViewModel.Login();
        Assert.False(result);
        Assert.True(loginViewModel.ShowErrorLogin);
    }

    [Fact]
    public async Task Login_InvalidPassword_ReturnsFalse()
    {
        var loginViewModel = new LoginViewModel(_mockUserManager.Object)
        {
            Username = "username",
            Password = "wrongpassword"
        };
        var user = new User { UserName = "username" };
        _mockUserManager.Setup(um => um.FindByNameAsync(It.IsAny<string>()))
            .ReturnsAsync(user);
        _mockUserManager.Setup(um => um.CheckPasswordAsync(user, It.IsAny<string>()))
            .ReturnsAsync(false);
        var result = await loginViewModel.Login();
        Assert.False(result);
        Assert.True(loginViewModel.ShowErrorLogin);
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsTrue()
    {
        var loginViewModel = new LoginViewModel(_mockUserManager.Object)
        {
            Username = "username",
            Password = "password"
        };
        var user = new User { UserName = "username" };
        _mockUserManager.Setup(um => um.FindByNameAsync(It.IsAny<string>()))
            .ReturnsAsync(user);
        _mockUserManager.Setup(um => um.CheckPasswordAsync(user, It.IsAny<string>()))
            .ReturnsAsync(true);
        var result = await loginViewModel.Login();
        Assert.True(result);
        Assert.False(loginViewModel.ShowErrorLogin);
    }
}