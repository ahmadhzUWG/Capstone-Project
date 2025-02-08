using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TaskManagerWebsite.Controllers;
using TaskManagerWebsite.Models;

namespace TaskManagerWebsite.Tests
{
    [TestClass]
    public class UsersControllerTests
    {
        private Mock<TaskManagerDatabaseEntities> mockContext;
        private Mock<DbSet<User>> mockSet;
        private List<User> data;

        [TestInitialize]
        public void Setup()
        {
            data = new List<User>
            {
                new User { Id = 1, Username = "User1", Password = "Password1", Role = "Role1" },
                new User { Id = 2, Username = "User2", Password = "Password2", Role = "Role2" },
            };

            mockSet = new Mock<DbSet<User>>();
            mockSet.As<IQueryable<User>>().Setup(m => m.Provider).Returns(data.AsQueryable().Provider);
            mockSet.As<IQueryable<User>>().Setup(m => m.Expression).Returns(data.AsQueryable().Expression);
            mockSet.As<IQueryable<User>>().Setup(m => m.ElementType).Returns(data.AsQueryable().ElementType);
            mockSet.As<IQueryable<User>>().Setup(m => m.GetEnumerator()).Returns(data.AsQueryable().GetEnumerator());

            mockSet.Setup(m => m.Find(It.IsAny<object[]>())).Returns<object[]>(id => data.FirstOrDefault(d => d.Id == (int)id[0]));
            mockSet.Setup(m => m.Add(It.IsAny<User>())).Returns<User>(u =>
            {
                data.Add(u);
                return u;
            });
            mockSet.Setup(m => m.Remove(It.IsAny<User>())).Returns<User>(u =>
            {
                data.Remove(u);
                return u;
            });

            mockContext = new Mock<TaskManagerDatabaseEntities>();
            mockContext.Setup(m => m.Users).Returns(mockSet.Object);
            mockContext.Setup(m => m.SetModified(It.IsAny<object>())).Callback<object>(entity =>
            {

                if (entity is User user)
                {
                    var existingUser = data.FirstOrDefault(u => u.Id == user.Id);
                    if (existingUser != null)
                    {
                        existingUser.Username = user.Username;
                        existingUser.Password = user.Password;
                        existingUser.Role = user.Role;
                    }
                }
            });
            mockContext.Setup(m => m.SaveChanges()).Returns(1);

        }

