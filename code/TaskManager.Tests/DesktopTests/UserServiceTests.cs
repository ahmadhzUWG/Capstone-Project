using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using TaskManagerData.Models;
using Task = System.Threading.Tasks.Task; // Replace with actual namespace

namespace TaskManager.Tests.DesktopTests
{
    public class UserServiceTests
    {
        private ApplicationDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Unique DB per test
                .Options;
            return new ApplicationDbContext(options);
        }

        private UserManager<User> GetUserManager(ApplicationDbContext dbContext)
        {
            var userStore = new UserStore<User, IdentityRole<int>, ApplicationDbContext, int>(dbContext);

            var mockOptions = new Mock<IOptions<IdentityOptions>>();
            var mockPasswordHasher = new PasswordHasher<User>();
            var mockUserValidators = new List<IUserValidator<User>> { new UserValidator<User>() };
            var mockPasswordValidators = new List<IPasswordValidator<User>> { new PasswordValidator<User>() };
            var mockLookupNormalizer = new Mock<ILookupNormalizer>();
            var mockIdentityErrorDescriber = new IdentityErrorDescriber();
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockLogger = new Mock<ILogger<UserManager<User>>>();

            // Ensure IdentityOptions are set to default
            mockOptions.Setup(opt => opt.Value).Returns(new IdentityOptions());

            return new UserManager<User>(
                userStore,
                mockOptions.Object,
                mockPasswordHasher,
                mockUserValidators,
                mockPasswordValidators,
                mockLookupNormalizer.Object,
                mockIdentityErrorDescriber,
                mockServiceProvider.Object,
                mockLogger.Object
            );
        }

        [Fact]
        public async Task CanCreateUser()
        {
            // Arrange
            var dbContext = GetDbContext();
            var userManager = GetUserManager(dbContext);

            var user = new User { UserName = "testuser", Email = "test@example.com" };

            // Act
            var result = await userManager.CreateAsync(user, "P@ssword123");

            // Assert
            Assert.True(result.Succeeded);
            Assert.NotNull(await dbContext.Users.FirstOrDefaultAsync(u => u.UserName == "testuser"));
        }
    }
}
