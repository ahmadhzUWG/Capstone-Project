using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManagerData.Models;
using TaskManagerDesktop;
using TaskManagerDesktop.ViewModels;
using Task = System.Threading.Tasks.Task;

namespace TaskManager.Tests.DesktopTests
{
    public class TasksTests
    {
        private readonly Mock<IServiceProvider> _mockServiceProvider;
        private readonly ApplicationDbContext _dbContext;

        public TasksTests()
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
        public async Task GetAssignedTasks_ReturnsAssignedTasks()
        {
            // Arrange
            var user = new User { Id = 1, UserName = "User1"};
            var task1 = new TaskManagerData.Models.Task { Id = 1, Name = "Task 1", Description = "for task 1"};
            var task2 = new TaskManagerData.Models.Task { Id = 2, Name = "Task 2", Description = "for task 2"};

            _dbContext.Users.Add(user);
            _dbContext.Tasks.AddRange(task1, task2);
            _dbContext.TaskEmployees.Add(new TaskEmployee { EmployeeId = user.Id, TaskId = task1.Id });
            _dbContext.TaskEmployees.Add(new TaskEmployee { EmployeeId = user.Id, TaskId = task2.Id });
            await _dbContext.SaveChangesAsync();

            Session.CurrentUser = user;
            var tasksViewModel = new TasksViewModel(_mockServiceProvider.Object);

            // Act
            var assignedTasks = await tasksViewModel.GetAssignedTasks();

            // Assert
            Assert.NotNull(assignedTasks);
            Assert.Equal(2, assignedTasks.Count);
            Assert.True(assignedTasks.Any(t => t.Id == task1.Id));
            Assert.True(assignedTasks.Any(t => t.Id == task2.Id));
        }

        [Fact]
        public async Task GetAvailableTasks_ReturnsAvailableTasks()
        {
            // Arrange
            var user = new User { Id = 1, UserName = "User1" };
            var group = new Group { Id = 1, Name = "Group 1", ManagerId = user.Id, Description = "group 1 description"};
            var userGroup = new UserGroup { UserId = user.Id, GroupId = group.Id, Role = "Manager"};
            var project = new Project { Id = 1, Name = "Project 1", Description = "project 1 description"};
            var groupProject = new GroupProject { GroupId = group.Id, ProjectId = project.Id };
            var board = new ProjectBoard{ Id = 1, ProjectId = project.Id };
            var stage = new Stage { Id = 1, ProjectBoardId = board.Id, Name = "ToDo"};
            var task1 = new TaskManagerData.Models.Task { Id = 1, Name = "Task 1", Description = "for task 1" };
            var task2 = new TaskManagerData.Models.Task { Id = 2, Name = "Task 2", Description = "for task 2" };
            var taskStage1 = new TaskStage { TaskId = task1.Id, StageId = stage.Id };
            var taskStage2 = new TaskStage { TaskId = task2.Id, StageId = stage.Id };
            _dbContext.Groups.Add(group);
            _dbContext.UserGroups.Add(userGroup);
            _dbContext.Projects.Add(project);
            _dbContext.GroupProjects.Add(groupProject);
            _dbContext.ProjectBoards.Add(board);
            _dbContext.Stages.Add(stage);
            _dbContext.Users.Add(user);
            _dbContext.Tasks.AddRange(task1, task2);
            _dbContext.TaskStages.AddRange(taskStage1, taskStage2);
            await _dbContext.SaveChangesAsync();

            Session.CurrentUser = user;
            var tasksViewModel = new TasksViewModel(_mockServiceProvider.Object);

            // Act
            var availableTasks = await tasksViewModel.GetAvailableTasks();

            // Assert
            Assert.NotNull(availableTasks);
            Assert.Equal(2, availableTasks.Count);
            Assert.True(availableTasks.Any(t => t.Id == task1.Id));
            Assert.True(availableTasks.Any(t => t.Id == task2.Id));
        }
    }
}