        [TestMethod]
        public void Index()
        {
            // Arrange
            UsersController controller = new UsersController();

            // Act
            ViewResult result = controller.Index() as ViewResult;

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void Details()
        {
            // Arrange
            UsersController controller = new UsersController(this.mockContext.Object);

            // Act
            ViewResult result = controller.Details(1) as ViewResult;
            var user = (User)result.Model;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(user);
            Assert.AreEqual(1, user.Id);
        }

        [TestMethod]
        public void DetailsNullId()
        {
            // Arrange
            UsersController controller = new UsersController(this.mockContext.Object);

            // Act
            HttpStatusCodeResult result = controller.Details(null) as HttpStatusCodeResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(result.StatusCode, Convert.ToInt32(HttpStatusCode.BadRequest));
        }

        [TestMethod]
        public void DetailsNullUser()
        {
            // Arrange
            UsersController controller = new UsersController(this.mockContext.Object);

            // Act
            HttpNotFoundResult result = controller.Details(12) as HttpNotFoundResult;

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void Create()
        {
            // Arrange
            UsersController controller = new UsersController(this.mockContext.Object);

            // Act
            ViewResult result = controller.Create() as ViewResult;

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void CreateWithUser()
        {
            // Arrange
            UsersController controller = new UsersController(this.mockContext.Object);

            // Act
            User newUser = new User()
            {
                Username = "NewUser",
                Password = "NewPassword",
                Role = "NewRole",
                Id = 3
            };
            RedirectToRouteResult result = controller.Create(newUser) as RedirectToRouteResult;

            // Assert
            mockContext.Verify(m => m.SaveChanges(), Times.Once());
            Assert.IsNotNull(result);
            Assert.AreEqual(3, this.data[2].Id);
            Assert.AreEqual("NewUser", this.data[2].Username);
            Assert.AreEqual("NewPassword", this.data[2].Password);
            Assert.AreEqual("NewRole", this.data[2].Role);
        }

        [TestMethod]
        public void CreateWithInvalidUser()
        {
            // Arrange
            UsersController controller = new UsersController(this.mockContext.Object);

            // Act
            User newUser = new User()
            {
                Username = "",
                Password = "NewPassword",
                Role = "NewRole",
                Id = 3
            };

            controller.ModelState.AddModelError("Username", "Username is required");

            ViewResult result = controller.Create(newUser) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(controller.ModelState.IsValid);
            Assert.AreEqual(newUser, result.Model);
        }

        [TestMethod]
        public void Delete()
        {
            // Arrange
            UsersController controller = new UsersController(this.mockContext.Object);

            // Act
            ViewResult result = controller.Delete(1) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void DeleteNullId()
        {
            // Arrange
            UsersController controller = new UsersController(this.mockContext.Object);

            // Act
            HttpStatusCodeResult result = controller.Delete(null) as HttpStatusCodeResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(result.StatusCode, Convert.ToInt32(HttpStatusCode.BadRequest));
        }

        [TestMethod]
        public void DeleteNullUser()
        {
            // Arrange
            UsersController controller = new UsersController(this.mockContext.Object);

            // Act
            HttpNotFoundResult result = controller.Delete(12) as HttpNotFoundResult;

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void DeleteConfirmed()
        {
            // Arrange
            UsersController controller = new UsersController(this.mockContext.Object);

            // Act
            RedirectToRouteResult result = controller.DeleteConfirmed(1) as RedirectToRouteResult;

            // Assert
            mockSet.Verify(m => m.Remove(It.IsAny<User>()), Times.Once());
            Assert.IsNotNull(result);
            Assert.IsNull(this.data.FirstOrDefault(u => u.Id == 1));

        }

        [TestMethod]
        public void EditWithId()
        {
            // Arrange
            UsersController controller = new UsersController(this.mockContext.Object);

            // Act
            ViewResult result = controller.Edit(1) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void EditNullId()
        {
            // Arrange
            UsersController controller = new UsersController(this.mockContext.Object);

            // Act
            HttpStatusCodeResult result = controller.Edit((int?)null) as HttpStatusCodeResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(Convert.ToInt32(HttpStatusCode.BadRequest), result.StatusCode);
        }


        [TestMethod]
        public void EditNullUser()
        {
            // Arrange
            UsersController controller = new UsersController(this.mockContext.Object);

            // Act
            HttpNotFoundResult result = controller.Edit(12) as HttpNotFoundResult;

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void EditWithUser()
        {
            // Arrange
            UsersController controller = new UsersController(this.mockContext.Object);

            // Act
            User editedUser = new User()
            {
                Username = "EditedUser",
                Password = "EditedPassword",
                Role = "EditedRole",
                Id = 1
            };
            RedirectToRouteResult result = controller.Edit(editedUser) as RedirectToRouteResult;

            // Assert
            mockContext.Verify(m => m.SaveChanges(), Times.Once());
            Assert.IsNotNull(result);
            Assert.AreEqual("EditedUser", this.data[0].Username);
            Assert.AreEqual("EditedPassword", this.data[0].Password);
            Assert.AreEqual("EditedRole", this.data[0].Role);
        }

        [TestMethod]
        public void EditWithInvalidUser()
        {
            // Arrange
            UsersController controller = new UsersController(this.mockContext.Object);

            // Act
            controller.ModelState.AddModelError("Username", "Username is required");

            ViewResult result = controller.Edit(this.data[0]) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(controller.ModelState.IsValid);  
            Assert.AreEqual(this.data[0], result.Model);  
        }
    }
}