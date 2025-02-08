using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
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
        public void Remove()
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
    }
}