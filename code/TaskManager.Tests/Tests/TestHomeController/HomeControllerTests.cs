using Moq;
using Xunit;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TaskManager.Controllers;
using TaskManagerWebsite.Models;
using Microsoft.AspNetCore.Http;
using TaskManagerWebsite.Data;

namespace TaskManager.Tests.Tests.TestHomeController
{
    public class HomeControllerTests
    {
        private readonly Mock<UserManager<User>> _mockUserManager;
        private readonly Mock<SignInManager<User>> _mockSignInManager;
        private readonly Mock<ApplicationDbContext> _mockDbContext;
        private readonly Mock<ILogger<HomeController>> _mockLogger;

        public HomeControllerTests()
        {
            _mockUserManager = new Mock<UserManager<User>>(
                new Mock<IUserStore<User>>().Object,
                null, null, null, null, null, null, null, null
            );

            _mockSignInManager = new Mock<SignInManager<User>>(
                _mockUserManager.Object,
                new Mock<IHttpContextAccessor>().Object,
                new Mock<IUserClaimsPrincipalFactory<User>>().Object,
                null, null, null, null
            );

            _mockDbContext = new Mock<ApplicationDbContext>();
            _mockLogger = new Mock<ILogger<HomeController>>();
        }

        [Fact]
        public void Index_ReturnsViewResult()
        {
            var controller = new HomeController(
                _mockLogger.Object,
                _mockUserManager.Object,
                _mockSignInManager.Object,
                _mockDbContext.Object
            );

            var result = controller.Index();

            Assert.NotNull(result);
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void Privacy_ReturnsViewResult()
        {
            var controller = new HomeController(
                _mockLogger.Object,
                _mockUserManager.Object,
                _mockSignInManager.Object,
                _mockDbContext.Object
            );

            var result = controller.Privacy();

            Assert.NotNull(result);
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void Error_ReturnsViewResult()
        {
            var controller = new HomeController(
                _mockLogger.Object,
                _mockUserManager.Object,
                _mockSignInManager.Object,
                _mockDbContext.Object
            );

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            var result = controller.Error();

            Assert.NotNull(result);
            Assert.IsType<ViewResult>(result);
        }
    }
}
