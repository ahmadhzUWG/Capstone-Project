using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using TaskManager.Controllers;
using TaskManagerWebsite.Controllers;
using TaskManagerWebsite.Data;
using TaskManagerWebsite.Models;

namespace TaskManager.Tests.Tests.TestUsersController
{
    public class UsersControllerTests
    {
        [Fact]
        public async Task Index_ReturnsAViewResult_WithAListOfUsers()
        {
            var dbContext = TestHelper.GetDbContext();
            dbContext.Users.Add(new User { UserName = "User1" });
            dbContext.Users.Add(new User { UserName = "User2" });
            await dbContext.SaveChangesAsync();
            var controller = new UsersController(dbContext);

            var result = await controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<User>>(viewResult.Model);
            Assert.Equal(2, model.Count());
        }

        [Fact]
        public async Task Details_ReturnsAViewResult_WithAUser()
        {
            var dbContext = TestHelper.GetDbContext();
            dbContext.Users.Add(new User { Id = 1, UserName = "User1" });
            await dbContext.SaveChangesAsync();
            var controller = new UsersController(dbContext);

            var result = await controller.Details(1);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<User>(viewResult.Model);
            Assert.Equal("User1", model.UserName);
        }

        [Fact]
        public async Task Details_ReturnsNotFoundResult_WithNotFoundUser()
        {
            var dbContext = TestHelper.GetDbContext();
            dbContext.Users.Add(new User { Id = 1, UserName = "User1" });
            await dbContext.SaveChangesAsync();
            var controller = new UsersController(dbContext);

            var result = await controller.Details(2);

            Assert.NotNull(result);
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_ReturnsAViewResult_WithAUser()
        {
            var dbContext = TestHelper.GetDbContext();
            dbContext.Users.Add(new User { Id = 1, UserName = "User1" });
            await dbContext.SaveChangesAsync();
            var controller = new UsersController(dbContext);

            var result = await controller.Edit(1);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<User>(viewResult.Model);
            Assert.Equal("User1", model.UserName);
        }

        [Fact]
        public async Task Edit_ReturnsNotFoundResult_WithNotFoundUser()
        {
            var dbContext = TestHelper.GetDbContext();
            dbContext.Users.Add(new User { Id = 1, UserName = "User1" });
            await dbContext.SaveChangesAsync();
            var controller = new UsersController(dbContext);

            var result = await controller.Edit(2);

            Assert.NotNull(result);
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task EditWithUserArg_ReturnsRedirectToActionResult_WithAUser()
        {
            var dbContext = TestHelper.GetDbContext();
            dbContext.Users.Add(new User { Id = 1, UserName = "User1" });
            await dbContext.SaveChangesAsync();
            var controller = new UsersController(dbContext);
            var user = await dbContext.Users.FindAsync(1);

            var result = await controller.Edit(1, new User { Id = 1, UserName = "EditedUser1" });

            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectToActionResult.ActionName);
            Assert.NotNull(user);
            Assert.Equal("EditedUser1", user.UserName);
        }

        [Fact]
        public async Task EditWithUserArg_ReturnsViewResult_WithAUser()
        {
            var dbContext = TestHelper.GetDbContext();
            dbContext.Users.Add(new User { Id = 1, UserName = "User1" });
            await dbContext.SaveChangesAsync();
            var controller = new UsersController(dbContext);
            var editedUser = new User { Id = 1, UserName = "" };

            controller.ModelState.AddModelError("UserName", "UserName is required");
            var result = await controller.Edit(1, editedUser);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<User>(viewResult.Model);
            Assert.False(controller.ModelState.IsValid);
        }

        [Fact]
        public async Task EditWithUserArg_ReturnsNotFoundResult_WithNotFoundUser()
        {
            var dbContext = TestHelper.GetDbContext();
            dbContext.Users.Add(new User { Id = 1, UserName = "User1" });
            await dbContext.SaveChangesAsync();
            var controller = new UsersController(dbContext);

            var result = await controller.Edit(2, new User { Id = 2, UserName = "User2" });

            Assert.NotNull(result);
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task EditWithUserArg_ReturnsNotFoundResult_WithNotMatchingUserIds()
        {
            var dbContext = TestHelper.GetDbContext();
            var controller = new UsersController(dbContext);

            var result = await controller.Edit(1, new User { Id = 2, UserName = "User2" });

            Assert.NotNull(result);
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Delete_ReturnsAViewResult_WithAUser()
        {
            var dbContext = TestHelper.GetDbContext();
            dbContext.Users.Add(new User { Id = 1, UserName = "User1" });
            await dbContext.SaveChangesAsync();
            var controller = new UsersController(dbContext);

            var result = await controller.Delete(1);

            Assert.NotNull(result);
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<User>(viewResult.Model);
            Assert.Equal("User1", model.UserName);
        }

        [Fact]
        public async Task Delete_ReturnsNotFoundResult_WithNotFoundUser()
        {
            var dbContext = TestHelper.GetDbContext();
            dbContext.Users.Add(new User { Id = 1, UserName = "User1" });
            await dbContext.SaveChangesAsync();
            var controller = new UsersController(dbContext);

            var result = await controller.Delete(2);

            Assert.NotNull(result);
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteConfirmed_ReturnsRedirectToActionResult_WithDeletedUser()
        {
            var dbContext = TestHelper.GetDbContext();
            dbContext.Users.Add(new User { Id = 1, UserName = "User1" });
            await dbContext.SaveChangesAsync();
            var controller = new UsersController(dbContext);

            var result = await controller.DeleteConfirmed(1);
            
            Assert.NotNull(result);
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectToActionResult.ActionName);
            Assert.Null(await dbContext.Users.FindAsync(1));
        }
    }
}
