using System;
using TaskManagerWebsite.Models;
using Xunit;

public class TaskStageTests
{
    [Fact]
    public void TaskStage_CanSetAndGetProperties()
    {
        // Arrange
        var task = new TaskManagerWebsite.Models.Task { Id = 1, Name = "Sample Task" };
        var stage = new Stage { Id = 2, Name = "To Do" };
        var user = new User { Id = 3, UserName = "updatedByUser" };
        var now = DateTime.UtcNow;

        // Act
        var taskStage = new TaskStage
        {
            Id = 10,
            TaskId = task.Id,
            Task = task,
            StageId = stage.Id,
            Stage = stage,
            EnteredDate = now,
            CompletedDate = now.AddDays(1),
            UpdatedByUserId = user.Id,
            UpdatedByUser = user
        };

        // Assert
        Assert.Equal(10, taskStage.Id);
        Assert.Equal(task.Id, taskStage.TaskId);
        Assert.Equal(task, taskStage.Task);
        Assert.Equal(stage.Id, taskStage.StageId);
        Assert.Equal(stage, taskStage.Stage);
        Assert.Equal(now, taskStage.EnteredDate);
        Assert.Equal(now.AddDays(1), taskStage.CompletedDate);
        Assert.Equal(user.Id, taskStage.UpdatedByUserId);
        Assert.Equal(user, taskStage.UpdatedByUser);
    }

    [Fact]
    public void TaskStage_CompletedDate_CanBeNull()
    {
        // Arrange & Act
        var taskStage = new TaskStage
        {
            CompletedDate = null
        };

        // Assert
        Assert.Null(taskStage.CompletedDate);
    }

    [Fact]
    public void TaskStage_UpdatedByUser_CanBeNull()
    {
        // Arrange & Act
        var taskStage = new TaskStage
        {
            UpdatedByUser = null,
            UpdatedByUserId = null
        };

        // Assert
        Assert.Null(taskStage.UpdatedByUser);
        Assert.Null(taskStage.UpdatedByUserId);
    }

    [Fact]
    public void TaskStage_CanBeCreatedWithMinimumProperties()
    {
        // Arrange
        var taskStage = new TaskStage
        {
            TaskId = 1,
            StageId = 2,
            EnteredDate = DateTime.UtcNow
        };

        // Assert
        Assert.Equal(1, taskStage.TaskId);
        Assert.Equal(2, taskStage.StageId);
        Assert.NotEqual(default, taskStage.EnteredDate);
    }
}
