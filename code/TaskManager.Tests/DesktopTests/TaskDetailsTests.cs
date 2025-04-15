using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TaskManagerData.Models;
using TaskManagerDesktop;
using TaskManagerDesktop.ViewModels;
using Task = System.Threading.Tasks.Task;

namespace TaskManager.Tests.DesktopTests
{
    public class TaskDetailsTests
    {
        private readonly Mock<IServiceProvider> _mockServiceProvider;
        private readonly ApplicationDbContext _dbContext;

        public TaskDetailsTests()
        {
            _dbContext = TestHelper.GetDbContext();
            _mockServiceProvider = GetMockServiceProvider();
        }

        public Mock<IServiceProvider> GetMockServiceProvider()
        {
            var serviceProviderMock = new Mock<IServiceProvider>();

            var serviceScopeMock = new Mock<IServiceScope>();

            var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();

            serviceScopeFactoryMock
                .Setup(factory => factory.CreateScope())
                .Returns(serviceScopeMock.Object);

            serviceScopeMock
                .Setup(scope => scope.ServiceProvider)
                .Returns(serviceProviderMock.Object);

            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(IServiceScopeFactory)))
                .Returns(serviceScopeFactoryMock.Object);

            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(ApplicationDbContext)))
                .Returns(_dbContext);

            return serviceProviderMock;
        }

        [Fact]
        public async Task UpdateTask_NoChanges()
        {
            var user = new User()
            {
                Id = 1,
                UserName = "User1",
                Email = "user1@example.com"
            };

            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            var task = new TaskManagerData.Models.Task
            {
                Id = 1,
                Name = "Test Task",
                Description = "Test Description",
                CreatorUserId = 1
            };

            var stage = new Stage
            {
                Id = 1,
                Name = "Test Stage",
                ProjectBoardId = 1
            };

            var taskStage = new TaskStage
                { Id = 1, TaskId = 1, StageId = 1, Stage = stage, EnteredDate = DateTime.Now, UpdatedByUserId = 1 };
            task.TaskStages.Add(taskStage);

            _dbContext.Projects.Add(new Project { Id = 1, Name = "Project1", Description = "Test Description" });
            _dbContext.ProjectBoards.Add(new ProjectBoard { Id = 1, ProjectId = 1, BoardCreatorId = 1});
            _dbContext.Stages.Add(stage);
            _dbContext.Tasks.Add(task);
            _dbContext.TaskStages.Add(taskStage);

            await _dbContext.SaveChangesAsync();

            var taskDetailsViewModel = new TaskDetailsViewModel(_mockServiceProvider.Object, task)
            {
                SelectedStage = stage,
                AssignedUser = null!
            };

            await taskDetailsViewModel.PopulateFields(task);

            await taskDetailsViewModel.UpdateTask();

            var updatedTask = await _dbContext.Tasks
                .Include(t => t.TaskStages)
                .ThenInclude(ts => ts.Stage)
                .FirstOrDefaultAsync(t => t.Id == task.Id);

            Assert.NotNull(updatedTask);
            Assert.Equal(task.Name, updatedTask.Name);
            Assert.Equal(task.Description, updatedTask.Description);
            Assert.Equal(task.TaskStages.First().StageId, updatedTask.TaskStages.First().StageId);
        }

        [Fact]
        public async Task UpdateTask_StageAndAssigneeChanged_WithNAAssignee()
        {
            var user = new User()
            {
                Id = 1,
                UserName = "User1",
                Email = "user1@example.com"
            };

            var manager = new User()
            {
                Id = 2,
                UserName = "Manager1",
                Email = "manager1@example.com"
            };

            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            var group = new Group
            {
                Id = 1,
                Name = "Test Group",
                Description = "Test Description",
                ManagerId = 2
            };

            var userGroup = new UserGroup
            {
                UserId = 2,
                GroupId = 1,
                Role = "Manager"
            };

            var task = new TaskManagerData.Models.Task
            {
                Id = 1,
                Name = "Test Task",
                Description = "Test Description",
                CreatorUserId = 1
            };

            var taskEmployee = new TaskEmployee
            {
                Id = 1,
                TaskId = 1,
                EmployeeId = 1,
                AssignedDate = DateTime.Now
            };

            var stage = new Stage
            {
                Id = 1,
                Name = "Test Stage",
                ProjectBoardId = 1
            };

            var newStage = new Stage
            {
                Id = 2,
                Name = "New Stage",
                ProjectBoardId = 1,
                AssignedGroupId = 1
            };

            var taskStage = new TaskStage
                { Id = 1, TaskId = 1, StageId = 1, Stage = stage, EnteredDate = DateTime.Now, UpdatedByUserId = 1 };
            task.TaskStages.Add(taskStage);

            _dbContext.Groups.Add(group);
            _dbContext.UserGroups.Add(userGroup);
            _dbContext.Projects.Add(new Project { Id = 1, Name = "Project1", Description = "Test Description" });
            _dbContext.ProjectBoards.Add(new ProjectBoard { Id = 1, ProjectId = 1, BoardCreatorId = 1 });
            _dbContext.Stages.Add(stage);
            _dbContext.Tasks.Add(task);
            _dbContext.TaskEmployees.Add(taskEmployee);
            _dbContext.TaskStages.Add(taskStage);

            await _dbContext.SaveChangesAsync();

            Session.CurrentUser = user;

            var taskDetailsViewModel = new TaskDetailsViewModel(_mockServiceProvider.Object, task)
            {
                SelectedStage = stage,
                AssignedUser = new UserOption { User = user }
            };

            await taskDetailsViewModel.PopulateFields(task);
            taskDetailsViewModel.SelectedStage = newStage;
            taskDetailsViewModel.UpdateAssignedToComboBox(new ComboBox{Items = { new UserOption{User = user} }});

            await taskDetailsViewModel.UpdateTask();

            var updatedTask = await _dbContext.Tasks
                .Include(t => t.TaskStages)
                .ThenInclude(ts => ts.Stage)
                .FirstOrDefaultAsync(t => t.Id == task.Id);

            var newTaskStage = _dbContext.TaskStages
                .FirstOrDefault(ts => ts.TaskId == updatedTask.Id && ts.StageId == newStage.Id);

            var newTaskEmployee = _dbContext.TaskEmployees.FirstOrDefault(te => te.EmployeeId == user.Id && te.TaskId == updatedTask.Id);

            Assert.NotNull(updatedTask);
            Assert.Equal(task.Name, updatedTask.Name);
            Assert.Equal(task.Description, updatedTask.Description);
            Assert.NotNull(taskStage.CompletedDate.Value);
            Assert.NotNull(newTaskStage);
            Assert.Null(newTaskEmployee);
            Assert.True(taskDetailsViewModel.MovedTaskToUnreachableStage);

        }

        [Fact]
        public async Task UpdateTask_StageAndAssigneeChanged_WithAssignee()
        {
            var user = new User()
            {
                Id = 1,
                UserName = "User1",
                Email = "user1@example.com"
            };

            var manager = new User()
            {
                Id = 2,
                UserName = "Manager1",
                Email = "manager1@example.com"
            };

            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            var task = new TaskManagerData.Models.Task
            {
                Id = 1,
                Name = "Test Task",
                Description = "Test Description",
                CreatorUserId = 1
            };

            var stage = new Stage
            {
                Id = 1,
                Name = "Test Stage",
                ProjectBoardId = 1
            };

            var newStage = new Stage
            {
                Id = 2,
                Name = "New Stage",
                ProjectBoardId = 1,
                AssignedGroupId = 1
            };

            var taskStage = new TaskStage
            { Id = 1, TaskId = 1, StageId = 1, Stage = stage, EnteredDate = DateTime.Now, UpdatedByUserId = 1 };
            task.TaskStages.Add(taskStage);

            _dbContext.Projects.Add(new Project { Id = 1, Name = "Project1", Description = "Test Description" });
            _dbContext.ProjectBoards.Add(new ProjectBoard { Id = 1, ProjectId = 1, BoardCreatorId = 1 });
            _dbContext.Stages.Add(stage);
            _dbContext.Tasks.Add(task);
            _dbContext.TaskStages.Add(taskStage);

            await _dbContext.SaveChangesAsync();

            Session.CurrentUser = user;

            var taskDetailsViewModel = new TaskDetailsViewModel(_mockServiceProvider.Object, task)
            {
                SelectedStage = stage,
                AssignedUser = null!
            };

            await taskDetailsViewModel.PopulateFields(task);

            taskDetailsViewModel.AssignedUser = new UserOption { User = user };
            taskDetailsViewModel.SelectedStage = newStage;

            await taskDetailsViewModel.UpdateTask();

            var updatedTask = await _dbContext.Tasks
                .Include(t => t.TaskStages)
                .ThenInclude(ts => ts.Stage)
                .FirstOrDefaultAsync(t => t.Id == task.Id);

            var newTaskStage = _dbContext.TaskStages
                .FirstOrDefault(ts => ts.TaskId == updatedTask.Id && ts.StageId == newStage.Id);

            var newTaskEmployee = _dbContext.TaskEmployees.FirstOrDefault(te => te.EmployeeId == user.Id && te.TaskId == updatedTask.Id);

            Assert.NotNull(updatedTask);
            Assert.Equal(task.Name, updatedTask.Name);
            Assert.Equal(task.Description, updatedTask.Description);
            Assert.NotNull(taskStage.CompletedDate.Value);
            Assert.NotNull(newTaskStage);
            Assert.NotNull(newTaskEmployee);
        }
    }
}
