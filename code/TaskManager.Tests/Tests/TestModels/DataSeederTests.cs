using Microsoft.AspNetCore.Identity;
using Moq;
using TaskManagerWebsite.Models;

namespace TaskManager.Tests.Tests.TestModels
{
    public class DataSeederTests
    {
        private readonly Mock<RoleManager<IdentityRole<int>>> _mockRoleManager;
        private readonly Mock<UserManager<User>> _mockUserManager;
        private readonly Mock<IServiceProvider> _mockServiceProvider;

        public DataSeederTests()
        {
            _mockRoleManager = GetMockRoleManager();
            _mockUserManager = GetMockUserManager();
            _mockServiceProvider = new Mock<IServiceProvider>();

            _mockServiceProvider
                .Setup(sp => sp.GetService(typeof(RoleManager<IdentityRole<int>>)))
                .Returns(_mockRoleManager.Object);

            _mockServiceProvider
                .Setup(sp => sp.GetService(typeof(UserManager<User>)))
                .Returns(_mockUserManager.Object);
        }

        private Mock<RoleManager<IdentityRole<int>>> GetMockRoleManager()
        {
            var roleStoreMock = new Mock<IRoleStore<IdentityRole<int>>>();
            return new Mock<RoleManager<IdentityRole<int>>>(roleStoreMock.Object, null, null, null, null);
        }

        private Mock<UserManager<User>> GetMockUserManager()
        {
            var userStoreMock = new Mock<IUserStore<User>>();
            return new Mock<UserManager<User>>(userStoreMock.Object, null, null, null, null, null, null, null, null);
        }

        [Fact]
        public async Task SeedRolesAndAdminAsync_CreatesAdminUser_WhenNotFound()
        {
            _mockUserManager.Setup(um => um.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((User)null);
            _mockUserManager.Setup(um => um.CreateAsync(It.IsAny<User>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);
            _mockUserManager.Setup(um => um.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);

            await DataSeeder.SeedRolesAndUsersAsync(_mockServiceProvider.Object);

            _mockUserManager.Verify(um => um.CreateAsync(It.Is<User>(u => u.Email == "manager@gmail.com"), "Manager1!"), Times.Once);
            _mockUserManager.Verify(um => um.AddToRoleAsync(It.Is<User>(u => u.Email == "manager@gmail.com"), "Manager"), Times.Once);
        }

        [Fact]
        public async Task SeedRolesAndAdminAsync_AddsManagerRole_WhenAdminExists()
        {
            var existingAdmin = new User { Email = "manager@gmail.com" };
            _mockUserManager.Setup(um => um.FindByEmailAsync("manager@gmail.com")).ReturnsAsync(existingAdmin);
            _mockUserManager.Setup(um => um.AddToRoleAsync(existingAdmin, "Manager")).ReturnsAsync(IdentityResult.Success);

            await DataSeeder.SeedRolesAndUsersAsync(_mockServiceProvider.Object);

            _mockUserManager.Verify(um => um.AddToRoleAsync(existingAdmin, "Manager"), Times.Once);
        }
    }
}
