using Microsoft.AspNetCore.Identity;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TaskManagerWebsite.Controllers;
using TaskManagerWebsite.Data;
using TaskManagerWebsite.Models;

namespace TaskManager.Tests.Tests.TestControllers
{
    public class EmployeeControllerTests
    {
        private readonly Mock<UserManager<User>> _mockUserManager;
        private readonly Mock<RoleManager<IdentityRole<int>>> _mockRoleManager;
        private readonly ApplicationDbContext _dbContext;

        public EmployeeControllerTests()
        {
            _mockUserManager = GetMockUserManager();
            _mockRoleManager = GetMockRoleManager();
            _dbContext = TestHelper.GetDbContext();

            var mockRoles = new List<IdentityRole<int>>
            {
                new IdentityRole<int> { Id = 1, Name = "Admin" },
                new IdentityRole<int> { Id = 2, Name = "Manager" },
                new IdentityRole<int> { Id = 3, Name = "Employee" }
            }.AsQueryable();

            _mockRoleManager
                .Setup(rm => rm.Roles)
                .Returns(mockRoles);

            _mockUserManager
                .Setup(um => um.GetRolesAsync(It.IsAny<User>()))
                .ReturnsAsync(new List<string> { "Admin", "Manager", "Employee" });
        }

        private Mock<UserManager<User>> GetMockUserManager()
        {
            var userStoreMock = new Mock<IUserStore<User>>();
            return new Mock<UserManager<User>>(userStoreMock.Object, null, null, null, null, null, null, null, null);
        }

        private Mock<RoleManager<IdentityRole<int>>> GetMockRoleManager()
        {
            var roleStoreMock = new Mock<IRoleStore<IdentityRole<int>>>();
            return new Mock<RoleManager<IdentityRole<int>>>(roleStoreMock.Object, null, null, null, null);
        }

        [Fact]
        public async Task Users_ReturnsViewResult()
        {
            var controller = new EmployeeController(_dbContext, _mockUserManager.Object, _mockRoleManager.Object);

            var result = await controller.Users();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<User>>(viewResult.Model);
            Assert.Empty(model);
        }

        [Fact]
        public async Task Groups_ReturnsViewResult()
        {
            var controller = new EmployeeController(_dbContext, _mockUserManager.Object, _mockRoleManager.Object);

            var result = await controller.Groups();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<Group>>(viewResult.Model);
            Assert.Empty(model);
        }

        [Fact]
        public async Task Projects_ReturnsViewResult()
        {
            var controller = new EmployeeController(_dbContext, _mockUserManager.Object, _mockRoleManager.Object);

            var user = new User
            {
                Id = 1,
                UserName = "TestUser",
                Email = "user@gmail.com"
            };

            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            _mockUserManager.Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>()))
                .Returns(user.Id.ToString());

            var result = await controller.Projects();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<Project>>(viewResult.Model);
            Assert.Empty(model);
        }

        [Fact]
        public async Task ProjectDetails_ReturnsViewResult_WithCorrectManagedAndUnmanagedGroups()
        {
            var controller = new EmployeeController(_dbContext, _mockUserManager.Object, _mockRoleManager.Object);

            var user = new User
            {
                Id = 1,
                UserName = "TestUser",
                Email = "user@gmail.com"
            };

            var otherUser = new User
            {
                Id = 2,
                UserName = "Test2User",
                Email = "user2@gmail.com"
            };

            var managedGroup = new Group
            {
                Id = 1,
                Name = "TestGroup",
                Description = "TestDescription",
                ManagerId = 1,
                Manager = user
            };

            var unmanagedGroup = new Group
            {
                Id = 2,
                Name = "Test2Group",
                Description = "Test2Description",
                ManagerId = 2,
                Manager = otherUser
            };

            var anotherUnmanagedGroup = new Group
            {
                Id = 3,
                Name = "Test3Group",
                Description = "Test3Description",
                ManagerId = 2,
                Manager = otherUser
            };

            var managedGroupManager = new GroupManager
            {
                GroupId = 1,
                Group = managedGroup,
                UserId = 1,
                User = user
            };

            var unmanagedGroupManager = new GroupManager
            {
                GroupId = 2,
                Group = unmanagedGroup,
                UserId = 2,
                User = otherUser
            };

            var project = new Project
            {
                Id = 1,
                Name = "Test1Project",
                Description = "Test1Description",
                ProjectLeadId = 1,
                ProjectLead = user
            };

            var groupProject1 = new GroupProject
            {
                GroupId = 1,
                Group = managedGroup,
                ProjectId = 1,
                Project = project
            };

            var groupProject2 = new GroupProject
            {
                GroupId = 2,
                Group = unmanagedGroup,
                ProjectId = 1,
                Project = project
            };

            _dbContext.Users.Add(user);
            _dbContext.Users.Add(otherUser);
            _dbContext.Groups.Add(managedGroup);
            _dbContext.Groups.Add(unmanagedGroup);
            _dbContext.Groups.Add(anotherUnmanagedGroup);
            _dbContext.Projects.Add(project);
            _dbContext.GroupManagers.Add(managedGroupManager);
            _dbContext.GroupManagers.Add(unmanagedGroupManager);
            _dbContext.GroupProjects.Add(groupProject1);
            _dbContext.GroupProjects.Add(groupProject2);
            await _dbContext.SaveChangesAsync();

            _mockUserManager.Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(user.Id.ToString());

            var result = await controller.ProjectDetails(1);

            var viewResult = Assert.IsType<ViewResult>(result);

            var managedGroups = Assert.IsAssignableFrom<List<Group>>(viewResult.ViewData["ManagedGroups"]);
            var unmanagedGroups = Assert.IsAssignableFrom<List<Group>>(viewResult.ViewData["UnmanagedGroups"]);

            Assert.NotNull(managedGroups);
            Assert.NotNull(unmanagedGroups);

            Assert.DoesNotContain(managedGroups, g => g.Id == 1);

            Assert.DoesNotContain(unmanagedGroups, g => g.Id == 2);

            Assert.Contains(unmanagedGroups, g => g.Id == 3);

            Assert.Empty(managedGroups);

            Assert.Single(unmanagedGroups);
        }

        [Fact]
        public async Task ProjectDetails_ReturnsNotFound_WithNullProject()
        {
            var controller = new EmployeeController(_dbContext, _mockUserManager.Object, _mockRoleManager.Object);

            var result = await controller.ProjectDetails(999);

            Assert.IsType<NotFoundResult>(result);
        }
    }
}
