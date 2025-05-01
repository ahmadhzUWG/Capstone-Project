﻿using Microsoft.AspNetCore.Identity;
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
        public async Task IsMovingToUnreachableStage_ReturnsFalse_WhenUserInAssignedGroup()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var dbContext = new ApplicationDbContext(options);

            var userId = 1;
            Session.CurrentUser = new User { Id = userId };

            var task = new TaskManagerData.Models.Task
            {
                Id = 100,
                Name = "Test Task",
                Description = "Test Description",
                CreatorUserId = 1
            };

            var originalStage = new Stage { Id = 1, Name = "Original Stage", ProjectBoardId = 1 };
            var newStage = new Stage { Id = 2, Name = "New Stage", ProjectBoardId = 1, AssignedGroupId = 10 };

            var projectBoard = new ProjectBoard { Id = 1, ProjectId = 1 };
            var project = new Project { Id = 1, Name = "Test Project", Description = "Test description"};
            var taskStage = new TaskStage { TaskId = task.Id, StageId = originalStage.Id };

            dbContext.Stages.AddRange(originalStage, newStage);
            dbContext.ProjectBoards.Add(projectBoard);
            dbContext.Projects.Add(project);
            dbContext.TaskStages.Add(taskStage);
            dbContext.Users.Add(Session.CurrentUser);
            dbContext.UserGroups.Add(new UserGroup { UserId = userId, GroupId = 10, Role = "Member"}); 
            await dbContext.SaveChangesAsync();

            var sp = new ServiceCollection()
                .AddSingleton(dbContext)
                .BuildServiceProvider();

            var serviceProviderMock = new Mock<IServiceProvider>();
            var scopeMock = new Mock<IServiceScope>();
            var scopeFactoryMock = new Mock<IServiceScopeFactory>();

            serviceProviderMock.Setup(sp => sp.GetService(typeof(IServiceScopeFactory)))
                .Returns(scopeFactoryMock.Object);
            scopeFactoryMock.Setup(sf => sf.CreateScope())
                .Returns(scopeMock.Object);
            scopeMock.Setup(s => s.ServiceProvider).Returns(sp);

            var viewModel = new TaskDetailsViewModel(serviceProviderMock.Object, task);

            await viewModel.PopulateFields(task); // initializes _originalStage internally

            viewModel.SelectedStage = newStage; 

            // Act
            var result = await viewModel.IsMovingToUnreachableStage();

            // Assert
            Assert.False(result);
        }


        [Fact]
        public async Task IsMovingToUnreachableStage_ReturnsTrue_WhenUserNotInAssignedGroup()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var dbContext = new ApplicationDbContext(options);

            var userId = 1;
            Session.CurrentUser = new User { Id = userId };

            var task = new TaskManagerData.Models.Task
            {
                Id = 300,
                Name = "Test Task",
                Description = "Test Description",
                CreatorUserId = 1
            };

            var stageOriginal = new Stage { Id = 1, Name = "Original", ProjectBoardId = 1 };
            var stageNew = new Stage { Id = 2, Name = "New Stage", ProjectBoardId = 1, AssignedGroupId = 10 };

            var projectBoard = new ProjectBoard { Id = 1, ProjectId = 1 };
            var project = new Project { Id = 1, Name = "Test Project", Description = "Test description"};
            var taskStage = new TaskStage { TaskId = task.Id, StageId = stageOriginal.Id };

            dbContext.Stages.AddRange(stageOriginal, stageNew);
            dbContext.ProjectBoards.Add(projectBoard);
            dbContext.Projects.Add(project);
            dbContext.TaskStages.Add(taskStage);
            dbContext.Users.Add(Session.CurrentUser);
            dbContext.UserGroups.Add(new UserGroup { UserId = userId, GroupId = 99, Role = "Member"}); // user is NOT in group 10
            await dbContext.SaveChangesAsync();

            var serviceProviderMock = new Mock<IServiceProvider>();
            var scopeMock = new Mock<IServiceScope>();
            var scopeFactoryMock = new Mock<IServiceScopeFactory>();

            var sp = new ServiceCollection()
                .AddSingleton(dbContext)
                .BuildServiceProvider();

            serviceProviderMock.Setup(sp => sp.GetService(typeof(IServiceScopeFactory)))
                .Returns(scopeFactoryMock.Object);
            scopeFactoryMock.Setup(sf => sf.CreateScope())
                .Returns(scopeMock.Object);
            scopeMock.Setup(s => s.ServiceProvider).Returns(sp);

            var viewModel = new TaskDetailsViewModel(serviceProviderMock.Object, task);

            await viewModel.PopulateFields(task); 

            viewModel.SelectedStage = stageNew; 

            // Act
            var result = await viewModel.IsMovingToUnreachableStage();

            // Assert
            Assert.True(result);
        }



        [Fact]
        public async Task GetComments_ReturnsCommentsForTask_OrderedDescending()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var dbContext = new ApplicationDbContext(options);

            var user = new User { Id = 1, UserName = "User1" };
            var task = new TaskManagerData.Models.Task 
            { 
                Id = 200, 
                Name = "Test Task",
                Description = "Test Description",
                CreatorUserId = 1
            };
            var comment1 = new Comment { Id = 1, TaskId = task.Id, User = user, Timestamp = DateTime.Now.AddMinutes(-5), Content = "Old comment" };
            var comment2 = new Comment { Id = 2, TaskId = task.Id, User = user, Timestamp = DateTime.Now, Content = "New comment" };

            dbContext.Users.Add(user);
            dbContext.Tasks.Add(task);
            dbContext.Comments.AddRange(comment1, comment2);
            await dbContext.SaveChangesAsync();

            var serviceProviderMock = new Mock<IServiceProvider>();
            var scopeMock = new Mock<IServiceScope>();
            var scopeFactoryMock = new Mock<IServiceScopeFactory>();

            serviceProviderMock.Setup(sp => sp.GetService(typeof(IServiceScopeFactory)))
                .Returns(scopeFactoryMock.Object);
            scopeFactoryMock.Setup(sf => sf.CreateScope())
                .Returns(scopeMock.Object);
            scopeMock.Setup(s => s.ServiceProvider)
                .Returns(new ServiceCollection().AddSingleton(dbContext).BuildServiceProvider());

            var viewModel = new TaskDetailsViewModel(serviceProviderMock.Object, task);

            // Act
            var result = await viewModel.GetComments();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal(comment2.Id, result[0].Id); // newest first
            Assert.Equal(comment1.Id, result[1].Id);
            Assert.All(result, c => Assert.NotNull(c.User));
        }


        [Fact]
        public async Task GetTaskHistories_ReturnsHistoriesForTask_OrderedDescending()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var dbContext = new ApplicationDbContext(options);

            var user = new User { Id = 1, UserName = "User1" };
            var task = new TaskManagerData.Models.Task {
                Id = 100,
                Name = "Test Task",
                Description = "Test Description",
                CreatorUserId = 1
            };
            var history1 = new TaskHistory { Id = 1, TaskId = task.Id, User = user, Timestamp = DateTime.Now.AddMinutes(-10), Action = "for test"};
            var history2 = new TaskHistory { Id = 2, TaskId = task.Id, User = user, Timestamp = DateTime.Now, Action = "for test" };

            dbContext.Users.Add(user);
            dbContext.Tasks.Add(task);
            dbContext.TaskHistories.AddRange(history1, history2);
            await dbContext.SaveChangesAsync();

            var serviceProviderMock = new Mock<IServiceProvider>();
            var scopeMock = new Mock<IServiceScope>();
            var scopeFactoryMock = new Mock<IServiceScopeFactory>();

            serviceProviderMock.Setup(sp => sp.GetService(typeof(IServiceScopeFactory)))
                .Returns(scopeFactoryMock.Object);
            scopeFactoryMock.Setup(sf => sf.CreateScope())
                .Returns(scopeMock.Object);
            scopeMock.Setup(s => s.ServiceProvider)
                .Returns(new ServiceCollection().AddSingleton(dbContext).BuildServiceProvider());

            var viewModel = new TaskDetailsViewModel(serviceProviderMock.Object, task);

            // Act
            var result = await viewModel.GetTaskHistories();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal(history2.Id, result[0].Id); // most recent first
            Assert.Equal(history1.Id, result[1].Id);
            Assert.All(result, h => Assert.NotNull(h.User));
        }


        [Fact]
        public async Task PostComment_AddsCommentToDatabase()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var dbContext = new ApplicationDbContext(options);

            // Set current user
            var user = new User { Id = 123 };
            Session.CurrentUser = user;

            // Add the user and task to db so foreign keys don’t break
            var task = new TaskManagerData.Models.Task
            {
                Id = 1,
                Name = "Test Task",
                Description = "Test Description",
                CreatorUserId = 1
            };
            dbContext.Users.Add(user);
            dbContext.Tasks.Add(task);
            await dbContext.SaveChangesAsync();

            // Setup service provider to return our dbContext
            var serviceProviderMock = new Mock<IServiceProvider>();
            var scopeMock = new Mock<IServiceScope>();
            var scopeFactoryMock = new Mock<IServiceScopeFactory>();

            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(IServiceScopeFactory)))
                .Returns(scopeFactoryMock.Object);

            scopeFactoryMock
                .Setup(sf => sf.CreateScope())
                .Returns(scopeMock.Object);

            scopeMock
                .Setup(s => s.ServiceProvider)
                .Returns(new ServiceCollection()
                    .AddSingleton(dbContext)
                    .BuildServiceProvider());

            var viewModel = new TaskDetailsViewModel(serviceProviderMock.Object, task)
            {
            };

            var commentText = "This is a test comment.";

            // Act
            await viewModel.PostComment(commentText);

            // Assert
            var savedComment = await dbContext.Comments.FirstOrDefaultAsync();
            Assert.NotNull(savedComment);
            Assert.Equal(task.Id, savedComment.TaskId);
            Assert.Equal(user.Id, savedComment.UserId);
            Assert.Equal(commentText, savedComment.Content);
            Assert.True((DateTime.Now - savedComment.Timestamp).TotalSeconds < 5); // timestamp close to now
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