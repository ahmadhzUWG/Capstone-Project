using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManagerData.Models;
using TaskManagerData.Services;
using TaskManagerWebsite.Controllers;
using Task = System.Threading.Tasks.Task;

namespace TaskManager.Tests.WebsiteTests.TestControllers
{
    public class FindUsernameControllerTests
    {
        private readonly Mock<UserManager<User>> _mockUserManager;
        private readonly Mock<EmailService> _mockEmailService;

        public FindUsernameControllerTests()
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
            // Act: Create a new instance of FindUsernameController
            var controller = new FindUsernameController(_mockEmailService.Object, _mockUserManager.Object);

            controller.TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());

            // Act
            var result = controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Null(viewResult.Model);
        }

        [Fact]
        public async Task Index_Post_ValidEmail_SendsEmail()
        {
            // Arrange
            var email = "test@example.com";
            var user = new User { UserName = "testuser", Email = email };

            _mockUserManager.Setup(um => um.Users)
                .Returns(new List<User> { user }.AsQueryable());

            // Act: Create a new instance of FindUsernameController
            var controller = new FindUsernameController(_mockEmailService.Object, _mockUserManager.Object);

            controller.TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());

            // Act
            var result = await controller.Index(email);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.True(controller.TempData.ContainsKey("SuccessMessage"));
            _mockEmailService.Verify(
                es => es.SendEmailAsync(email, "Task Manager - Username Recovery", It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task Index_Post_UserNotFound_DoesNotSendEmail()
        {
            // Arrange
            var email = "nonexistent@example.com";
            var user = new User { UserName = "testuser", Email = "test@example.com" };

            // Mock UserManager to return no users with the given email
            _mockUserManager.Setup(um => um.Users)
                .Returns(new List<User> { user }.AsQueryable());

            // Act: Create a new instance of FindUsernameController
            var controller = new FindUsernameController(_mockEmailService.Object, _mockUserManager.Object);

            controller.TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());

            // Act
            var result = await controller.Index(email);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.True(controller.TempData.ContainsKey("SuccessMessage"));
            _mockEmailService.Verify(
                es => es.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Index_Post_InvalidModel_DoesNotProcess()
        {
            // Arrange
            var model = string.Empty; // Invalid email format

            // Act: Create a new instance of FindUsernameController
            var controller = new FindUsernameController(_mockEmailService.Object, _mockUserManager.Object);

            controller.ModelState.AddModelError("email", "Email is required");
            controller.TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());

            // Act
            var result = await controller.Index(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(controller.ModelState.IsValid);
            _mockEmailService.Verify(
                es => es.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }
    }
}
