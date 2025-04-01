using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;
using TaskManagerWebsite.Controllers;
using TaskManagerWebsite.Data;
using TaskManagerWebsite.Models;
using TaskManagerWebsite.ViewModels.ProjectViewModels;
using Task = System.Threading.Tasks.Task;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace TaskManager.Tests.Tests
{
    public class ProjectControllerTests
    {
        private ApplicationDbContext CreateContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(dbName)
                .Options;
            return new ApplicationDbContext(options);
        }

        private UserManager<User> CreateUserManager(User user, bool isAdmin)
        {
            var store = new Mock<IUserStore<User>>();
            var mockUM = new Mock<UserManager<User>>(store.Object, null, null, null, null, null, null, null, null);
            mockUM.Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(user.Id.ToString());
            mockUM.Setup(um => um.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user);
            mockUM.Setup(um => um.IsInRoleAsync(It.IsAny<User>(), "Admin")).ReturnsAsync(isAdmin);
            mockUM.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user); // <--- Add this line
            return mockUM.Object;
        }


        private ControllerContext CreateControllerContext(User user)
        {
            var identity = new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            }, "TestAuth");
            return new ControllerContext
                { HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) } };
        }

        // GET: ProjectBoard

        [Fact]
        public async Task ProjectBoard_Get_ReturnsNotFound_IfProjectMissing()
        {
            using var context = CreateContext(Guid.NewGuid().ToString());
            var user = new User { Id = 1, UserName = "user" };
            var um = CreateUserManager(user, false);
            var controller = new ProjectController(context, um) { ControllerContext = CreateControllerContext(user) };

            var result = await controller.ProjectBoard(999);
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task ProjectBoard_Get_CreatesBoard_IfMissing()
        {
            using var context = CreateContext(Guid.NewGuid().ToString());
            var proj = new Project
            {
                Id = 1,
                Name = "P1",
                Description = "D1",
                ProjectLeadId = 1,
                ProjectGroups = new List<GroupProject>()
            };
            context.Projects.Add(proj);
            await context.SaveChangesAsync();

            var user = new User { Id = 1, UserName = "user" };
            var um = CreateUserManager(user, false);
            var controller = new ProjectController(context, um) { ControllerContext = CreateControllerContext(user) };

            await controller.ProjectBoard(1);
            var updated = await context.Projects.Include(p => p.ProjectBoard).FirstOrDefaultAsync(p => p.Id == 1);
            Assert.NotNull(updated.ProjectBoard);
        }

        [Fact]
        public async Task ProjectBoard_Get_ReturnsAdminView_IfAdmin()
        {
            using var context = CreateContext(Guid.NewGuid().ToString());
            var proj = new Project
            {
                Id = 2,
                Name = "P2",
                Description = "D2",
                ProjectLeadId = 2,
                ProjectGroups = new List<GroupProject>()
            };
            proj.ProjectBoard = new ProjectBoard
                { Id = 10, ProjectId = 2, BoardCreatorId = 2, Stages = new List<Stage>() };
            context.Projects.Add(proj);
            await context.SaveChangesAsync();

            var user = new User { Id = 2, UserName = "admin" };
            var um = CreateUserManager(user, true);
            var controller = new ProjectController(context, um) { ControllerContext = CreateControllerContext(user) };

            var result = await controller.ProjectBoard(2) as ViewResult;
            Assert.Equal("~/Views/Admin/ProjectBoard.cshtml", result?.ViewName);
        }

        // POST: ProjectBoard

        [Fact]
        public async Task ProjectBoard_Post_ReturnsNotFound_IfProjectMissing()
        {
            using var context = CreateContext(Guid.NewGuid().ToString());
            var user = new User { Id = 1, UserName = "user" };
            var um = CreateUserManager(user, false);
            var controller = new ProjectController(context, um) { ControllerContext = CreateControllerContext(user) };

            var vm = new ProjectBoardViewModel();
            var result = await controller.ProjectBoard(999, vm);
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task ProjectBoard_Post_ReturnsForbid_IfUnauthorized()
        {
            using var context = CreateContext(Guid.NewGuid().ToString());
            var proj = new Project
            {
                Id = 4,
                Name = "P4",
                Description = "D4",
                ProjectLeadId = 2,
                ProjectGroups = new List<GroupProject>()
            };
            proj.ProjectBoard = new ProjectBoard
                { Id = 20, ProjectId = 4, BoardCreatorId = 2, Stages = new List<Stage>() };
            context.Projects.Add(proj);
            await context.SaveChangesAsync();

            var user = new User { Id = 1, UserName = "user" };
            var um = CreateUserManager(user, false);
            var controller = new ProjectController(context, um) { ControllerContext = CreateControllerContext(user) };

            var vm = new ProjectBoardViewModel
                { StageForm = new CreateStageViewModel { Name = "S1", Position = 1, SelectedGroupId = 0 } };
            var result = await controller.ProjectBoard(4, vm);
            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async Task ProjectBoard_Post_ReturnsView_IfModelStateInvalid()
        {
            using var context = CreateContext(Guid.NewGuid().ToString());
            var proj = new Project
            {
                Id = 5,
                Name = "P5",
                Description = "D5",
                ProjectLeadId = 1,
                ProjectGroups = new List<GroupProject>()
            };
            proj.ProjectBoard = new ProjectBoard
                { Id = 30, ProjectId = 5, BoardCreatorId = 1, Stages = new List<Stage>() };
            context.Projects.Add(proj);
            await context.SaveChangesAsync();

            var user = new User { Id = 1, UserName = "lead" };
            var um = CreateUserManager(user, false);
            var controller = new ProjectController(context, um) { ControllerContext = CreateControllerContext(user) };
            controller.ModelState.AddModelError("Error", "Invalid");

            var vm = new ProjectBoardViewModel
                { StageForm = new CreateStageViewModel { Name = "S1", Position = 1, SelectedGroupId = 0 } };
            var result = await controller.ProjectBoard(5, vm) as ViewResult;
            var model = result.Model as ProjectBoardViewModel;
            Assert.NotNull(model);
            Assert.Equal(5, model.Project.Id);
        }

        [Fact]
        public async Task ProjectBoard_Post_ReturnsView_IfDuplicateStageExists()
        {
            using var context = CreateContext(Guid.NewGuid().ToString());
            var grp = new Group { Id = 1, Name = "G1", Description = "Test" };
            var proj = new Project
            {
                Id = 6,
                Name = "P6",
                Description = "D6",
                ProjectLeadId = 1,
                ProjectGroups = new List<GroupProject> { new GroupProject { Group = grp, GroupId = grp.Id } }
            };
            proj.ProjectBoard = new ProjectBoard
                { Id = 40, ProjectId = 6, BoardCreatorId = 1, Stages = new List<Stage>() };
            context.Projects.Add(proj);
            context.Stages.Add(new Stage
            {
                Id = 100,
                Name = "Duplicate",
                Position = 1,
                ProjectBoardId = 40,
                AssignedGroup = grp
            });
            await context.SaveChangesAsync();

            var user = new User { Id = 1, UserName = "lead" };
            var um = CreateUserManager(user, false);
            var controller = new ProjectController(context, um) { ControllerContext = CreateControllerContext(user) };

            var vm = new ProjectBoardViewModel
                { StageForm = new CreateStageViewModel { Name = "Duplicate", Position = 2, SelectedGroupId = grp.Id } };
            var result = await controller.ProjectBoard(6, vm) as ViewResult;
            Assert.NotNull(result);
            Assert.True(controller.ModelState.ErrorCount > 0);
        }

        [Fact]
        public async Task ProjectBoard_Post_AddsStage_IfValid()
        {
            using var context = CreateContext(Guid.NewGuid().ToString());
            var grp = new Group { Id = 2, Name = "G2", Description = "Test" };
            var proj = new Project
            {
                Id = 7,
                Name = "P7",
                Description = "D7",
                ProjectLeadId = 1,
                ProjectGroups = new List<GroupProject> { new GroupProject { Group = grp, GroupId = grp.Id } }
            };
            proj.ProjectBoard = new ProjectBoard
                { Id = 50, ProjectId = 7, BoardCreatorId = 1, Stages = new List<Stage>() };
            context.Projects.Add(proj);
            await context.SaveChangesAsync();
            context.UserGroups.Add(new UserGroup { UserId = 1, GroupId = grp.Id, Role = "Manager" });
            await context.SaveChangesAsync();

            var user = new User { Id = 1, UserName = "user" };
            var um = CreateUserManager(user, false);
            var controller = new ProjectController(context, um) { ControllerContext = CreateControllerContext(user) };

            var vm = new ProjectBoardViewModel
                { StageForm = new CreateStageViewModel { Name = "NewStage", Position = 1, SelectedGroupId = grp.Id } };
            var result = await controller.ProjectBoard(7, vm) as RedirectToActionResult;
            Assert.Equal(nameof(ProjectController.ProjectBoard), result.ActionName);
            var stage = await context.Stages.FirstOrDefaultAsync(s => s.Name == "NewStage");
            Assert.NotNull(stage);
        }

        // GET: EditStage

        [Fact]
        public async Task EditStage_Get_ReturnsNotFound_IfStageMissing()
        {
            using var context = CreateContext(Guid.NewGuid().ToString());
            var user = new User { Id = 1, UserName = "user" };
            var um = CreateUserManager(user, false);
            var controller = new ProjectController(context, um) { ControllerContext = CreateControllerContext(user) };
            var result = await controller.EditStage(999);
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task EditStage_Get_ReturnsNotFound_IfProjectMissing()
        {
            using var context = CreateContext(Guid.NewGuid().ToString());
            var board = new ProjectBoard { Id = 60, ProjectId = 100, Stages = new List<Stage>() };
            var stage = new Stage
            {
                Id = 200, ProjectBoard = board, Position = 1, Name = "S",
                AssignedGroup = new Group { Id = 1, Name = "Dummy", Description = "Dummy" }
            };
            context.Stages.Add(stage);
            await context.SaveChangesAsync();

            var user = new User { Id = 1, UserName = "user" };
            var um = CreateUserManager(user, false);
            var controller = new ProjectController(context, um) { ControllerContext = CreateControllerContext(user) };
            var result = await controller.EditStage(200);
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task EditStage_Get_ReturnsView_IfValid()
        {
            using var context = CreateContext(Guid.NewGuid().ToString());
            var grp = new Group { Id = 3, Name = "G3", Description = "Test" };
            var proj = new Project
            {
                Id = 8,
                Name = "P8",
                Description = "D8",
                ProjectLeadId = 1,
                ProjectGroups = new List<GroupProject> { new GroupProject { Group = grp, GroupId = grp.Id } }
            };
            var board = new ProjectBoard { Id = 70, ProjectId = 8, BoardCreatorId = 1, Stages = new List<Stage>() };
            proj.ProjectBoard = board;
            var stage = new Stage
            {
                Id = 300, ProjectBoard = board, Position = 1, Name = "EditStage", AssignedGroup = grp,
                AssignedGroupId = grp.Id
            };
            board.Stages = new List<Stage> { stage };
            context.Projects.Add(proj);
            await context.SaveChangesAsync();

            var user = new User { Id = 1, UserName = "user" };
            var um = CreateUserManager(user, false);
            var controller = new ProjectController(context, um) { ControllerContext = CreateControllerContext(user) };

            var result = await controller.EditStage(300) as ViewResult;
            var vm = result.Model as StageEditViewModel;
            Assert.NotNull(vm);
            Assert.Equal(300, vm.StageId);
        }

        // POST: EditStage

        [Fact]
        public async Task EditStage_Post_ReturnsNotFound_IfStageMissing()
        {
            using var context = CreateContext(Guid.NewGuid().ToString());
            var user = new User { Id = 1, UserName = "user" };
            var um = CreateUserManager(user, false);
            var controller = new ProjectController(context, um) { ControllerContext = CreateControllerContext(user) };
            var vm = new StageEditViewModel { StageId = 999 };
            var result = await controller.EditStage(vm);
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task EditStage_Post_ReturnsNotFound_IfProjectMissing()
        {
            using var context = CreateContext(Guid.NewGuid().ToString());
            var board = new ProjectBoard { Id = 80, ProjectId = 500, Stages = new List<Stage>() };
            var stage = new Stage
            {
                Id = 400, ProjectBoard = board, Position = 1, Name = "S",
                AssignedGroup = new Group { Id = 1, Name = "Dummy", Description = "Dummy" }
            };
            context.Stages.Add(stage);
            await context.SaveChangesAsync();

            var user = new User { Id = 1, UserName = "user" };
            var um = CreateUserManager(user, false);
            var controller = new ProjectController(context, um) { ControllerContext = CreateControllerContext(user) };
            var vm = new StageEditViewModel { StageId = 400, Name = "Updated" };
            var result = await controller.EditStage(vm);
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task EditStage_Post_ReturnsForbid_IfUnauthorized()
        {
            using var context = CreateContext(Guid.NewGuid().ToString());
            var proj = new Project
            {
                Id = 9, Name = "P9", Description = "D9", ProjectLeadId = 2, ProjectGroups = new List<GroupProject>()
            };
            var board = new ProjectBoard { Id = 90, ProjectId = 9, BoardCreatorId = 2, Stages = new List<Stage>() };
            proj.ProjectBoard = board;
            context.Projects.Add(proj);
            var stage = new Stage
            {
                Id = 500, ProjectBoard = board, Position = 1, Name = "S",
                AssignedGroup = new Group { Id = 1, Name = "Dummy", Description = "Dummy" }
            };
            context.Stages.Add(stage);
            await context.SaveChangesAsync();

            var user = new User { Id = 1, UserName = "user" };
            var um = CreateUserManager(user, false);
            var controller = new ProjectController(context, um) { ControllerContext = CreateControllerContext(user) };
            var vm = new StageEditViewModel { StageId = 500, Name = "Updated", Position = 2, SelectedGroupId = 0 };
            var result = await controller.EditStage(vm);
            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async Task EditStage_Post_ReturnsView_IfModelStateInvalid()
        {
            using var context = CreateContext(Guid.NewGuid().ToString());
            var grp = new Group { Id = 10, Name = "G10", Description = "Test" };
            var proj = new Project
            {
                Id = 10, Name = "P10", Description = "D10", ProjectLeadId = 1,
                ProjectGroups = new List<GroupProject> { new GroupProject { Group = grp, GroupId = grp.Id } }
            };
            var board = new ProjectBoard { Id = 100, ProjectId = 10, BoardCreatorId = 1, Stages = new List<Stage>() };
            proj.ProjectBoard = board;
            var stage = new Stage { Id = 600, ProjectBoard = board, Position = 1, Name = "S", AssignedGroup = grp };
            board.Stages = new List<Stage> { stage };
            context.Projects.Add(proj);
            await context.SaveChangesAsync();

            var user = new User { Id = 1, UserName = "user" };
            var um = CreateUserManager(user, false);
            var controller = new ProjectController(context, um) { ControllerContext = CreateControllerContext(user) };
            controller.ModelState.AddModelError("Error", "Invalid");
            var vm = new StageEditViewModel { StageId = 600, Name = "Updated", Position = 2, SelectedGroupId = grp.Id };
            var result = await controller.EditStage(vm) as ViewResult;
            Assert.NotNull(result);
        }

        [Fact]
        public async Task EditStage_Post_ReturnsView_IfDuplicateStageNameExists()
        {
            using var context = CreateContext(Guid.NewGuid().ToString());
            var grp = new Group { Id = 11, Name = "G11", Description = "Test" };
            var proj = new Project
            {
                Id = 11, Name = "P11", Description = "D11", ProjectLeadId = 1,
                ProjectGroups = new List<GroupProject> { new GroupProject { Group = grp, GroupId = grp.Id } }
            };
            var board = new ProjectBoard { Id = 110, ProjectId = 11, BoardCreatorId = 1, Stages = new List<Stage>() };
            proj.ProjectBoard = board;

            context.Stages.Add(new Stage
                { Id = 700, Name = "Duplicate", Position = 1, ProjectBoardId = 110, AssignedGroup = grp });
            var stage = new Stage
                { Id = 800, ProjectBoard = board, Position = 2, Name = "Original", AssignedGroup = grp };
            board.Stages = new List<Stage> { stage };
            context.Stages.Add(stage);
            context.Projects.Add(proj);
            await context.SaveChangesAsync();

            var user = new User { Id = 1, UserName = "user" };
            var um = CreateUserManager(user, false);
            var controller = new ProjectController(context, um) { ControllerContext = CreateControllerContext(user) };
            var vm = new StageEditViewModel
                { StageId = 800, Name = "Duplicate", Position = 3, SelectedGroupId = grp.Id };
            var result = await controller.EditStage(vm) as ViewResult;
            Assert.NotNull(result);
            Assert.True(controller.ModelState.ErrorCount > 0);
        }

        [Fact]
        public async Task EditStage_Post_UpdatesStage_IfValid()
        {
            using var context = CreateContext(Guid.NewGuid().ToString());
            var grp = new Group { Id = 12, Name = "G12", Description = "Test" };
            var proj = new Project
            {
                Id = 12, Name = "P12", Description = "D12", ProjectLeadId = 1,
                ProjectGroups = new List<GroupProject> { new GroupProject { Group = grp, GroupId = grp.Id } }
            };
            var board = new ProjectBoard { Id = 120, ProjectId = 12, BoardCreatorId = 1, Stages = new List<Stage>() };
            proj.ProjectBoard = board;
            var s1 = new Stage { Id = 900, ProjectBoard = board, Position = 1, Name = "S1", AssignedGroup = grp };
            var s2 = new Stage { Id = 901, ProjectBoard = board, Position = 2, Name = "S2", AssignedGroup = grp };
            board.Stages = new List<Stage> { s1, s2 };
            context.Projects.Add(proj);
            await context.SaveChangesAsync();

            var user = new User { Id = 1, UserName = "user" };
            var um = CreateUserManager(user, false);
            var controller = new ProjectController(context, um) { ControllerContext = CreateControllerContext(user) };
            var vm = new StageEditViewModel
                { StageId = 901, Name = "S2 Updated", Position = 1, SelectedGroupId = grp.Id };
            var result = await controller.EditStage(vm) as RedirectToActionResult;
            Assert.Equal(nameof(ProjectController.ProjectBoard), result.ActionName);
            var updated = await context.Stages.FindAsync(901);
            Assert.Equal("S2 Updated", updated.Name);
            Assert.Equal(1, updated.Position);
            var swapped = await context.Stages.FindAsync(900);
            Assert.Equal(2, swapped.Position);
        }


        [Fact]
        public async Task EditTask_Get_TaskNotFound_ReturnsNotFound()
        {
            using var context = CreateContext(Guid.NewGuid().ToString());
            var user = new User { Id = 1, UserName = "user" };

            var controller = new ProjectController(context, this.CreateUserManager(user, false));

            var result = await controller.EditTask(taskId: 999);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task EditTask_Get_ProjectNotFound_ReturnsNotFound()
        {
            using var context = CreateContext(Guid.NewGuid().ToString());
            context.Tasks.Add(new TaskManagerWebsite.Models.Task { Id = 1, Name = "Test", Description = "Test Task" });
            await context.SaveChangesAsync();
            var user = new User { Id = 1, UserName = "user" };

            var controller = new ProjectController(context, this.CreateUserManager(user, false));

            var result = await controller.EditTask(1);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task EditTask_Get_UserNotAuthorized_ReturnsForbid()
        {
            using var context = CreateContext(Guid.NewGuid().ToString());
            var task = new TaskManagerWebsite.Models.Task { Id = 1, Name = "Task", Description = "Test Description" };
            var project = new Project { Id = 1, ProjectLeadId = 999, Name = "Task", Description = "Test Description" };
            var board = new ProjectBoard { Id = 1, Project = project };
            var stage = new Stage { Id = 1, ProjectBoard = board, Name = "Test Stage" };
            var taskStage = new TaskStage { Id = 1, TaskId = 1, Stage = stage, Task = task };

            context.Tasks.Add(task);
            context.Projects.Add(project);
            context.ProjectBoards.Add(board);
            context.Stages.Add(stage);
            context.TaskStages.Add(taskStage);
            await context.SaveChangesAsync();

            var userManager = this.CreateUserManager(new User { Id = 1, UserName = "test" }, false);
            var controller = new ProjectController(context, userManager);

            var result = await controller.EditTask(1);

            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async Task EditTask_Get_Valid_ReturnsViewWithModel()
        {
            using var context = CreateContext(Guid.NewGuid().ToString());

            var user = new User { Id = 1, UserName = "user" };
            var project = new Project
                { Id = 1, ProjectLeadId = 1, Name = "Test Project", Description = "Test Description" };
            var board = new ProjectBoard { Id = 1, Project = project };
            var stage = new Stage { Id = 1, ProjectBoard = board, Name = "Test Stage" };
            var task = new TaskManagerWebsite.Models.Task
                { Id = 2, Name = "My Task", Description = "Test Description", CreatorUserId = user.Id };
            var taskStage = new TaskStage { Id = 1, TaskId = task.Id, Task = task, Stage = stage };
            var assignment = new TaskEmployee { TaskId = 2, EmployeeId = user.Id, Employee = user };

            context.Users.Add(user);
            context.Projects.Add(project);
            context.ProjectBoards.Add(board);
            context.Stages.Add(stage);
            context.Tasks.Add(task);
            context.TaskStages.Add(taskStage);
            context.TaskEmployees.Add(assignment);
            await context.SaveChangesAsync();

            var userManager = this.CreateUserManager(user, isAdmin: true);
            var controller = new ProjectController(context, userManager);

            var result = await controller.EditTask(task.Id);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<CreateTaskViewModel>(viewResult.Model);
            Assert.Equal("My Task", model.Name);
        }


        [Fact]
        public async Task EditTask_Post_InvalidModel_ReturnsView()
        {
            using var context = CreateContext(nameof(EditTask_Post_InvalidModel_ReturnsView));
            var user = new User { Id = 1, UserName = "user" };

            var controller = new ProjectController(context, CreateUserManager(user, false));
            controller.ModelState.AddModelError("Name", "Required");

            var vm = new CreateTaskViewModel { TaskId = 1 };

            var result = await controller.EditTask(vm);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<CreateTaskViewModel>(viewResult.Model);
        }

        [Fact]
        public async Task EditTask_Post_DuplicateName_ReturnsModelError()
        {
            using var context = CreateContext(nameof(EditTask_Post_DuplicateName_ReturnsModelError));

            var user = new User { Id = 1 };
            var project = new Project
                { Id = 1, ProjectLeadId = user.Id, Name = "Test Project", Description = "Test Description" };
            var board = new ProjectBoard { Id = 1, Project = project };
            var stage = new Stage { Id = 1, ProjectBoard = board, Name = "Test Stage" };
            var task1 = new TaskManagerWebsite.Models.Task
                { Id = 1, Name = "Original Task", CreatorUserId = user.Id, Description = "Task" };
            var task2 = new TaskManagerWebsite.Models.Task
                { Id = 2, Name = "Duplicate Name", CreatorUserId = user.Id, Description = "Task" };

            context.Users.Add(user);
            context.Projects.Add(project);
            context.ProjectBoards.Add(board);
            context.Stages.Add(stage);
            context.Tasks.AddRange(task1, task2);
            context.TaskStages.AddRange(
                new TaskStage { Task = task1, Stage = stage },
                new TaskStage { Task = task2, Stage = stage }
            );
            await context.SaveChangesAsync();

            var controller = new ProjectController(context, CreateUserManager(user, false));

            var vm = new CreateTaskViewModel
            {
                TaskId = 1,
                Name = "Duplicate Name" // Duplicate
            };

            var result = await controller.EditTask(vm);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(controller.ModelState.IsValid);
            Assert.True(controller.ModelState.ContainsKey("Name"));
        }

        [Fact]
        public async Task EditTask_Post_TaskNotFound_ReturnsNotFound()
        {
            using var context = CreateContext(nameof(EditTask_Post_TaskNotFound_ReturnsNotFound));
            var user = new User { Id = 1 };

            var controller = new ProjectController(context, CreateUserManager(user, false));

            var vm = new CreateTaskViewModel
            {
                TaskId = 999, // Non-existent
                Name = "New Name"
            };

            var result = await controller.EditTask(vm);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task EditTask_Post_ValidUpdate_UpdatesTaskAndRedirects()
        {
            using var context = CreateContext(nameof(EditTask_Post_ValidUpdate_UpdatesTaskAndRedirects));

            var user = new User { Id = 1, UserName = "admin" };
            var project = new Project
                { Id = 1, ProjectLeadId = user.Id, Name = "Test Project", Description = "Test Description" };
            var board = new ProjectBoard { Id = 1, Project = project };
            var stage = new Stage { Id = 1, ProjectBoard = board, Name = "Test" };
            var task = new TaskManagerWebsite.Models.Task
                { Id = 1, Name = "Old Task", Description = "Old Desc", CreatorUserId = user.Id };
            var taskStage = new TaskStage { Task = task, Stage = stage };

            context.Users.Add(user);
            context.Projects.Add(project);
            context.ProjectBoards.Add(board);
            context.Stages.Add(stage);
            context.Tasks.Add(task);
            context.TaskStages.Add(taskStage);
            await context.SaveChangesAsync();

            var userManager = CreateUserManager(user, false);
            var controller = new ProjectController(context, userManager);

            var vm = new CreateTaskViewModel
            {
                TaskId = 1,
                Name = "Updated Task",
                Description = "Updated Desc"
            };

            var result = await controller.EditTask(vm);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("ProjectBoard", redirect.ActionName);

            var updatedTask = await context.Tasks.FindAsync(1);
            Assert.Equal("Updated Task", updatedTask.Name);
            Assert.Equal("Updated Desc", updatedTask.Description);
        }


        // DELETE: DeleteStage

        [Fact]
        public async Task DeleteStage_Post_ReturnsNotFound_IfStageMissing()
        {
            using var context = CreateContext(Guid.NewGuid().ToString());
            var user = new User { Id = 1, UserName = "user" };
            var um = CreateUserManager(user, false);
            var controller = new ProjectController(context, um) { ControllerContext = CreateControllerContext(user) };
            var result = await controller.DeleteStage(999);
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteStage_Post_ReturnsNotFound_IfProjectMissing()
        {
            using var context = CreateContext(Guid.NewGuid().ToString());
            var board = new ProjectBoard { Id = 130, ProjectId = 600, Stages = new List<Stage>() };
            var stage = new Stage
            {
                Id = 1000, ProjectBoard = board, Position = 1, Name = "S",
                AssignedGroup = new Group { Id = 1, Name = "Dummy", Description = "Dummy" }
            };
            context.Stages.Add(stage);
            await context.SaveChangesAsync();

            var user = new User { Id = 1, UserName = "user" };
            var um = CreateUserManager(user, false);
            var controller = new ProjectController(context, um) { ControllerContext = CreateControllerContext(user) };
            var result = await controller.DeleteStage(1000);
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteStage_Post_ReturnsForbid_IfUnauthorized()
        {
            using var context = CreateContext(Guid.NewGuid().ToString());
            var proj = new Project
            {
                Id = 13, Name = "P13", Description = "D13", ProjectLeadId = 2, ProjectGroups = new List<GroupProject>()
            };
            var board = new ProjectBoard { Id = 140, ProjectId = 13, BoardCreatorId = 2, Stages = new List<Stage>() };
            proj.ProjectBoard = board;
            context.Projects.Add(proj);
            var stage = new Stage
            {
                Id = 1100, ProjectBoard = board, Position = 1, Name = "S",
                AssignedGroup = new Group { Id = 1, Name = "Dummy", Description = "Dummy" }
            };
            context.Stages.Add(stage);
            await context.SaveChangesAsync();

            var user = new User { Id = 1, UserName = "user" };
            var um = CreateUserManager(user, false);
            var controller = new ProjectController(context, um) { ControllerContext = CreateControllerContext(user) };
            var result = await controller.DeleteStage(1100);
            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async Task DeleteStage_Post_DeletesStage_IfAuthorized()
        {
            using var context = CreateContext(Guid.NewGuid().ToString());
            var grp = new Group { Id = 1, Name = "G1", Description = "Test" };
            var proj = new Project
            {
                Id = 14,
                Name = "P14",
                Description = "D14",
                ProjectLeadId = 1,
                ProjectGroups = new List<GroupProject> { new GroupProject { Group = grp, GroupId = grp.Id } }
            };
            var board = new ProjectBoard { Id = 150, ProjectId = 14, BoardCreatorId = 1, Stages = new List<Stage>() };
            proj.ProjectBoard = board;
            var stage = new Stage { Id = 1200, ProjectBoard = board, Position = 1, Name = "SDel", AssignedGroup = grp };
            context.Projects.Add(proj);
            context.Stages.Add(stage);
            await context.SaveChangesAsync();

            context.UserGroups.Add(new UserGroup { UserId = 1, GroupId = grp.Id, Role = "Manager" });
            await context.SaveChangesAsync();

            var user = new User { Id = 1, UserName = "user" };
            var um = CreateUserManager(user, false);
            var controller = new ProjectController(context, um) { ControllerContext = CreateControllerContext(user) };

            var result = await controller.DeleteStage(1200) as RedirectToActionResult;
            Assert.Equal(nameof(ProjectController.ProjectBoard), result.ActionName);
            Assert.Null(await context.Stages.FindAsync(1200));
        }

        [Fact]
        public async Task DeleteTask_ValidId_DeletesTaskAndRedirects()
        {
            // Arrange
            using var context = CreateContext(Guid.NewGuid().ToString());

            var user = new User { Id = 1, UserName = "test" };
            var project = new Project { Id = 1, Name = "Test", Description = "Test Description" };
            var board = new ProjectBoard { Id = 1, ProjectId = 1, Project = project };
            var stage = new Stage { Name = "Stage1", Id = 1, ProjectBoardId = 1, ProjectBoard = board };
            var task = new TaskManagerWebsite.Models.Task
                { Description = "TestDescription", Id = 1, Name = "Task1", CreatorUserId = user.Id };
            var taskStage = new TaskStage { Id = 1, TaskId = 1, Task = task, StageId = 1, Stage = stage };
            var taskEmployee = new TaskEmployee { TaskId = 1, EmployeeId = 2 };

            context.Projects.Add(project);
            context.ProjectBoards.Add(board);
            context.Stages.Add(stage);
            context.Tasks.Add(task);
            context.TaskStages.Add(taskStage);
            context.TaskEmployees.Add(taskEmployee);
            await context.SaveChangesAsync();

            var controller = new ProjectController(context, this.CreateUserManager(user, true));

            // Act
            var result = await controller.DeleteTask(taskStage.Id);

            // Assert
            Assert.IsType<RedirectToActionResult>(result);
            Assert.False(context.Tasks.Any());
            Assert.False(context.TaskStages.Any());
            Assert.False(context.TaskEmployees.Any());
        }

        [Fact]
        public async Task DeleteTask_StageNotFound_ReturnsNotFound()
        {
            using var context = CreateContext(Guid.NewGuid().ToString());

            var user = new User { Id = 1, UserName = "test" };
            var task = new TaskManagerWebsite.Models.Task
                { Description = "TestDescription", Id = 1, Name = "Task1", CreatorUserId = 1 };
            var taskStage = new TaskStage { Id = 1, TaskId = 1, Task = task, StageId = 42 }; // stageId 42 doesn't exist

            context.Tasks.Add(task);
            context.TaskStages.Add(taskStage);
            await context.SaveChangesAsync();

            var controller = new ProjectController(context, this.CreateUserManager(user, false));

            var result = await controller.DeleteTask(taskStage.Id);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task CreateTask_Post_InvalidModelState_ReturnsView()
        {
            var controller = new ProjectController(CreateContext(Guid.NewGuid().ToString()),
                CreateUserManager(new User { Id = 1 }, false));
            controller.ModelState.AddModelError("Name", "Required");

            var vm = new CreateTaskViewModel { StageId = 1 };
            var result = await controller.CreateTask(vm);
            var view = Assert.IsType<ViewResult>(result);
            Assert.Equal(vm, view.Model);
        }

        [Fact]
        public async Task CreateTask_Post_ReturnsNotFound_IfStageMissing()
        {
            using var context = CreateContext(Guid.NewGuid().ToString());
            var controller = new ProjectController(context, CreateUserManager(new User { Id = 1 }, false));
            var result = await controller.CreateTask(new CreateTaskViewModel { StageId = 999 });
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task CreateTask_Post_ReturnsViewIfDuplicateNameExists()
        {
            using var context = CreateContext(Guid.NewGuid().ToString());

            var user = new User { Id = 1 };
            var project = new Project { Id = 1, Name = "Test Project", Description = "Some description", ProjectLeadId = user.Id };
            var board = new ProjectBoard { Id = 1, Project = project };
            var stage = new Stage { Id = 1, Name = "Stage", ProjectBoard = board };
            var existingTask = new TaskManagerWebsite.Models.Task
                { Id = 1, Name = "Task1", Description = "desc", CreatorUserId = user.Id };
            var taskStage = new TaskStage { Task = existingTask, Stage = stage };

            context.Users.Add(user);
            context.Projects.Add(project);
            context.ProjectBoards.Add(board);
            context.Stages.Add(stage);
            context.Tasks.Add(existingTask);
            context.TaskStages.Add(taskStage);
            await context.SaveChangesAsync();

            var controller = new ProjectController(context, CreateUserManager(user, false))
            {
                ControllerContext = CreateControllerContext(user)
            };

            var vm = new CreateTaskViewModel { Name = "Task1", Description = "Duplicate", StageId = stage.Id };
            var result = await controller.CreateTask(vm);
            var view = Assert.IsType<ViewResult>(result);
            Assert.True(controller.ModelState.ContainsKey("Name"));
        }

        [Fact]
        public async Task CreateTask_Post_CreatesTaskAndRedirects_WhenValid()
        {
            using var context = CreateContext(Guid.NewGuid().ToString());
            var user = new User { Id = 1 };
            var project = new Project { Id = 1, Name = "Test Project", Description = "Some description", ProjectLeadId = user.Id };
            var board = new ProjectBoard { Id = 1, Project = project };
            var stage = new Stage { Id = 1, Name = "Stage", ProjectBoard = board };

            context.Users.Add(user);
            context.Projects.Add(project);
            context.ProjectBoards.Add(board);
            context.Stages.Add(stage);
            await context.SaveChangesAsync();

            var controller = new ProjectController(context, CreateUserManager(user, false))
            {
                ControllerContext = CreateControllerContext(user)
            };

            var vm = new CreateTaskViewModel
            {
                Name = "New Task",
                Description = "desc",
                StageId = stage.Id
            };

            var result = await controller.CreateTask(vm);
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("ProjectBoard", redirect.ActionName);
            Assert.True(context.Tasks.Any(t => t.Name == "New Task"));
        }

        [Fact]
        public async Task CreateTask_Post_CreatesTaskWithEmployeeAssignment()
        {
            using var context = CreateContext(Guid.NewGuid().ToString());
            var user = new User { Id = 1 };
            var assigned = new User { Id = 2 };
            var project = new Project {Id = 1, Name = "Test Project", Description = "Some description", ProjectLeadId = user.Id };
            var board = new ProjectBoard { Id = 1, Project = project };
            var stage = new Stage { Id = 1, Name = "Stage", ProjectBoard = board };

            context.Users.AddRange(user, assigned);
            context.Projects.Add(project);
            context.ProjectBoards.Add(board);
            context.Stages.Add(stage);
            await context.SaveChangesAsync();

            var userManager = CreateUserManager(user, false);
            Mock.Get(userManager).Setup(um => um.FindByIdAsync("2")).ReturnsAsync(assigned);

            var controller = new ProjectController(context, userManager)
            {
                ControllerContext = CreateControllerContext(user)
            };

            var vm = new CreateTaskViewModel
            {
                Name = "With Assignment",
                Description = "desc",
                StageId = stage.Id,
                SelectedEmployeeId = 2
            };

            var result = await controller.CreateTask(vm);
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("ProjectBoard", redirect.ActionName);

            var created = await context.Tasks.FirstOrDefaultAsync(t => t.Name == "With Assignment");
            var assignment = await context.TaskEmployees.FirstOrDefaultAsync(te => te.TaskId == created.Id);
            Assert.NotNull(assignment);
            Assert.Equal(2, assignment.EmployeeId);
        }

        [Fact]
        public async Task CreateTask_Get_ReturnsNotFound_IfStageMissing()
        {
            using var context = CreateContext(Guid.NewGuid().ToString());
            var controller = new ProjectController(context, CreateUserManager(new User { Id = 1 }, false));
            var result = await controller.CreateTask(999);
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task CreateTask_Get_ReturnsNotFound_IfProjectMissing()
        {
            using var context = CreateContext(Guid.NewGuid().ToString());

            var board = new ProjectBoard { Id = 1, ProjectId = 42 };
            var stage = new Stage { Id = 1, Name = "Stage 1", ProjectBoard = board };

            context.Stages.Add(stage);
            context.ProjectBoards.Add(board);
            await context.SaveChangesAsync();

            var controller = new ProjectController(context, CreateUserManager(new User { Id = 1 }, false));
            var result = await controller.CreateTask(stage.Id);
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task CreateTask_Get_ReturnsView_ForAdmin()
        {
            using var context = CreateContext(Guid.NewGuid().ToString());
            var user = new User { Id = 1, UserName = "admin" };

            var group = new Group { Id = 1, Name = "G", Description = "Desc"};
            var project = new Project
            {
                Id = 1,
                Name = "Project A",
                Description = "Test",
                ProjectLeadId = user.Id,
                ProjectGroups = new List<GroupProject> { new GroupProject { Group = group, GroupId = group.Id } }
            };

            var board = new ProjectBoard { Id = 1, Project = project };
            var stage = new Stage { Id = 1, Name = "Stage", ProjectBoard = board };
            project.ProjectBoard = board;

            context.Users.Add(user);
            context.Projects.Add(project);
            context.ProjectBoards.Add(board);
            context.Stages.Add(stage);
            context.Groups.Add(group);
            await context.SaveChangesAsync();

            var controller = new ProjectController(context, CreateUserManager(user, true))
            {
                ControllerContext = CreateControllerContext(user)
            };

            var result = await controller.CreateTask(stage.Id);
            var view = Assert.IsType<ViewResult>(result);
            Assert.IsType<CreateTaskViewModel>(view.Model);
        }

        [Fact]
        public async Task CreateTask_Get_ReturnsView_ForProjectLead()
        {
            using var context = CreateContext(Guid.NewGuid().ToString());
            var user = new User { Id = 1, UserName = "lead" };

            var group = new Group { Id = 1, Name = "G", Description = "Desc" };
            var project = new Project
            {
                Id = 1,
                Name = "Project B",
                Description = "Test B",
                ProjectLeadId = user.Id,
                ProjectGroups = new List<GroupProject> { new GroupProject { Group = group, GroupId = group.Id } }
            };

            var board = new ProjectBoard { Id = 1, Project = project };
            var stage = new Stage { Id = 1, Name = "Stage", ProjectBoard = board };
            project.ProjectBoard = board;

            context.Users.Add(user);
            context.Projects.Add(project);
            context.ProjectBoards.Add(board);
            context.Stages.Add(stage);
            context.Groups.Add(group);
            await context.SaveChangesAsync();

            var controller = new ProjectController(context, CreateUserManager(user, false))
            {
                ControllerContext = CreateControllerContext(user)
            };

            var result = await controller.CreateTask(stage.Id);
            var view = Assert.IsType<ViewResult>(result);
            Assert.IsType<CreateTaskViewModel>(view.Model);
        }

        [Fact]
        public async Task CreateTask_Get_ReturnsView_ForGroupManager()
        {
            using var context = CreateContext(Guid.NewGuid().ToString());
            var user = new User { Id = 1 };
            var group = new Group { Id = 1, Name = "Group", Description = "Desc" };

            context.UserGroups.Add(new UserGroup { UserId = 1, GroupId = 1, Role = "Manager" });

            var project = new Project
            {
                Id = 1,
                Name = "Project C",
                Description = "Test C",
                ProjectLeadId = 999,
                ProjectGroups = new List<GroupProject> { new GroupProject { GroupId = group.Id, Group = group } }
            };

            var board = new ProjectBoard { Id = 1, Project = project };
            var stage = new Stage { Id = 1, Name = "Stage", ProjectBoard = board };
            project.ProjectBoard = board;

            context.Users.Add(user);
            context.Projects.Add(project);
            context.Groups.Add(group);
            context.ProjectBoards.Add(board);
            context.Stages.Add(stage);
            await context.SaveChangesAsync();

            var controller = new ProjectController(context, CreateUserManager(user, false))
            {
                ControllerContext = CreateControllerContext(user)
            };

            var result = await controller.CreateTask(stage.Id);
            var view = Assert.IsType<ViewResult>(result);
            Assert.IsType<CreateTaskViewModel>(view.Model);
        }

        [Fact]
        public async Task CreateTask_Get_ReturnsView_ForMember()
        {
            using var context = CreateContext(Guid.NewGuid().ToString());
            var user = new User { Id = 1 };
            var group = new Group { Id = 1, Name = "Group", Description = "Desc" };

            context.UserGroups.Add(new UserGroup { UserId = 1, GroupId = 1, Role = "Employee" });

            var project = new Project
            {
                Id = 1,
                Name = "Project D",
                Description = "Test D",
                ProjectLeadId = 999,
                ProjectGroups = new List<GroupProject> { new GroupProject { GroupId = group.Id, Group = group } }
            };

            var board = new ProjectBoard { Id = 1, Project = project };
            var stage = new Stage { Id = 1, Name = "Stage", ProjectBoard = board };
            project.ProjectBoard = board;

            context.Users.Add(user);
            context.Projects.Add(project);
            context.Groups.Add(group);
            context.ProjectBoards.Add(board);
            context.Stages.Add(stage);
            await context.SaveChangesAsync();

            var controller = new ProjectController(context, CreateUserManager(user, false))
            {
                ControllerContext = CreateControllerContext(user)
            };

            var result = await controller.CreateTask(stage.Id);
            var view = Assert.IsType<ViewResult>(result);
            Assert.IsType<CreateTaskViewModel>(view.Model);
        }



        [Fact]
        public async Task MoveTask_Valid_MoveUpdatesStages()
        {
            using var context = CreateContext(Guid.NewGuid().ToString());

            var user = new User { Id = 1 };
            var task = new TaskManagerWebsite.Models.Task
            {
                Id = 1,
                CreatorUserId = user.Id,
                Name = "Move Task",
                Description = "Test description"
            };

            var board = new ProjectBoard { Id = 1, ProjectId = 1 };
            var stage1 = new Stage { Id = 1, Name = "To Do", ProjectBoard = board };
            var stage2 = new Stage { Id = 2, Name = "In Progress", ProjectBoard = board };

            var taskStage = new TaskStage
            {
                Id = 1,
                TaskId = task.Id,
                StageId = stage1.Id,
                CompletedDate = null
            };

            context.Users.Add(user);
            context.Tasks.Add(task);
            context.ProjectBoards.Add(board);
            context.Stages.AddRange(stage1, stage2);
            context.TaskStages.Add(taskStage);
            await context.SaveChangesAsync();

            var controller = new ProjectController(context, CreateUserManager(user, false));
            controller.ControllerContext = CreateControllerContext(user);

            var result = await controller.MoveTask(task.Id, stage1.Id, stage2.Id);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("ProjectBoard", redirect.ActionName);
        }
    }
}