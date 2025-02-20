using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TaskManagerDesktop.Models;
using TaskManagerDesktop.Services;
using TaskManagerDesktop.Data;

using Xunit;

namespace TaskManagerDesktop.Tests
{
    public class IdentityServiceTests
    {
        private ServiceProvider _serviceProvider;
        private ApplicationDbContext _dbContext;
        private IdentityService _identityService;

        public IdentityServiceTests()
        {
            _serviceProvider = IdentityService.ConfigureServices(useInMemoryDb: true);
            _dbContext = _serviceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = _serviceProvider.GetRequiredService<UserManager<User>>();
            _identityService = new IdentityService(userManager);
        }

        [Fact]
        public async Task CanRegisterUser()
        {
            // Arrange
            var user = new User { UserName = "testuser", Email = "test@example.com" };

            // Act
            var result = await _identityService.RegisterUserAsync(user, "P@ssword123");

            // Assert
            Assert.True(result.Succeeded);
            Assert.NotNull(await _dbContext.Users.FirstOrDefaultAsync(u => u.UserName == "testuser"));
        }
    }
}