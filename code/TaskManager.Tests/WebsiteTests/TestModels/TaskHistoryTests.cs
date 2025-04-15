using System;
using Xunit;
using TaskManagerData.Models;
using Task = TaskManagerData.Models.Task;

namespace TaskManager.Tests.Models
{
    public class TaskHistoryTests
    {
        [Fact]
        public void TaskHistory_Id_CanBeSetAndRetrieved()
        {
            var history = new TaskHistory { Id = 123 };
            Assert.Equal(123, history.Id);
        }

        [Fact]
        public void TaskHistory_TaskId_CanBeSetAndRetrieved()
        {
            var history = new TaskHistory { TaskId = 10 };
            Assert.Equal(10, history.TaskId);
        }

        [Fact]
        public void TaskHistory_Task_CanBeSetAndRetrieved()
        {
            // Assuming Task has an Id property
            var task = new Task { Id = 99 };
            var history = new TaskHistory { Task = task };
            Assert.NotNull(history.Task);
            Assert.Equal(99, history.Task.Id);
        }

        [Fact]
        public void TaskHistory_Timestamp_CanBeSetAndRetrieved()
        {
            var timeStamp = DateTime.Now;
            var history = new TaskHistory { Timestamp = timeStamp };
            Assert.Equal(timeStamp, history.Timestamp);
        }

        [Fact]
        public void TaskHistory_UserId_CanBeSetAndRetrieved()
        {
            var history = new TaskHistory { UserId = 15 };
            Assert.Equal(15, history.UserId);
        }

        [Fact]
        public void TaskHistory_User_CanBeSetAndRetrieved()
        {
            var user = new User { Id = 7, UserName = "TaskOwner" };
            var history = new TaskHistory { User = user };
            Assert.NotNull(history.User);
            Assert.Equal("TaskOwner", history.User.UserName);
        }

        [Fact]
        public void TaskHistory_Action_CanBeSetAndRetrieved()
        {
            var history = new TaskHistory { Action = "Task Created" };
            Assert.Equal("Task Created", history.Action);
        }
    }
}