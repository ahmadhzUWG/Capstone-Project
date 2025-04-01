using TaskManagerWebsite.Models;
using Task = TaskManagerWebsite.Models.Task;

namespace TaskManager.Tests.Tests.TestModels
{
    public class TaskTests
    {
        [Fact]
        public void Task_Properties_CanBeSetAndRetrieved()
        {
            var user = new User { Id = 10, UserName = "creatorUser" };
            var task = new Task
            {
                Id = 1,
                Name = "Design UI",
                Description = "Create the user interface for the dashboard",
                CreatorUserId = 10,
                CreatorUser = user
            };

            Assert.Equal(1, task.Id);
            Assert.Equal("Design UI", task.Name);
            Assert.Equal("Create the user interface for the dashboard", task.Description);
            Assert.Equal(10, task.CreatorUserId);
            Assert.Equal(user, task.CreatorUser);
        }

        [Fact]
        public void Task_TaskEmployees_DefaultsToEmptyList()
        {
            var task = new Task();
            Assert.NotNull(task.TaskEmployees);
            Assert.Empty(task.TaskEmployees);
        }

        [Fact]
        public void Task_TaskStages_DefaultsToEmptyList()
        {
            var task = new Task();
            Assert.NotNull(task.TaskStages);
            Assert.Empty(task.TaskStages);
        }

        [Fact]
        public void Task_TaskEmployees_CanBeAssignedAndRetrieved()
        {
            var taskEmployees = new List<TaskEmployee>
            {
                new TaskEmployee { TaskId = 1, EmployeeId = 2 },
                new TaskEmployee { TaskId = 1, EmployeeId = 3 }
            };

            var task = new Task
            {
                Id = 1,
                TaskEmployees = taskEmployees
            };

            Assert.Equal(2, task.TaskEmployees.Count);
            Assert.Contains(task.TaskEmployees, te => te.EmployeeId == 2);
            Assert.Contains(task.TaskEmployees, te => te.EmployeeId == 3);
        }

        [Fact]
        public void Task_TaskStages_CanBeAssignedAndRetrieved()
        {
            var taskStages = new List<TaskStage>
            {
                new TaskStage { TaskId = 1, StageId = 5 },
                new TaskStage { TaskId = 1, StageId = 6 }
            };

            var task = new Task
            {
                Id = 1,
                TaskStages = taskStages
            };

            Assert.Equal(2, task.TaskStages.Count);
            Assert.Contains(task.TaskStages, ts => ts.StageId == 5);
            Assert.Contains(task.TaskStages, ts => ts.StageId == 6);
        }
    }
}
