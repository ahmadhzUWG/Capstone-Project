using System.Security.Claims;
using System.Windows.Forms;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using Moq;
using TaskManagerWebsite.Controllers;
using TaskManagerWebsite.Data;
using TaskManagerWebsite.Models;
using TaskManagerWebsite.ViewModels.ProjectViewModels;
using Task = System.Threading.Tasks.Task;

namespace TaskManager.Tests.Tests.TestControllers
{
    public class EmployeeControllerTests
    {
        private ApplicationDbContext CreateContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(dbName)
                .Options;
            return new ApplicationDbContext(options);
        }

        private UserManager<User> CreateUserManager(User user, bool isAdmin = false)
        {
            var store = new Mock<IUserStore<User>>();
            var mockUM = new Mock<UserManager<User>>(
                store.Object, null, null, null, null, null, null, null, null);
            mockUM.Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>()))
                .Returns(user.Id.ToString());
            mockUM.Setup(um => um.FindByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(user);
            mockUM.Setup(um => um.IsInRoleAsync(It.IsAny<User>(), "Admin"))
                .ReturnsAsync(isAdmin);
            mockUM.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);
            return mockUM.Object;
        }

        private RoleManager<IdentityRole<int>> CreateRoleManager()
        {
            var roleStore = new Mock<IRoleStore<IdentityRole<int>>>();
            return new RoleManager<IdentityRole<int>>(roleStore.Object, null, null, null, null);
        }

        private ControllerContext CreateControllerContext(User user)
        {
            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            }, "TestAuth");
            return new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
            };
        }

        private void InitializeTempData(Controller controller)
        {
            controller.TempData =
                new TempDataDictionary(controller.ControllerContext.HttpContext, Mock.Of<ITempDataProvider>());
        }

        // ------------------- Users & Groups -------------------

        [Fact]
        public async Task Users_ReturnsAllUsers()
        {
            using var context = CreateContext(Guid.NewGuid().ToString());
            context.Users.AddRange(new List<User>
            {
                new User { Id = 1, UserName = "user1" },
                new User { Id = 2, UserName = "user2" }
            });
            await context.SaveChangesAsync();

            var currentUser = new User { Id = 1, UserName = "user1" };
            var controller = new EmployeeController(context, CreateUserManager(currentUser), CreateRoleManager())
            {
                ControllerContext = CreateControllerContext(currentUser)
            };
            InitializeTempData(controller);

            var result = await controller.Users() as ViewResult;
            var model = result.Model as List<User>;
            Assert.Equal(2, model.Count);
        }

        [Fact]
        public async Task Groups_ReturnsAllGroups()
        {
            using var context = CreateContext(Guid.NewGuid().ToString());
            context.Groups.AddRange(new List<Group>
            {
                new Group { Id = 1, Name = "Group1", Description = "Desc1" },
                new Group { Id = 2, Name = "Group2", Description = "Desc2" }
            });
            await context.SaveChangesAsync();

            var currentUser = new User { Id = 1, UserName = "user1" };
            var controller = new EmployeeController(context, CreateUserManager(currentUser), CreateRoleManager())
            {
                ControllerContext = CreateControllerContext(currentUser)
            };
            InitializeTempData(controller);

            var result = await controller.Groups() as ViewResult;
            var model = result.Model as List<Group>;
            Assert.Equal(2, model.Count);
        }

        // ------------------- GroupDetails -------------------

        [Fact]
        public async Task GroupDetails_ReturnsNotFound_IfGroupMissing()
        {
            using var context = CreateContext(Guid.NewGuid().ToString());
            var currentUser = new User { Id = 1, UserName = "user1" };
            var controller = new EmployeeController(context, CreateUserManager(currentUser), CreateRoleManager())
                { ControllerContext = CreateControllerContext(currentUser) };
            InitializeTempData(controller);

            var result = await controller.GroupDetails(999);
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GroupDetails_ReturnsView_WithViewBagValues()
        {
            using var context = CreateContext(Guid.NewGuid().ToString());
            var group = new Group { Id = 1, Name = "Group1", Description = "Desc1", ManagerId = 2 };
            context.Groups.Add(group);
            context.UserGroups.AddRange(new List<UserGroup>
            {
                new UserGroup { GroupId = 1, UserId = 3, Role = "Member" },
                new UserGroup { GroupId = 1, UserId = 4, Role = "Member" }
            });
            context.Users.AddRange(new List<User>
            {
                new User { Id = 2, UserName = "manager" },
                new User { Id = 3, UserName = "user3" },
                new User { Id = 4, UserName = "user4" },
                new User { Id = 5, UserName = "user5" }
            });
            await context.SaveChangesAsync();

            var currentUser = new User { Id = 2, UserName = "manager" };
            var controller = new EmployeeController(context, CreateUserManager(currentUser), CreateRoleManager())
                { ControllerContext = CreateControllerContext(currentUser) };
            InitializeTempData(controller);

            var result = await controller.GroupDetails(1) as ViewResult;
            Assert.NotNull(result);
            Assert.NotNull(result.ViewData["Users"]);
            Assert.NotNull(result.ViewData["AvailableManagers"]);
            Assert.NotNull(result.ViewData["GroupUsers"]);
        }

        // ------------------- AddUserToGroup & RemoveUserFromGroup -------------------

        [Fact]
        public async Task AddUserToGroup_ReturnsNotFound_IfGroupOrUserMissing()
        {
            using var context = CreateContext(Guid.NewGuid().ToString());
            var currentUser = new User { Id = 1, UserName = "user1" };
            var controller = new EmployeeController(context, CreateUserManager(currentUser), CreateRoleManager())
                { ControllerContext = CreateControllerContext(currentUser) };
            InitializeTempData(controller);

            var result = await controller.AddUserToGroup(1, 1);
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task AddUserToGroup_AddsUser_IfNotAlreadyMember()
        {
            using var context = CreateContext(Guid.NewGuid().ToString());
            context.Groups.Add(new Group { Id = 1, Name = "Group1", Description = "Desc1" });
            context.Users.AddRange(new List<User>
            {
                new User { Id = 1, UserName = "user1" },
                new User { Id = 2, UserName = "user2" }
            });
            await context.SaveChangesAsync();

            var currentUser = new User { Id = 1, UserName = "user1" };
            var controller = new EmployeeController(context, CreateUserManager(currentUser), CreateRoleManager())
                { ControllerContext = CreateControllerContext(currentUser) };
            InitializeTempData(controller);

            var result = await controller.AddUserToGroup(1, 2) as RedirectToActionResult;
            Assert.Equal("GroupDetails", result.ActionName);
            Assert.Single(context.UserGroups.Where(ug => ug.UserId == 2 && ug.GroupId == 1));
        }

        [Fact]
        public async Task RemoveUserFromGroup_RemovesMembership_IfExists()
        {
            using var context = CreateContext(Guid.NewGuid().ToString());
            context.UserGroups.Add(new UserGroup { GroupId = 1, UserId = 1, Role = "Member" });
            await context.SaveChangesAsync();

            var currentUser = new User { Id = 1, UserName = "user1" };
            var controller = new EmployeeController(context, CreateUserManager(currentUser), CreateRoleManager())
                { ControllerContext = CreateControllerContext(currentUser) };
            InitializeTempData(controller);

            var result = await controller.RemoveUserFromGroup(1, 1) as RedirectToActionResult;
            Assert.Equal("GroupDetails", result.ActionName);
            Assert.Empty(context.UserGroups);
        }

        // ------------------- ChangeManager -------------------

        [Fact]
        public async Task ChangeManager_ReturnsNotFound_IfGroupMissing()
        {
            using var context = CreateContext(Guid.NewGuid().ToString());
            var currentUser = new User { Id = 1, UserName = "user1" };
            var controller = new EmployeeController(context, CreateUserManager(currentUser), CreateRoleManager())
                { ControllerContext = CreateControllerContext(currentUser) };
            InitializeTempData(controller);

            var result = await controller.ChangeManager(999, 2);
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task ChangeManager_ReturnsNotFound_IfNewManagerMissing()
        {
            using var context = CreateContext(Guid.NewGuid().ToString());
            context.Groups.Add(new Group { Id = 1, Name = "Group1", Description = "Desc" });
            await context.SaveChangesAsync();

            var currentUser = new User { Id = 1, UserName = "user1" };
            var controller = new EmployeeController(context, CreateUserManager(currentUser), CreateRoleManager())
                { ControllerContext = CreateControllerContext(currentUser) };
            InitializeTempData(controller);

            var result = await controller.ChangeManager(1, 999);
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task ChangeManager_UpdatesManager_IfValid()
        {
            using var context = CreateContext(Guid.NewGuid().ToString());
            var group = new Group { Id = 1, Name = "Group1", Description = "Desc" };
            context.Groups.Add(group);
            context.Users.AddRange(new List<User>
            {
                new User { Id = 1, UserName = "user1" },
                new User { Id = 2, UserName = "user2" }
            });
            await context.SaveChangesAsync();

            var currentUser = new User { Id = 1, UserName = "user1" };
            var controller = new EmployeeController(context, CreateUserManager(currentUser), CreateRoleManager())
                { ControllerContext = CreateControllerContext(currentUser) };
            InitializeTempData(controller);

            var result = await controller.ChangeManager(1, 2) as RedirectToActionResult;
            Assert.Equal("GroupDetails", result.ActionName);
            var updatedGroup = await context.Groups.FindAsync(1);
            Assert.Equal(2, updatedGroup.ManagerId);
        }

        [Fact]
        public async Task ChangeManager_ChangesManagerRoleAndDemotesPrevious_IfBothExist()
        {
            using var context = CreateContext(Guid.NewGuid().ToString());
            var group = new Group { Id = 1, Name = "Group1", Description = "Desc", ManagerId = 1 };
            context.Groups.Add(group);
            context.Users.AddRange(
                new User { Id = 1, UserName = "OldManager" },
                new User { Id = 2, UserName = "NewManager" }
            );
            context.UserGroups.AddRange(
                new UserGroup { GroupId = 1, UserId = 1, Role = "Manager" },
                new UserGroup { GroupId = 1, UserId = 2, Role = "Member" }
            );
            await context.SaveChangesAsync();

            var controller = new EmployeeController(context, CreateUserManager(new User { Id = 2 }), CreateRoleManager())
            {
                ControllerContext = CreateControllerContext(new User { Id = 2 })
            };
            InitializeTempData(controller);

            var result = await controller.ChangeManager(1, 2);

            Assert.IsType<RedirectToActionResult>(result);
            var updatedGroup = await context.Groups.FindAsync(1);
            Assert.Equal(2, updatedGroup.ManagerId);

            var oldManager = await context.UserGroups.FirstOrDefaultAsync(u => u.UserId == 1 && u.GroupId == 1);
            var newManager = await context.UserGroups.FirstOrDefaultAsync(u => u.UserId == 2 && u.GroupId == 1);
            Assert.Equal("Member", oldManager.Role);
            Assert.Equal("Manager", newManager.Role);
        }

        // ------------------- CreateProject (GET & POST) -------------------

        [Fact]
        public async Task CreateProject_Get_ReturnsView_WithProjectLeadsAndGroups()
        {
            using var context = CreateContext(Guid.NewGuid().ToString());
            context.Users.Add(new User { Id = 1, UserName = "user1" });
            context.Groups.Add(new Group { Id = 1, Name = "Group1", Description = "Desc" });
            await context.SaveChangesAsync();

            var currentUser = new User { Id = 1, UserName = "user1" };
            var controller = new EmployeeController(context, CreateUserManager(currentUser), CreateRoleManager())
                { ControllerContext = CreateControllerContext(currentUser) };
            InitializeTempData(controller);

            var result = await controller.CreateProject() as ViewResult;
            Assert.NotNull(result.ViewData["ProjectLead"]);
            Assert.NotNull(result.ViewData["Groups"]);
        }

        [Fact]
        public async Task CreateProject_Post_ReturnsView_IfInvalid()
        {
            using var context = CreateContext(Guid.NewGuid().ToString());
            context.Users.Add(new User { Id = 1, UserName = "user1" });
            await context.SaveChangesAsync();

            var model = new CreateProjectViewModel { Name = "NewProj", Description = "NewDesc", ProjectLeadId = 1 };
            var currentUser = new User { Id = 1, UserName = "user1" };
            var controller = new EmployeeController(context, CreateUserManager(currentUser), CreateRoleManager())
                { ControllerContext = CreateControllerContext(currentUser) };
            controller.ModelState.AddModelError("Error", "Invalid");
            InitializeTempData(controller);

            var result = await controller.CreateProject(model) as ViewResult;
            Assert.NotNull(result);
        }

        [Fact]
        public async Task CreateProject_Post_ReturnsError_IfUserNotAuthenticated()
        {
            using var context = CreateContext(Guid.NewGuid().ToString());
            context.Users.Add(new User { Id = 1, UserName = "user1" });
            await context.SaveChangesAsync();

            var model = new CreateProjectViewModel
            {
                Name = "Test Project",
                Description = "Some description",
                ProjectLeadId = 1
            };

            var user = new User { Id = 1, UserName = "user1" };

            var mockUserManager = new Mock<UserManager<User>>(new Mock<IUserStore<User>>().Object,
                null, null, null, null, null, null, null, null);

            mockUserManager.Setup(m => m.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(string.Empty);
            mockUserManager.Setup(m => m.FindByIdAsync(It.IsAny<string>())).ReturnsAsync((User)null);

            var controller = new EmployeeController(context, mockUserManager.Object, CreateRoleManager())
            {
                ControllerContext = CreateControllerContext(user)
            };
            InitializeTempData(controller);

            controller.ModelState.Clear();

            var result = await controller.CreateProject(model) as ViewResult;

            Assert.NotNull(result);
            Assert.True(controller.ModelState.ErrorCount > 0);
            Assert.Contains(controller.ModelState, m => m.Value.Errors.Any());
        }

        [Fact]
        public async Task CreateProject_Post_CreatesProjectAndAssignsGroups_IfValid()
        {
            using var context = CreateContext(Guid.NewGuid().ToString());

            var user = new User { Id = 1, UserName = "user1" };
            var group = new Group { Id = 1, Name = "Group1", Description = "Test group", ManagerId = user.Id };

            context.Users.Add(user);
            context.Groups.Add(group);
            await context.SaveChangesAsync();

            var model = new CreateProjectViewModel
            {
                Name = "Test Project",
                Description = "Test Desc",
                ProjectLeadId = user.Id
            };

            var controller = new EmployeeController(context, CreateUserManager(user), CreateRoleManager())
            {
                ControllerContext = CreateControllerContext(user)
            };
            InitializeTempData(controller);

            var formCollection = new Mock<IFormCollection>();
            formCollection.Setup(f => f["GroupId"]).Returns(new Microsoft.Extensions.Primitives.StringValues("1"));

            var mockRequest = new Mock<HttpRequest>();
            mockRequest.Setup(r => r.Form).Returns(formCollection.Object);

            var mockContext = new Mock<HttpContext>();
            mockContext.Setup(c => c.Request).Returns(mockRequest.Object);
            controller.ControllerContext.HttpContext = mockContext.Object;

            var result = await controller.CreateProject(model);

            Assert.IsType<RedirectToActionResult>(result);
            Assert.Single(context.Projects);
            Assert.Single(context.GroupProjects);
        }

        // ------------------- ProjectDetails -------------------

        [Fact]
        public async Task ProjectDetails_ReturnsNotFound_IfProjectMissing()
        {
            using var context = CreateContext(Guid.NewGuid().ToString());
            var currentUser = new User { Id = 1, UserName = "user1" };
            var controller = new EmployeeController(context, CreateUserManager(currentUser), CreateRoleManager())
                { ControllerContext = CreateControllerContext(currentUser) };
            InitializeTempData(controller);

            var result = await controller.ProjectDetails(999);
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task ProjectDetails_ReturnsView_IfValid()
        {
            using var context = CreateContext(Guid.NewGuid().ToString());
            context.Groups.AddRange(new Group
                    { Id = 1, Name = "Group1", Description = "D1", ManagerId = 2 },
                new Group { Id = 2, Name = "Group2", Description = "D2", ManagerId = 3 });
            context.Users.AddRange(new User { Id = 1, UserName = "user1" },
                new User { Id = 2, UserName = "manager1" },
                new User { Id = 3, UserName = "manager2" });
            var proj = new Project
            {
                Id = 1,
                Name = "Proj1",
                Description = "D1",
                ProjectLeadId = 1,
                ProjectGroups = new List<GroupProject>
                {
                    new GroupProject { GroupId = 1 }
                }
            };
            context.Projects.Add(proj);
            await context.SaveChangesAsync();

            var currentUser = new User { Id = 1, UserName = "user1" };
            var controller = new EmployeeController(context, CreateUserManager(currentUser), CreateRoleManager())
                { ControllerContext = CreateControllerContext(currentUser) };
            InitializeTempData(controller);

            var result = await controller.ProjectDetails(1) as ViewResult;
            Assert.NotNull(result);
        }

        // ------------------- EditProject -------------------

        [Fact]
        public async Task EditProject_Get_ReturnsNotFound_IfProjectMissing()
        {
            using var context = CreateContext(Guid.NewGuid().ToString());
            var currentUser = new User { Id = 1, UserName = "user1" };
            var controller = new EmployeeController(context, CreateUserManager(currentUser), CreateRoleManager())
                { ControllerContext = CreateControllerContext(currentUser) };
            InitializeTempData(controller);

            var result = await controller.EditProject(999);
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task EditProject_Get_ReturnsView_IfFound()
        {
            using var context = CreateContext(Guid.NewGuid().ToString());
            context.Projects.Add(new Project { Id = 1, Name = "Proj1", Description = "D1", ProjectLeadId = 1 });
            await context.SaveChangesAsync();

            var currentUser = new User { Id = 1, UserName = "user1" };
            var controller = new EmployeeController(context, CreateUserManager(currentUser), CreateRoleManager())
                { ControllerContext = CreateControllerContext(currentUser) };
            InitializeTempData(controller);

            var result = await controller.EditProject(1) as ViewResult;
            Assert.NotNull(result);
        }

        [Fact]
        public async Task EditProject_Post_ReturnsBadRequest_IfIdMismatch()
        {
            using var context = CreateContext(Guid.NewGuid().ToString());
            context.Projects.Add(new Project { Id = 1, Name = "Proj1", Description = "D1", ProjectLeadId = 1 });
            await context.SaveChangesAsync();

            var currentUser = new User { Id = 1, UserName = "user1" };
            var controller = new EmployeeController(context, CreateUserManager(currentUser), CreateRoleManager())
                { ControllerContext = CreateControllerContext(currentUser) };
            InitializeTempData(controller);

            var proj = new Project { Id = 1, Name = "Proj1", Description = "D1", ProjectLeadId = 1 };
            var result = await controller.EditProject(2, proj);
            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async Task EditProject_Post_UpdatesProject_IfValid()
        {
            using var context = CreateContext(Guid.NewGuid().ToString());
            var proj = new Project { Id = 1, Name = "Proj1", Description = "D1", ProjectLeadId = 1 };
            context.Projects.Add(proj);
            await context.SaveChangesAsync();

            var currentUser = new User { Id = 1, UserName = "user1" };
            var controller = new EmployeeController(context, CreateUserManager(currentUser), CreateRoleManager())
                { ControllerContext = CreateControllerContext(currentUser) };
            InitializeTempData(controller);

            proj.Name = "Updated";
            var result = await controller.EditProject(1, proj) as RedirectToActionResult;
            Assert.Equal("ProjectDetails", result.ActionName);
            Assert.Equal("Updated", (await context.Projects.FindAsync(1)).Name);
        }

        [Fact]
        public async Task EditProject_Post_ReturnsView_IfModelStateInvalid()
        {
            using var context = CreateContext(Guid.NewGuid().ToString());
            var project = new Project { Id = 1, Name = "Project1", Description = "Desc", ProjectLeadId = 1 };
            context.Projects.Add(project);
            context.Users.Add(new User { Id = 1, UserName = "user1" });
            await context.SaveChangesAsync();

            var controller = new EmployeeController(context, CreateUserManager(new User { Id = 1 }), CreateRoleManager())
            {
                ControllerContext = CreateControllerContext(new User { Id = 1 })
            };
            InitializeTempData(controller);
            controller.ModelState.AddModelError("Error", "Invalid");

            var result = await controller.EditProject(1, project);

            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task DeleteProject_Post_DeletesProject_IfExists()
        {
            using var context = CreateContext(Guid.NewGuid().ToString());
            context.Projects.Add(new Project { Id = 1, Name = "Proj1", Description = "D1", ProjectLeadId = 1 });
            await context.SaveChangesAsync();

            var currentUser = new User { Id = 1, UserName = "user1" };
            var controller = new EmployeeController(context, CreateUserManager(currentUser), CreateRoleManager())
                { ControllerContext = CreateControllerContext(currentUser) };
            InitializeTempData(controller);

            var result = await controller.DeleteProject(1) as RedirectToActionResult;
            Assert.Equal("Projects", result.ActionName);
            Assert.Empty(context.Projects);
        }

        // ------------------- RequestGroupToProject -------------------

        [Fact]
        public async Task RequestGroupToProject_ReturnsUnauthorized_IfUserMissing()
        {
            using var context = CreateContext(Guid.NewGuid().ToString());
            var umMock = new Mock<UserManager<User>>(new Mock<IUserStore<User>>().Object, null, null, null, null, null,
                null, null, null);
            umMock.Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("");
            var controller = new EmployeeController(context, umMock.Object, CreateRoleManager());
            var result = await controller.RequestGroupToProject(1, 1);
            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public async Task RequestGroupToProject_ReturnsError_IfProjectOrGroupMissing()
        {
            using var context = CreateContext(Guid.NewGuid().ToString());
            context.Users.Add(new User { Id = 1, UserName = "user1" });
            await context.SaveChangesAsync();

            var currentUser = new User { Id = 1, UserName = "user1" };
            var controller = new EmployeeController(context, CreateUserManager(currentUser), CreateRoleManager())
                { ControllerContext = CreateControllerContext(currentUser) };
            InitializeTempData(controller);

            var result = await controller.RequestGroupToProject(999, 999) as RedirectToActionResult;
            Assert.Equal("ProjectDetails", result.ActionName);
        }

        [Fact]
        public async Task RequestGroupToProject_CreatesRequest_IfValid()
        {
            using var context = CreateContext(Guid.NewGuid().ToString());
            context.Users.Add(new User { Id = 1, UserName = "user1" });
            var proj = new Project
            {
                Id = 1, Name = "Proj1", Description = "D1", ProjectLeadId = 1, ProjectGroups = new List<GroupProject>()
            };
            var grp = new Group { Id = 1, Name = "Group1", Description = "Desc1" };
            context.Projects.Add(proj);
            context.Groups.Add(grp);
            await context.SaveChangesAsync();

            var currentUser = new User { Id = 1, UserName = "user1" };
            var controller = new EmployeeController(context, CreateUserManager(currentUser), CreateRoleManager())
                { ControllerContext = CreateControllerContext(currentUser) };
            InitializeTempData(controller);

            var result = await controller.RequestGroupToProject(1, 1) as RedirectToActionResult;
            Assert.Equal("ProjectDetails", result.ActionName);
            Assert.Single(context.GroupRequests);
        }

        // ------------------- AcceptRequest -------------------

        [Fact]
        public async Task AcceptRequest_ReturnsNotFound_IfRequestMissing()
        {
            using var context = CreateContext(Guid.NewGuid().ToString());
            var currentUser = new User { Id = 1, UserName = "user1" };
            var controller = new EmployeeController(context, CreateUserManager(currentUser), CreateRoleManager())
                { ControllerContext = CreateControllerContext(currentUser) };
            InitializeTempData(controller);

            var result = await controller.AcceptRequest(999);
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task AcceptRequest_CreatesGroupAssignment_IfNotAlreadyAssigned()
        {
            using var context = CreateContext(Guid.NewGuid().ToString());
            var proj = new Project
            {
                Id = 1,
                Name = "Proj1",
                Description = "D1",
                ProjectLeadId = 1,
                ProjectGroups = new List<GroupProject>()
            };
            var grp = new Group { Id = 1, Name = "Group1", Description = "Desc1" };
            context.Projects.Add(proj);
            context.Groups.Add(grp);
            await context.SaveChangesAsync();

            context.GroupRequests.Add(new GroupRequest
            {
                Id = 1,
                SenderId = 1,
                ProjectId = 1,
                GroupId = 1,
                Response = null
            });
            await context.SaveChangesAsync();

            var currentUser = new User { Id = 1, UserName = "user1" };
            var controller = new EmployeeController(context, CreateUserManager(currentUser), CreateRoleManager())
                { ControllerContext = CreateControllerContext(currentUser) };
            InitializeTempData(controller);

            var result = await controller.AcceptRequest(1) as RedirectToActionResult;
            Assert.Equal("Projects", result.ActionName);
            Assert.Single(context.GroupProjects);
            var request = await context.GroupRequests.FindAsync(1);
            Assert.True(request.Response);
        }

        // ------------------- DenyRequest -------------------

        [Fact]
        public async Task DenyRequest_ReturnsNotFound_IfRequestMissing()
        {
            using var context = CreateContext(Guid.NewGuid().ToString());
            var currentUser = new User { Id = 1, UserName = "user1" };
            var controller = new EmployeeController(context, CreateUserManager(currentUser), CreateRoleManager())
                { ControllerContext = CreateControllerContext(currentUser) };
            InitializeTempData(controller);

            var result = await controller.DenyRequest(999);
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DenyRequest_SetsResponseToFalse_IfValid()
        {
            using var context = CreateContext(Guid.NewGuid().ToString());
            var proj = new Project
            {
                Id = 1,
                Name = "Proj1",
                Description = "D1",
                ProjectLeadId = 1,
                ProjectGroups = new List<GroupProject>()
            };
            var grp = new Group { Id = 1, Name = "Group1", Description = "Desc1" };
            context.Projects.Add(proj);
            context.Groups.Add(grp);
            await context.SaveChangesAsync();

            context.GroupRequests.Add(new GroupRequest
            {
                Id = 1,
                SenderId = 1,
                ProjectId = 1,
                GroupId = 1,
                Response = null
            });
            await context.SaveChangesAsync();

            var currentUser = new User { Id = 1, UserName = "user1" };
            var controller = new EmployeeController(context, CreateUserManager(currentUser), CreateRoleManager())
                { ControllerContext = CreateControllerContext(currentUser) };
            InitializeTempData(controller);

            var result = await controller.DenyRequest(1) as RedirectToActionResult;
            Assert.Equal("Projects", result.ActionName);
            var request = await context.GroupRequests.FindAsync(1);
            Assert.False(request.Response);
        }

        // ------------------- DeleteGroupRequest -------------------

        [Fact]
        public async Task DeleteGroupRequest_DeletesRequest_IfFound()
        {
            using var context = CreateContext(Guid.NewGuid().ToString());
            context.GroupRequests.Add(new GroupRequest
                { Id = 1, SenderId = 1, ProjectId = 1, GroupId = 1, Response = null });
            await context.SaveChangesAsync();

            var currentUser = new User { Id = 1, UserName = "user1" };
            var controller = new EmployeeController(context, CreateUserManager(currentUser), CreateRoleManager())
                { ControllerContext = CreateControllerContext(currentUser) };
            InitializeTempData(controller);

            var result = await controller.DeleteGroupRequest(1) as RedirectToActionResult;
            Assert.Equal("Projects", result.ActionName);
            Assert.Empty(context.GroupRequests);
        }

        [Fact]
        public async Task DeleteGroupRequest_Redirects_IfNotFound()
        {
            using var context = CreateContext(Guid.NewGuid().ToString());
            var currentUser = new User { Id = 1, UserName = "user1" };
            var controller = new EmployeeController(context, CreateUserManager(currentUser), CreateRoleManager())
                { ControllerContext = CreateControllerContext(currentUser) };
            InitializeTempData(controller);

            var result = await controller.DeleteGroupRequest(999) as RedirectToActionResult;
            Assert.Equal("Projects", result.ActionName);
        }

        // ------------------- AssignGroupToProject -------------------

        [Fact]
        public async Task AssignGroupToProject_RedirectsWithError_IfProjectOrGroupMissing()
        {
            using var context = CreateContext(Guid.NewGuid().ToString());
            var currentUser = new User { Id = 1, UserName = "user1" };
            var controller = new EmployeeController(context, CreateUserManager(currentUser), CreateRoleManager())
                { ControllerContext = CreateControllerContext(currentUser) };
            InitializeTempData(controller);

            var result = await controller.AssignGroupToProject(999, 999) as RedirectToActionResult;
            Assert.Equal("ProjectDetails", result.ActionName);
        }

        [Fact]
        public async Task AssignGroupToProject_AddsGroupToProject_IfNotAlreadyAssigned()
        {
            using var context = CreateContext(Guid.NewGuid().ToString());
            var proj = new Project
            {
                Id = 1,
                Name = "Proj1",
                Description = "D1",
                ProjectLeadId = 1,
                ProjectGroups = new List<GroupProject>()
            };
            context.Projects.Add(proj);
            context.Groups.Add(new Group { Id = 1, Name = "Group1", Description = "Desc1" });
            await context.SaveChangesAsync();

            var currentUser = new User { Id = 1, UserName = "user1" };
            var controller = new EmployeeController(context, CreateUserManager(currentUser), CreateRoleManager())
                { ControllerContext = CreateControllerContext(currentUser) };
            InitializeTempData(controller);

            var result = await controller.AssignGroupToProject(1, 1) as RedirectToActionResult;
            Assert.Equal("ProjectDetails", result.ActionName);
            Assert.Single(context.GroupProjects);
        }

        // ------------------- RemoveGroupFromProject -------------------

        [Fact]
        public async Task RemoveGroupFromProject_ReturnsNotFound_IfProjectMissing()
        {
            using var context = CreateContext(Guid.NewGuid().ToString());
            var currentUser = new User { Id = 1, UserName = "user1" };
            var controller = new EmployeeController(context, CreateUserManager(currentUser), CreateRoleManager())
                { ControllerContext = CreateControllerContext(currentUser) };
            InitializeTempData(controller);

            var result = await controller.RemoveGroupFromProject(999, 1);
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task RemoveGroupFromProject_RemovesAssignment_IfExists()
        {
            using var context = CreateContext(Guid.NewGuid().ToString());
            var proj = new Project
            {
                Id = 1,
                Name = "Proj1",
                Description = "D1",
                ProjectLeadId = 1,
                ProjectGroups = new List<GroupProject> { new GroupProject { GroupId = 1 } }
            };
            context.Projects.Add(proj);
            await context.SaveChangesAsync();

            var currentUser = new User { Id = 1, UserName = "user1" };
            var controller = new EmployeeController(context, CreateUserManager(currentUser), CreateRoleManager())
                { ControllerContext = CreateControllerContext(currentUser) };
            InitializeTempData(controller);

            var result = await controller.RemoveGroupFromProject(1, 1) as RedirectToActionResult;
            Assert.Equal("ProjectDetails", result.ActionName);
            Assert.Empty(proj.ProjectGroups);
        }

        // ------------------- Projects -------------------
        [Fact]
        public async Task Projects_ReturnsProjectsView_WithCorrectViewBags()
        {
            using var context = CreateContext(Guid.NewGuid().ToString());

            var user = new User { Id = 1, UserName = "manager" };
            var project1 = new Project { Id = 1, Name = "P1", Description = "D1", ProjectLeadId = 1 };
            var project2 = new Project { Id = 2, Name = "P2", Description = "D2", ProjectLeadId = 1 };

            var group = new Group { Id = 1, Name = "Group1", Description = "Some group", ManagerId = 1 };

            var request1 = new GroupRequest { Id = 1, GroupId = 1, ProjectId = 1, SenderId = 1, Response = null };
            var request2 = new GroupRequest { Id = 2, GroupId = 1, ProjectId = 2, SenderId = 1, Response = true };

            context.Users.Add(user);
            context.Groups.Add(group);
            context.Projects.AddRange(project1, project2);
            context.GroupRequests.AddRange(request1, request2);
            await context.SaveChangesAsync();

            var controller = new EmployeeController(context, CreateUserManager(user), CreateRoleManager())
            {
                ControllerContext = CreateControllerContext(user)
            };
            InitializeTempData(controller);

            var result = await controller.Projects() as ViewResult;

            Assert.NotNull(result);
            Assert.IsType<List<Project>>(result.Model);
            Assert.True((bool)controller.ViewData["IsManager"]);
            Assert.NotNull(controller.ViewData["GroupRequests"]);
            Assert.NotNull(controller.ViewData["SentGroupRequests"]);
        }

        

        


    }
}