using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Moq;
using TaskManagerData.Models;
using TaskManagerData.Services;
using TaskManagerWebsite.Controllers;
using TaskManagerWebsite.ViewModels;
using Task = System.Threading.Tasks.Task;

namespace TaskManager.Tests.WebsiteTests.TestControllers
{
    public class ForgotPasswordControllerTests
    {
        private readonly ApplicationDbContext _context;
        private readonly Mock<UserManager<User>> _mockUserManager;
        private readonly Mock<EmailService> _mockEmailService;

        public ForgotPasswordControllerTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("TestDb")
                .Options;
            _context = new ApplicationDbContext(options);

            var userStore = new Mock<IUserStore<User>>();
            _mockUserManager = new Mock<UserManager<User>>(userStore.Object, null, null, null, null, null, null, null, null);
            _mockEmailService = new Mock<EmailService>();
        }

        [Fact]
        public async Task SendOneTimeCode_SendsCode_WhenUserExists()
        {
            var user = new User { UserName = "testuser", Email = "test@example.com" };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            _mockUserManager.Setup(m => m.Users)
                .Returns(_context.Users);

            var controller = new ForgotPasswordController(_context, _mockEmailService.Object, _mockUserManager.Object);
            controller.TempData = new TempDataDictionary(
                new DefaultHttpContext(),
                Mock.Of<ITempDataProvider>());

            var model = new ForgotPasswordViewModel { Username = "testuser", Password = "123456", ConfirmPassword = "123456", OneTimeCode = "654321"};

            _mockEmailService.Setup(es => es.SendEmailAsync(user.Email, It.IsAny<string>(), It.IsAny<string>())).Returns(Task.CompletedTask);

            var result = await controller.SendOneTimeCode(model);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("Index", viewResult.ViewName);
            var returnedModel = Assert.IsType<ForgotPasswordViewModel>(viewResult.Model);
            Assert.True(returnedModel.SentOneTime);
        }

        [Fact]
        public async Task SendOneTimeCode_AddsModelError_WhenUserNotFound()
        {
            var controller = new ForgotPasswordController(_context, _mockEmailService.Object, _mockUserManager.Object);
            var model = new ForgotPasswordViewModel { Username = "testuser", Password = "123456", ConfirmPassword = "123456", OneTimeCode = "654321" };

            var result = await controller.SendOneTimeCode(model);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.True(controller.ModelState.ContainsKey("Username"));
        }

        [Fact]
        public async Task VerifyOneTimeCode_SetsVerified_WhenCodeMatches()
        {
            var user = new User { UserName = "testuser", Email = "test@example.com" };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            _mockUserManager.Setup(m => m.Users)
                .Returns(_context.Users);

            _context.PasswordResets.Add(new PasswordReset
            {
                Username = user.UserName,
                Email = user.Email,
                Code = "123456"
            });
            await _context.SaveChangesAsync();

            var controller = new ForgotPasswordController(_context, _mockEmailService.Object, _mockUserManager.Object);
            controller.TempData = new TempDataDictionary(
                new DefaultHttpContext(),
                Mock.Of<ITempDataProvider>());

            var model = new ForgotPasswordViewModel { Username = user.UserName, OneTimeCode = "123456", Password = "654321", ConfirmPassword = "654321"};

            var result = await controller.VerifyOneTimeCode(model);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("Index", viewResult.ViewName);
            var returnedModel = Assert.IsType<ForgotPasswordViewModel>(viewResult.Model);
            Assert.True(returnedModel.VerifiedOneTime);

        }

        [Fact]
        public async Task VerifyOneTimeCode_AddsError_WhenCodeDoesNotMatch()
        {
            var user = new User { UserName = "testuser", Email = "test@example.com" };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            _context.PasswordResets.Add(new PasswordReset { Username = user.UserName, Email = user.Email, Code = "999999" });
            await _context.SaveChangesAsync();

            var controller = new ForgotPasswordController(_context, _mockEmailService.Object, _mockUserManager.Object);
            var model = new ForgotPasswordViewModel { Username = user.UserName, OneTimeCode = "123456", Password = "654321", ConfirmPassword = "654321" };

            var result = await controller.VerifyOneTimeCode(model);

            Assert.True(controller.ModelState.ContainsKey("OneTimeCode"));
        }

        [Fact]
        public async Task Index_Post_ReturnsView_WhenPasswordResetFails()
        {
            // Arrange
            var user = new User { UserName = "testuser", Email = "test@example.com" };

            _mockUserManager.Setup(m => m.FindByNameAsync(user.UserName)).ReturnsAsync(user);
            _mockUserManager.Setup(m => m.GeneratePasswordResetTokenAsync(user)).ReturnsAsync("token");

            // Simulate failed password reset
            var identityError = IdentityResult.Failed(new IdentityError { Description = "Password too weak" });
            _mockUserManager.Setup(m => m.ResetPasswordAsync(user, "token", "weak")).ReturnsAsync(identityError);

            _mockEmailService.Setup(es => es.SendEmailAsync(user.Email, It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            _context.Users.Add(user);
            _context.PasswordResets.Add(new PasswordReset
            {
                Username = user.UserName,
                Email = user.Email,
                Code = "123456"
            });
            await _context.SaveChangesAsync();

            var controller = new ForgotPasswordController(_context, _mockEmailService.Object, _mockUserManager.Object);
            controller.TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());

            var model = new ForgotPasswordViewModel
            {
                Username = user.UserName,
                Password = "weak",  // Simulate weak password
                ConfirmPassword = "weak",
                OneTimeCode = "123456"
            };

            // Act
            var result = await controller.Index(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(controller.ModelState.IsValid);
            Assert.True(controller.ModelState.ErrorCount > 0);
            Assert.Contains("Password too weak", controller.ModelState[string.Empty].Errors[0].ErrorMessage);
        }


        [Fact]
        public async Task Index_Post_ResetsPassword_WhenValid()
        {
            var user = new User { UserName = "testuser", Email = "test@example.com" };

            _mockUserManager.Setup(m => m.FindByNameAsync(user.UserName)).ReturnsAsync(user);
            _mockUserManager.Setup(m => m.GeneratePasswordResetTokenAsync(user)).ReturnsAsync("token");
            _mockUserManager.Setup(m => m.ResetPasswordAsync(user, "token", "NewPass1!")).ReturnsAsync(IdentityResult.Success);
            _mockEmailService.Setup(es => es.SendEmailAsync(user.Email, It.IsAny<string>(), It.IsAny<string>())).Returns(Task.CompletedTask);

            _context.Users.Add(user);
            _context.PasswordResets.Add(new PasswordReset { Username = user.UserName, Email = user.Email, Code = "123456" });
            await _context.SaveChangesAsync();

            var model = new ForgotPasswordViewModel
            {
                Username = user.UserName,
                Password = "NewPass1!",
                ConfirmPassword = "NewPass1!",
                OneTimeCode = "123456"
            };

            var controller = new ForgotPasswordController(_context, _mockEmailService.Object, _mockUserManager.Object);

            controller.TempData = new TempDataDictionary(
                new DefaultHttpContext(),
                Mock.Of<ITempDataProvider>());

            var result = await controller.Index(model);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            Assert.Equal("Login", redirect.ControllerName);
        }



        [Fact]
        public void Index_Get_ReturnsView()
        {
            var controller = new ForgotPasswordController(_context, _mockEmailService.Object, _mockUserManager.Object);
            var result = controller.Index();
            Assert.IsType<ViewResult>(result);
        }

    }
}
