﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using Azure.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using TaskManagerWebsite.Controllers;
using TaskManagerWebsite.Data;
using TaskManagerWebsite.Models;
using TaskManagerWebsite.ViewModels;

namespace TaskManager.Tests.Tests.TestAccountController
{
    public class LoginControllerTests
    {

        private Mock<SignInManager<User>> createSignInManagerMock(ApplicationDbContext context, UserManager<User> userManager)
        {
            var mockSignInManager = new Mock<SignInManager<User>>(
                userManager,
                Mock.Of<IHttpContextAccessor>(),
                Mock.Of<IUserClaimsPrincipalFactory<User>>(),
                Mock.Of<IOptions<IdentityOptions>>(),
                Mock.Of<ILogger<SignInManager<User>>>(),
                Mock.Of<IAuthenticationSchemeProvider>(),
                Mock.Of<IUserConfirmation<User>>()
            );

            return mockSignInManager;
        }

        private Mock<UserManager<User>> createUserManagerMock(ApplicationDbContext context)
        {
            var mockUserManager = new Mock<UserManager<User>>(
                Mock.Of<IUserStore<User>>(),
                Mock.Of<IOptions<IdentityOptions>>(),
                Mock.Of<IPasswordHasher<User>>(),
                Array.Empty<IUserValidator<User>>(), Array.Empty<IPasswordValidator<User>>(),
                Mock.Of<ILookupNormalizer>(),
                Mock.Of<IdentityErrorDescriber>(),
                Mock.Of<IServiceProvider>(),
                Mock.Of<ILogger<UserManager<User>>>()
            );

            return mockUserManager;
        }

        [Fact]
        public void Register_ReturnsViewResult()
        {
            var dbContext = TestHelper.GetDbContext();
            var userManagerMock = createUserManagerMock(dbContext);
            userManagerMock.Setup(um => um.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);
            var signInManagerMock = createSignInManagerMock(dbContext, userManagerMock.Object);
            signInManagerMock.Setup(sm => sm.SignInAsync(It.IsAny<User>(), false, null))
                .Returns(Task.CompletedTask);
            var controller = new LoginController(userManagerMock.Object, signInManagerMock.Object);

            var result = controller.Register();

            Assert.NotNull(result);
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task RegisterWithModelArg_ReturnsViewResult_WithModelStateError()
        {
            var dbContext = TestHelper.GetDbContext();
            var userManagerMock = createUserManagerMock(dbContext);
            userManagerMock.Setup(um => um.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);
            var signInManagerMock = createSignInManagerMock(dbContext, userManagerMock.Object);
            signInManagerMock.Setup(sm => sm.SignInAsync(It.IsAny<User>(), false, null))
                .Returns(Task.CompletedTask);
            var controller = new LoginController(userManagerMock.Object, signInManagerMock.Object);
            var model = new RegisterViewModel
            {
                ConfirmPassword = "password",
                Email = "test@gmail.com",
                Password = "password",
                UserName = ""
            };

            controller.ModelState.AddModelError("UserName", "UserName is required");
            var result = await controller.Register(model);

            Assert.NotNull(result);
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(controller.ModelState.IsValid);
            Assert.IsType<RegisterViewModel>(viewResult.Model);
        }

        [Fact]
        public async Task RegisterWithModelArg_ReturnsRedirectToActionResult()
        {
            var dbContext = TestHelper.GetDbContext();
            var userManagerMock = createUserManagerMock(dbContext);
            userManagerMock.Setup(um => um.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);
            var signInManagerMock = createSignInManagerMock(dbContext, userManagerMock.Object);
            signInManagerMock.Setup(sm => sm.SignInAsync(It.IsAny<User>(), false, null))
                .Returns(Task.CompletedTask);
            var controller = new LoginController(userManagerMock.Object, signInManagerMock.Object);
            var model = new RegisterViewModel
            {
                ConfirmPassword = "password",
                Email = "test@gmail.com",
                Password = "password",
                UserName = "test"
            };

            var result = await controller.Register(model);

            Assert.NotNull(result);
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectToActionResult.ActionName);
        }

        [Fact]
        public async Task RegisterWithModelArg_ReturnsViewResult_WithFailedResult()
        {
            var dbContext = TestHelper.GetDbContext();
            var userManagerMock = createUserManagerMock(dbContext);
            userManagerMock.Setup(um => um.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError
                {
                    Code = "DuplicateUserName",
                    Description = "The username is already taken."
                }));
            var signInManagerMock = createSignInManagerMock(dbContext, userManagerMock.Object);
            signInManagerMock.Setup(sm => sm.SignInAsync(It.IsAny<User>(), false, null))
                .Returns(Task.CompletedTask);
            var controller = new LoginController(userManagerMock.Object, signInManagerMock.Object);
            var model = new RegisterViewModel
            {
                ConfirmPassword = "password",
                Email = "test@gmail.com",
                Password = "password",
                UserName = "test"
            };

            var result = await controller.Register(model);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(model, viewResult.Model); 
            Assert.True(viewResult.ViewData.ModelState.ContainsKey("")); 

