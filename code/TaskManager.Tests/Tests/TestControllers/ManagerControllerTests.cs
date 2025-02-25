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
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace TaskManager.Tests.Tests.TestControllers
{
    public class ManagerControllerTests
    {
        private readonly Mock<UserManager<User>> _mockUserManager;
        private readonly Mock<RoleManager<IdentityRole<int>>> _mockRoleManager;
        private readonly ApplicationDbContext _dbContext;

        public ManagerControllerTests()
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
            var controller = new ManagerController(_dbContext, _mockUserManager.Object, _mockRoleManager.Object);

            var result = await controller.Users();

            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task Groups_ReturnsViewResult()
        {
            var controller = new ManagerController(_dbContext, _mockUserManager.Object, _mockRoleManager.Object);

            var result = await controller.Groups();

            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task Projects_ReturnsViewResult()
        {
            var user = setupGroupRequestsScenario(out var requestingUser, out var group1, out var group2, out var groupManager1, out var groupManager2, out var requestingProject1, out var requestingProject2, out var groupRequest1, out var groupRequest2);

            _dbContext.Users.Add(user);
            _dbContext.Users.Add(requestingUser);
            _dbContext.Groups.Add(group1);
            _dbContext.Groups.Add(group2);
            _dbContext.GroupManagers.Add(groupManager1);
            _dbContext.GroupManagers.Add(groupManager2);
            _dbContext.Projects.Add(requestingProject1);
            _dbContext.Projects.Add(requestingProject2);
            _dbContext.GroupRequests.Add(groupRequest1);
            _dbContext.GroupRequests.Add(groupRequest2);

            await _dbContext.SaveChangesAsync();

            _mockUserManager.Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(user.Id.ToString());

            var controller = new ManagerController(_dbContext, _mockUserManager.Object, _mockRoleManager.Object);

            var result = await controller.Projects();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<GroupRequest>>(viewResult.ViewData["GroupRequests"]);
            var model2 = Assert.IsAssignableFrom<List<GroupRequest>>(viewResult.ViewData["SentGroupRequests"]);
            Assert.NotNull(model);
            Assert.NotNull(model2);
            Assert.Single(model);
            Assert.Single(model2);
        }

        [Fact]
        public async Task CreateProject_ReturnsViewResult()
        {
            var controller = new ManagerController(_dbContext, _mockUserManager.Object, _mockRoleManager.Object);

            var result = await controller.CreateProject();

            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task CreateProjectWithArg_ReturnsViewResult_WithInvalidModelState()
        {
            var controller = new ManagerController(_dbContext, _mockUserManager.Object, _mockRoleManager.Object);

            controller.ModelState.AddModelError("Name", "Required");

            var result = await controller.CreateProject(new Project());

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.IsType<Project>(viewResult.Model);
        }

        [Fact]
        public async Task CreateProjectWithArg_ReturnsViewResult_WithEmptyUserId()
        {
            var controller = new ManagerController(_dbContext, _mockUserManager.Object, _mockRoleManager.Object);
            _mockUserManager.Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("");

            var result = await controller.CreateProject(new Project());

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.IsType<Project>(viewResult.Model);
            Assert.Single(controller.ModelState[""]!.Errors);
        }

        [Fact]
        public async Task CreateProjectWithArg_ReturnsRedirectToActionResult()
        {
            var controller = new ManagerController(_dbContext, _mockUserManager.Object, _mockRoleManager.Object);

            var user = new User
            {
                Id = 1,
                UserName = "TestUser",
                Email = "user@gmail.com"
            };

            _dbContext.Users.Add(user);

            await _dbContext.SaveChangesAsync();

            _mockUserManager.Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(user.Id.ToString());

            var result = await controller.CreateProject(new Project
            {
                Name = "TestProject",
                Description = "TestDescription",
                ProjectLeadId = 1
            });

            Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(1, _dbContext.Projects.First().ProjectCreatorId);
        }

        [Fact]
        public async Task ProjectDetails_ReturnsNotFound_WithNullProject()
        {
            var controller = new ManagerController(_dbContext, _mockUserManager.Object, _mockRoleManager.Object);

            var result = await controller.ProjectDetails(999);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task ProjectDetails_ReturnsViewResult()
        {
            var controller = new ManagerController(_dbContext, _mockUserManager.Object, _mockRoleManager.Object);

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
                PrimaryManagerId = 1,
                PrimaryManager = user
            };

            var unmanagedGroup = new Group
            {
                Id = 2,
                Name = "Test2Group",
                Description = "Test2Description",
                PrimaryManagerId = 2,
                PrimaryManager = otherUser
            };

            var anotherUnmanagedGroup = new Group
            {
                Id = 3,
                Name = "Test3Group",
                Description = "Test3Description",
                PrimaryManagerId = 2,
                PrimaryManager = otherUser
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
        public async Task EditProject_ReturnsNotFound_WithNullProject()
        {
            var controller = new ManagerController(_dbContext, _mockUserManager.Object, _mockRoleManager.Object);

            var result = await controller.EditProject(999);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task EditProject_ReturnsViewResult()
        {
            var controller = new ManagerController(_dbContext, _mockUserManager.Object, _mockRoleManager.Object);

            var project = new Project
            {
                Id = 1,
                Name = "TestProject",
                Description = "TestDescription",
                ProjectLeadId = 1
            };

            _dbContext.Projects.Add(project);
            await _dbContext.SaveChangesAsync();

            var result = await controller.EditProject(1);

            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task EditProjectWithArg_ReturnsBadRequest()
        {
            var controller = new ManagerController(_dbContext, _mockUserManager.Object, _mockRoleManager.Object);

            var result = await controller.EditProject(1, new Project { Id = 2, Description = "N/A" });

            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async Task EditProjectWithArg_ReturnsViewResult_WithInvalidModelState()
        {
            var controller = new ManagerController(_dbContext, _mockUserManager.Object, _mockRoleManager.Object);

            controller.ModelState.AddModelError("Name", "Required");

            var result = await controller.EditProject(1, new Project { Id = 1, Description = "N/A" });

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.IsType<Project>(viewResult.Model);
            Assert.False(controller.ModelState.IsValid);
        }

        [Fact]
        public async Task EditProjectWithArg_ReturnsViewResult_WithEmptyUserId()
        {
            var controller = new ManagerController(_dbContext, _mockUserManager.Object, _mockRoleManager.Object);

            _mockUserManager.Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("");

            var result = await controller.EditProject(1, new Project { Id = 1, Description = "N/A" });

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.IsType<Project>(viewResult.Model);
            Assert.Single(controller.ModelState[""]!.Errors);
        }

        [Fact]
        public async Task EditProjectWithArg_ReturnsRedirectToActionResult()
        {
            var controller = new ManagerController(_dbContext, _mockUserManager.Object, _mockRoleManager.Object);
            var project = new Project
            {
                Id = 1,
                Name = "TestProject",
                Description = "TestDescription",
                ProjectLeadId = 1
            };

            _dbContext.Projects.Add(project);
            await _dbContext.SaveChangesAsync();

            _mockUserManager.Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(project.ProjectLeadId.ToString());
            _dbContext.Entry(project).State = EntityState.Detached;

            var result = await controller.EditProject(1, new Project
            {
                Id = 1,
                Name = "Test2Project",
                Description = "Test2Description",
                ProjectLeadId = 1
            });

            Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Test2Project", _dbContext.Projects.First().Name);
        }

        [Fact]
        public async Task DeleteProject_ReturnsRedirectToAction()
        {
            var controller = new ManagerController(_dbContext, _mockUserManager.Object, _mockRoleManager.Object);

            var project = new Project
            {
                Id = 1,
                Name = "TestProject",
                Description = "TestDescription"
            };

            _dbContext.Projects.Add(project);
            await _dbContext.SaveChangesAsync();

            var result = await controller.DeleteProject(1);

            Assert.IsType<RedirectToActionResult>(result);
        }

        [Fact]
        public async Task RequestGroupToProject_ReturnsUnauthorized_WithEmptyUserId()
        {
            var controller = new ManagerController(_dbContext, _mockUserManager.Object, _mockRoleManager.Object);

            _mockUserManager.Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("");

            var result = await controller.RequestGroupToProject(1, 1);

            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public async Task RequestGroupToProject_ReturnsRedirectToActionResult_WithNoFoundProject()
        {
            var controller = new ManagerController(_dbContext, _mockUserManager.Object, _mockRoleManager.Object);
            controller.TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());

            var user = new User
            {
                Id = 1,
                UserName = "TestUser",
                Email = "user@gmail.com"
            };

            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            _mockUserManager.Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(user.Id.ToString());

            var result = await controller.RequestGroupToProject(1, 1);

            Assert.IsType<RedirectToActionResult>(result);
            Assert.True(controller.TempData.ContainsKey("ErrorMessage"));
            Assert.Equal("No group selected", controller.TempData["ErrorMessage"]);
        }

        [Fact]
        public async Task RequestGroupToProject_ReturnsRedirectToActionResult_WithNoFoundGroup()
        {
            var controller = new ManagerController(_dbContext, _mockUserManager.Object, _mockRoleManager.Object);
            controller.TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());

            var user = new User
            {
                Id = 1,
                UserName = "TestUser",
                Email = "user@gmail.com"
            };
            var project = new Project
            {
                Id = 1,
                Name = "TestProject",
                Description = "TestDescription",
                ProjectLeadId = 1,
                ProjectLead = user
            };

            _dbContext.Users.Add(user);
            _dbContext.Projects.Add(project);
            await _dbContext.SaveChangesAsync();

            _mockUserManager.Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(user.Id.ToString());

            var result = await controller.RequestGroupToProject(1, 1);

            Assert.IsType<RedirectToActionResult>(result);
            Assert.True(controller.TempData.ContainsKey("ErrorMessage"));
            Assert.Equal("No group selected", controller.TempData["ErrorMessage"]);
        }

        [Fact]
        public async Task RequestGroupToProject_ReturnsRedirectToActionResult_WithExistingRequest()
        {
            var controller = new ManagerController(_dbContext, _mockUserManager.Object, _mockRoleManager.Object);
            controller.TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());

            var user = new User
            {
                Id = 1,
                UserName = "TestUser",
                Email = "user@gmail.com"
            };

            var group = new Group
            {
                Id = 1,
                Name = "TestGroup",
                Description = "TestDescription",
                PrimaryManagerId = 1,
                PrimaryManager = user
            };

            var project = new Project
            {
                Id = 1,
                Name = "TestProject",
                Description = "TestDescription",
                ProjectLeadId = 1,
                ProjectLead = user
            };

            var groupRequest = new GroupRequest
            {
                Id = 1,
                GroupId = 1,
                Group = group,
                SenderId = 1,
                ProjectId = 1,
                Project = project,
                Response = null
            };

            _dbContext.Users.Add(user);
            _dbContext.Projects.Add(project);
            _dbContext.Groups.Add(group);
            _dbContext.GroupRequests.Add(groupRequest);
            await _dbContext.SaveChangesAsync();

            _mockUserManager.Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(user.Id.ToString());

            var result = await controller.RequestGroupToProject(1, 1);

            Assert.IsType<RedirectToActionResult>(result);
            Assert.True(controller.TempData.ContainsKey("ErrorMessage"));
            Assert.Equal("You have already requested to assign this group to the project.", controller.TempData["ErrorMessage"]);
        }

        [Fact]
        public async Task RequestGroupToProject_ReturnsRedirectToActionResult()
        {
            var controller = new ManagerController(_dbContext, _mockUserManager.Object, _mockRoleManager.Object);
            controller.TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());

            var user = new User
            {
                Id = 1,
                UserName = "TestUser",
                Email = "user@gmail.com"
            };

            var group = new Group
            {
                Id = 1,
                Name = "TestGroup",
                Description = "TestDescription",
            };

            var project = new Project
            {
                Id = 1,
                Name = "TestProject",
                Description = "TestDescription",
                ProjectLeadId = 1,
                ProjectLead = user
            };

            _dbContext.Users.Add(user);
            _dbContext.Projects.Add(project);
            _dbContext.Groups.Add(group);
            await _dbContext.SaveChangesAsync();

            _mockUserManager.Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(user.Id.ToString());

            var result = await controller.RequestGroupToProject(1, 1);

            Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(1, _dbContext.GroupRequests.First().GroupId);
            Assert.Equal(1, _dbContext.GroupRequests.First().ProjectId);
            Assert.True(controller.TempData.ContainsKey("SuccessMessage"));
            Assert.Equal("You have successfully requested to assign this group to the project.", controller.TempData["SuccessMessage"]);
        }

        [Fact]
        public async Task AssignGroupToProject_ReturnsRedirectToActionResult_WithNoFoundProject()
        {
            var controller = new ManagerController(_dbContext, _mockUserManager.Object, _mockRoleManager.Object);
            controller.TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());

            var user = new User
            {
                Id = 1,
                UserName = "TestUser",
                Email = "user@gmail.com"
            };

            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            _mockUserManager.Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(user.Id.ToString());

            var result = await controller.AssignGroupToProject(1, 1);

            Assert.IsType<RedirectToActionResult>(result);
            Assert.True(controller.TempData.ContainsKey("AssignedErrorMessage"));
            Assert.Equal("No group selected", controller.TempData["AssignedErrorMessage"]);
        }

        [Fact]
        public async Task AssignGroupToProject_ReturnsRedirectToActionResult_WithNoFoundGroup()
        {
            var controller = new ManagerController(_dbContext, _mockUserManager.Object, _mockRoleManager.Object);
            controller.TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());

            var user = new User
            {
                Id = 1,
                UserName = "TestUser",
                Email = "user@gmail.com"
            };

            var project = new Project
            {
                Id = 1,
                Name = "TestProject",
                Description = "TestDescription",
                ProjectLeadId = 1,
                ProjectLead = user
            };

            _dbContext.Users.Add(user);
            _dbContext.Projects.Add(project);
            await _dbContext.SaveChangesAsync();

            _mockUserManager.Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(user.Id.ToString());

            var result = await controller.AssignGroupToProject(1, 1);

            Assert.IsType<RedirectToActionResult>(result);
            Assert.True(controller.TempData.ContainsKey("AssignedErrorMessage"));
            Assert.Equal("No group selected", controller.TempData["AssignedErrorMessage"]);
        }

        [Fact]
        public async Task AssignGroupToProject_ReturnsRedirectToActionResult()
        {
            var controller = new ManagerController(_dbContext, _mockUserManager.Object, _mockRoleManager.Object);
            controller.TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());

            var user = new User
            {
                Id = 1,
                UserName = "TestUser",
                Email = "user@gmail.com"
            };

            var group = new Group
            {
                Id = 1,
                Name = "TestGroup",
                Description = "TestDescription",
            };

            var project = new Project
            {
                Id = 1,
                Name = "TestProject",
                Description = "TestDescription",
                ProjectLeadId = 1,
                ProjectLead = user
            };

            _dbContext.Users.Add(user);
            _dbContext.Projects.Add(project);
            _dbContext.Groups.Add(group);
            await _dbContext.SaveChangesAsync();

            _mockUserManager.Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(user.Id.ToString());

            var result = await controller.AssignGroupToProject(1, 1);

            Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(1, _dbContext.GroupProjects.First().GroupId);
            Assert.Equal(1, _dbContext.GroupProjects.First().ProjectId);
        }

        [Fact]
        public async Task RemoveGroupFromProject_ReturnsNotFound_WithNoFoundProject()
        {
            var controller = new ManagerController(_dbContext, _mockUserManager.Object, _mockRoleManager.Object);

            var result = await controller.RemoveGroupFromProject(1, 1);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task RemoveGroupFromProject_ReturnsRedirectToAction()
        {
            var controller = new ManagerController(_dbContext, _mockUserManager.Object, _mockRoleManager.Object);

            var group = new Group
            {
                Id = 1,
                Name = "TestGroup",
                Description = "TestDescription"
            };

            var project = new Project
            {
                Id = 1,
                Name = "TestProject",
                Description = "TestDescription"
            };

            var groupProject = new GroupProject
            {
                GroupId = 1,
                ProjectId = 1
            };

            _dbContext.Groups.Add(group);
            _dbContext.Projects.Add(project);
            _dbContext.GroupProjects.Add(groupProject);
            await _dbContext.SaveChangesAsync();

            var result = await controller.RemoveGroupFromProject(1, 1);

            Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(0, _dbContext.GroupProjects.Count());
        }

        [Fact]
        public async Task AcceptGroupRequest_ReturnsNotFound_WithNoFoundRequest()
        {
            var controller = new ManagerController(_dbContext, _mockUserManager.Object, _mockRoleManager.Object);

            var result = await controller.AcceptRequest(1);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task AcceptGroupRequest_ReturnsRedirectToAction()
        {
            var controller = new ManagerController(_dbContext, _mockUserManager.Object, _mockRoleManager.Object);
            controller.TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());

            var user = new User
            {
                Id = 1,
                UserName = "TestUser",
                Email = "user@gmail.com"
            };

            var group = new Group
            {
                Id = 1,
                Name = "TestGroup",
                Description = "TestDescription",
                PrimaryManagerId = 1,
                PrimaryManager = user
            };

            var project = new Project
            {
                Id = 1,
                Name = "TestProject",
                Description = "TestDescription",
                ProjectLeadId = 1,
                ProjectLead = user
            };

            var groupRequest = new GroupRequest
            {
                Id = 1,
                GroupId = 1,
                ProjectId = 1,
                Response = null
            };

            _dbContext.Users.Add(user);
            _dbContext.Groups.Add(group);
            _dbContext.Projects.Add(project);
            _dbContext.GroupRequests.Add(groupRequest);
            await _dbContext.SaveChangesAsync();

            var result = await controller.AcceptRequest(1);

            Assert.IsType<RedirectToActionResult>(result);
            Assert.True(_dbContext.GroupRequests.First().Response);
            Assert.True(controller.TempData.ContainsKey("SuccessMessage"));
            Assert.Equal("The group request has been accepted.", controller.TempData["SuccessMessage"]);
        }

        [Fact]
        public async Task DenyRequest_ReturnsNotFound_WithNoFoundRequest()
        {
            var controller = new ManagerController(_dbContext, _mockUserManager.Object, _mockRoleManager.Object);

            var result = await controller.DenyRequest(1);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DenyRequest_ReturnsRedirectToAction()
        {
            var controller = new ManagerController(_dbContext, _mockUserManager.Object, _mockRoleManager.Object);
            controller.TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());

            var user = new User
            {
                Id = 1,
                UserName = "TestUser",
                Email = "user@gmail.com"
            };

            var group = new Group
            {
                Id = 1,
                Name = "TestGroup",
                Description = "TestDescription",
                PrimaryManagerId = 1,
                PrimaryManager = user
            };

            var project = new Project
            {
                Id = 1,
                Name = "TestProject",
                Description = "TestDescription",
                ProjectLeadId = 1,
                ProjectLead = user
            };

            var groupRequest = new GroupRequest
            {
                Id = 1,
                GroupId = 1,
                ProjectId = 1,
                Response = null
            };

            _dbContext.Users.Add(user);
            _dbContext.Groups.Add(group);
            _dbContext.Projects.Add(project);
            _dbContext.GroupRequests.Add(groupRequest);
            await _dbContext.SaveChangesAsync();

            var result = await controller.DenyRequest(1);

            Assert.IsType<RedirectToActionResult>(result);
            Assert.False(_dbContext.GroupRequests.First().Response);
            Assert.True(controller.TempData.ContainsKey("ErrorMessage"));
            Assert.Equal("The group request has been denied.", controller.TempData["ErrorMessage"]);
        }

        [Fact]
        public async Task DeleteGroupRequest_ReturnsRedirectToAction()
        {
            var controller = new ManagerController(_dbContext, _mockUserManager.Object, _mockRoleManager.Object);

            var groupRequest = new GroupRequest
            {
                Id = 1,
                GroupId = 1,
                ProjectId = 1,
                Response = null
            };

            _dbContext.GroupRequests.Add(groupRequest);
            await _dbContext.SaveChangesAsync();

            var result = await controller.DeleteGroupRequest(1);

            Assert.IsType<RedirectToActionResult>(result);
            Assert.Empty(_dbContext.GroupRequests);
        }

        private static User setupGroupRequestsScenario(out User requestingUser, out Group group1, out Group group2,
            out GroupManager groupManager1, out GroupManager groupManager2, out Project requestingProject1,
            out Project requestingProject2, out GroupRequest groupRequest1, out GroupRequest groupRequest2)
        {
            var user = new User
            {
                Id = 1,
                UserName = "TestUser",
                Email = "user@gmail.com",
            };

            requestingUser = new User
            {
                Id = 2,
                UserName = "RequestingUser",
                Email = "user2@gmail.com"
            };

            group1 = new Group
            {
                Id = 1,
                Name = "TestGroup",
                Description = "TestDescription",
                PrimaryManagerId = 1,
                PrimaryManager = user
            };

            group2 = new Group
            {
                Id = 2,
                Name = "Test2Group",
                Description = "Test2Description",
                PrimaryManagerId = 2,
                PrimaryManager = requestingUser
            };

            groupManager1 = new GroupManager
            {
                GroupId = 1,
                Group = group1,
                UserId = 1,
                User = user
            };

            groupManager2 = new GroupManager
            {
                GroupId = 2,
                Group = group2,
                UserId = 2,
                User = requestingUser
            };

            requestingProject1 = new Project
            {
                Id = 1,
                Name = "TestProject",
                Description = "TestDescription",
                ProjectLeadId = 2,
                ProjectLead = requestingUser
            };

            requestingProject2 = new Project
            {
                Id = 2,
                Name = "Test2Project",
                Description = "Test2Description",
                ProjectLeadId = 1,
                ProjectLead = user
            };

            groupRequest1 = new GroupRequest
            {
                Id = 1,
                GroupId = 1,
                Group = group1,
                SenderId = 2,
                ProjectId = 1,
                Project = requestingProject1,
                Response = null
            };

            groupRequest2 = new GroupRequest
            {
                Id = 2,
                GroupId = 2,
                Group = group2,
                SenderId = 1,
                ProjectId = 2,
                Project = requestingProject2,
                Response = false
            };
            return user;
        }
    }
}
