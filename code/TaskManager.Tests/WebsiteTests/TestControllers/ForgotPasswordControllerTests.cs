using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
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
        private readonly Mock<UserManager<User>> _mockUserManager;
        private readonly Mock<EmailService> _mockEmailService;


        public ForgotPasswordControllerTests()
        {
            _mockUserManager = GetMockUserManager();

            _mockEmailService = new Mock<EmailService>();

            _mockEmailService.Setup(es => es.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask); 

            _mockUserManager.Setup(um => um.GetRolesAsync(It.IsAny<User>()))
                .ReturnsAsync(new List<string> { "Admin", "Manager", "Employee" });
        }

        private Mock<UserManager<User>> GetMockUserManager()
        {
            var userStoreMock = new Mock<IUserStore<User>>();
            return new Mock<UserManager<User>>(userStoreMock.Object,
                null, null, null, null, null, null, null, null);
        }

        [Fact]
        public void Index_Get_ReturnsView()
        {
            var controller = new ForgotPasswordController(_mockEmailService.Object, _mockUserManager.Object);

            var result = controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Null(viewResult.Model);
        }

        [Fact]
        public async Task Index_Post_UserExists_ResetsPassword_SendsEmail()
        {
            var model = new ForgotPasswordViewModel
            {
                Username = "testuser",
                Password = "NewPass1!",
                ConfirmPassword = "NewPass1!"
            };

            var user = new User { UserName = "testuser", Email = "test@example.com" };

            _mockUserManager.Setup(um => um.FindByNameAsync(model.Username))
                .ReturnsAsync(user);

            _mockUserManager.Setup(um => um.GeneratePasswordResetTokenAsync(user))
                .ReturnsAsync("valid-token");

            _mockUserManager.Setup(um => um.ResetPasswordAsync(user, "valid-token", model.Password))
                .ReturnsAsync(IdentityResult.Success);

            _mockEmailService.Setup(es => es.SendEmailAsync(user.Email, It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            var controller = new ForgotPasswordController(_mockEmailService.Object, _mockUserManager.Object);

            controller.TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());

            var result = await controller.Index(model);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.True(controller.TempData.ContainsKey("SuccessMessage"));
            _mockUserManager.Verify(um => um.ResetPasswordAsync(user, "valid-token", model.Password), Times.Once);
            _mockEmailService.Verify(es => es.SendEmailAsync(user.Email, "Task Manager - Password Reset", It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task Index_Post_UserDoesNotExist_AddsModelError()
        {
            var model = new ForgotPasswordViewModel
            {
                Username = "nonexistentuser",
                Password = "NewPass1!",
                ConfirmPassword = "NewPass1!"
            };

            _mockUserManager.Setup(um => um.FindByNameAsync(model.Username))
                .ReturnsAsync((User)null);

            var controller = new ForgotPasswordController(_mockEmailService.Object, _mockUserManager.Object);

            var result = await controller.Index(model);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(controller.ModelState.IsValid);
            Assert.Contains(controller.ModelState, kvp => kvp.Key == "Username" && kvp.Value.Errors.Count > 0);
        }

        [Fact]
        public async Task Index_Post_ResetPasswordFails_ShowsErrors()
        {
            var model = new ForgotPasswordViewModel
            {
                Username = "testuser",
                Password = "NewPass1!",
                ConfirmPassword = "NewPass1!"
            };

            var user = new User { UserName = "testuser", Email = "test@example.com" };

            _mockUserManager.Setup(um => um.FindByNameAsync(model.Username))
                .ReturnsAsync(user);

            _mockUserManager.Setup(um => um.GeneratePasswordResetTokenAsync(user))
                .ReturnsAsync("valid-token");

            _mockUserManager.Setup(um => um.ResetPasswordAsync(user, "valid-token", model.Password))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Weak password." }));

            var controller = new ForgotPasswordController(_mockEmailService.Object, _mockUserManager.Object);

            controller.TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());

            var result = await controller.Index(model);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(controller.ModelState.IsValid);
            Assert.Contains(controller.ModelState, kvp => kvp.Key == "" && kvp.Value.Errors[0].ErrorMessage == "Weak password.");
            Assert.True(controller.TempData.ContainsKey("ErrorMessage"));
        }
    }
}
