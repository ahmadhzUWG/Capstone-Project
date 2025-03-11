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
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TaskManagerWebsite.ViewModels;

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

        [Fact]
        public async Task CreateProject_Get_ReturnsViewWithProjectLeads()
        {
            // Arrange
            var dbContext = TestHelper.GetDbContext();
            dbContext.Users.Add(new User { Id = 1, UserName = "Lead1", Email = "lead1@example.com" });
            dbContext.Users.Add(new User { Id = 2, UserName = "Lead2", Email = "lead2@example.com" });
            await dbContext.SaveChangesAsync();
            var controller = new EmployeeController(dbContext, _mockUserManager.Object, _mockRoleManager.Object);

            CreateProjectViewModel model = new CreateProjectViewModel
            {
                ProjectLeads = new List<SelectListItem>
            {
                new SelectListItem { Value = "1", Text = "Lead1" },
                new SelectListItem { Value = "2", Text = "Lead2" }
            }
            };

            // Act
            var result = await controller.CreateProject();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(model.ProjectLeads);
            Assert.Equal(2, model.ProjectLeads.Count);
        }

        [Fact]
        public async Task CreateProject_Post_InvalidModel_ReturnsView()
        {
            CreateProjectViewModel model = new CreateProjectViewModel();
            // Arrange
            var dbContext = TestHelper.GetDbContext();
            var controller = new EmployeeController(dbContext, _mockUserManager.Object, _mockRoleManager.Object);
            controller.ModelState.AddModelError("Error", "Invalid");
            var project = new Project { Id = 1, Name = "InvalidProject" };
            var users = await dbContext.Users.ToListAsync();

            model.ProjectLeads = users.Select(u => new SelectListItem
            {
                Value = u.Id.ToString(),
                Text = u.UserName
            }).ToList();

            CreateProjectViewModel viewModel = new CreateProjectViewModel
            {
                ProjectLeads = new List<SelectListItem>
            {
                new SelectListItem { Value = "1", Text = "Lead1" },
                new SelectListItem { Value = "2", Text = "Lead2" }
            }
            };
            // Act
            var result = await controller.CreateProject(viewModel);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(viewModel, viewResult.Model);
        }

        [Fact]
        public async Task CreateProject_Post_WithoutUserId_ReturnsViewWithError()
        {
            // Arrange
            var dbContext = TestHelper.GetDbContext();
            var controller = new EmployeeController(dbContext, _mockUserManager.Object, _mockRoleManager.Object);
            _mockUserManager.Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(string.Empty);
            var project = new Project { Id = 1, Name = "ProjectNoUser" };
            var users = await dbContext.Users.ToListAsync();

            CreateProjectViewModel model = new CreateProjectViewModel
            {
                ProjectLeads = users.Select(u => new SelectListItem
                {
                    Value = u.Id.ToString(),
                    Text = u.UserName
                }).ToList()
            };

            // Act
            var result = await controller.CreateProject(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(controller.ModelState.IsValid);
            Assert.Contains("", controller.ModelState.Keys);
        }

        [Fact]
        public async Task CreateProject_Post_Valid_ReturnsRedirect()
        {
            // Arrange
            var dbContext = TestHelper.GetDbContext();
            var controller = new UserController(dbContext, _mockUserManager.Object, _mockRoleManager.Object);

            _mockUserManager.Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("1");

            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, "1") };
            controller.ControllerContext.HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"))
            };

            var services = new ServiceCollection()
                .AddSingleton<ITempDataProvider>(new Mock<ITempDataProvider>().Object)
                .BuildServiceProvider();

            controller.ControllerContext.HttpContext.RequestServices = services;
            controller.TempData = new TempDataDictionary(controller.ControllerContext.HttpContext,
                services.GetRequiredService<ITempDataProvider>());

            var mockUrlHelper = new Mock<IUrlHelper>();
            controller.Url = mockUrlHelper.Object;

            var users = await dbContext.Users.ToListAsync();

            CreateProjectViewModel model = new CreateProjectViewModel
            {
                Name = "ValidProject",
                Description = "Test Description",
                ProjectLeads = users.Select(u => new SelectListItem
                {
                    Value = u.Id.ToString(),
                    Text = u.UserName
                }).ToList()
            };

            // Act
            var result = await controller.CreateProject(model);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Projects", redirectResult.ActionName);
            Assert.True(dbContext.Projects.Any(p => p.Name == "ValidProject"));
        }
    }
}