            var error = viewResult.ViewData.ModelState[""].Errors[0].ErrorMessage;
            Assert.Equal("The username is already taken.", error); 
        }

        [Fact]
        public void Login_ReturnsViewResult()
        {
            var dbContext = TestHelper.GetDbContext();
            var userManagerMock = createUserManagerMock(dbContext);
            userManagerMock.Setup(um => um.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);
            var signInManagerMock = createSignInManagerMock(dbContext, userManagerMock.Object);
            signInManagerMock.Setup(sm => sm.SignInAsync(It.IsAny<User>(), false, null))
                .Returns(Task.CompletedTask);
            var controller = new LoginController(userManagerMock.Object, signInManagerMock.Object);

            var result = controller.Login();

            Assert.NotNull(result);
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task LoginWithArgs_ReturnViewResult_WithModelStateError()
        {
            var dbContext = TestHelper.GetDbContext();
            var userManagerMock = createUserManagerMock(dbContext);
            userManagerMock.Setup(um => um.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);
            var signInManagerMock = createSignInManagerMock(dbContext, userManagerMock.Object);
            signInManagerMock.Setup(sm => sm.SignInAsync(It.IsAny<User>(), false, null))
                .Returns(Task.CompletedTask);
            var controller = new LoginController(userManagerMock.Object, signInManagerMock.Object);
            var model = new LoginViewModel()
            {
                Password = "password",
                UserName = ""
            };

            controller.ModelState.AddModelError("UserName", "UserName is required");
            var result = await controller.Login(model);

            Assert.NotNull(result);
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(controller.ModelState.IsValid);
            Assert.IsType<LoginViewModel>(viewResult.Model);
        }

        [Fact]
        public async Task LoginWithArgs_ReturnLocalRedirectResult()
        {
            var dbContext = TestHelper.GetDbContext();
            var userManagerMock = createUserManagerMock(dbContext);
            userManagerMock.Setup(um => um.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);
            var signInManagerMock = createSignInManagerMock(dbContext, userManagerMock.Object);
            signInManagerMock.Setup(sm => sm.PasswordSignInAsync(It.IsAny<string>(), It.IsAny<string>(), false, false))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);
            var mockUrlHelper = new Mock<IUrlHelper>();
            mockUrlHelper.Setup(m => m.IsLocalUrl(It.IsAny<string>())).Returns(true);
            var controller = new LoginController(userManagerMock.Object, signInManagerMock.Object);
            controller.Url = mockUrlHelper.Object;
            var model = new LoginViewModel()
            {
                Password = "password",
                UserName = "test"
            };


            var result = await controller.Login(model, "/Home");

            Assert.NotNull(result);
            var localRedirectResult = Assert.IsType<LocalRedirectResult>(result);
            Assert.Equal("/Home", localRedirectResult.Url);
        }

        [Fact]
        public async Task LoginWithArgs_ReturnRedirectToAction()
        {
            var dbContext = TestHelper.GetDbContext();
            var userManagerMock = createUserManagerMock(dbContext);
            userManagerMock.Setup(um => um.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);
            var signInManagerMock = createSignInManagerMock(dbContext, userManagerMock.Object);
            signInManagerMock.Setup(sm => sm.PasswordSignInAsync(It.IsAny<string>(), It.IsAny<string>(), false, false))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);
            var controller = new LoginController(userManagerMock.Object, signInManagerMock.Object);
            var model = new LoginViewModel()
            {
                Password = "password",
                UserName = "test"
            };

            var result = await controller.Login(model);

            Assert.NotNull(result);
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectToActionResult.ActionName);
        }

        [Fact]
        public async Task LoginWithArgs_ReturnViewResult_WithFailedResult()
        {
            var dbContext = TestHelper.GetDbContext();
            var userManagerMock = createUserManagerMock(dbContext);
            userManagerMock.Setup(um => um.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);
            var signInManagerMock = createSignInManagerMock(dbContext, userManagerMock.Object);
            signInManagerMock.Setup(sm => sm.PasswordSignInAsync(It.IsAny<string>(), It.IsAny<string>(), false, false))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Failed);
            var controller = new LoginController(userManagerMock.Object, signInManagerMock.Object);
            var model = new LoginViewModel()
            {
                Password = "password",
                UserName = "test"
            };

            var result = await controller.Login(model);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(model, viewResult.Model);
            Assert.True(viewResult.ViewData.ModelState.ContainsKey(""));
            var error = viewResult.ViewData.ModelState[""].Errors[0].ErrorMessage;
            Assert.Equal("Invalid login attempt.", error);
        }

        [Fact]
        public async Task Logout_ReturnRedirectToActionResult()
        {
            var dbContext = TestHelper.GetDbContext();
            var userManagerMock = createUserManagerMock(dbContext);
            var signInManagerMock = createSignInManagerMock(dbContext, userManagerMock.Object);
            signInManagerMock.Setup(sm => sm.SignOutAsync())
                .Returns(Task.CompletedTask);
            var controller = new LoginController(userManagerMock.Object, signInManagerMock.Object);
            
            var result = await controller.Logout();

            Assert.NotNull(result);
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirectToActionResult.ActionName);
        }
    }

}
