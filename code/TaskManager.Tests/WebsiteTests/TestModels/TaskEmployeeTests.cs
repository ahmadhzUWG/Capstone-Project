using TaskManagerData.Models;
using Task = TaskManagerData.Models.Task;

namespace TaskManager.Tests.WebsiteTests.TestModels
{
    public class TaskEmployeeTests
    {
        [Fact]
        public void TaskEmployee_CanSetAndGetProperties()
        {
            // Arrange
            var employee = new User { Id = 1, UserName = "jdoe" };
            var task = new Task { Id = 42, Name = "Test Task" };
            var assignedDate = new DateTime(2023, 1, 1);
            var completedDate = new DateTime(2023, 1, 10);

            // Act
            var taskEmployee = new TaskEmployee
            {
                Id = 5,
                EmployeeId = employee.Id,
                Employee = employee,
                TaskId = task.Id,
                Task = task,
                AssignedDate = assignedDate,
                CompletedDate = completedDate
            };

            // Assert
            Assert.Equal(5, taskEmployee.Id);
            Assert.Equal(1, taskEmployee.EmployeeId);
            Assert.Equal(employee, taskEmployee.Employee);
            Assert.Equal(42, taskEmployee.TaskId);
            Assert.Equal(task, taskEmployee.Task);
            Assert.Equal(assignedDate, taskEmployee.AssignedDate);
            Assert.Equal(completedDate, taskEmployee.CompletedDate);
        }

        [Fact]
        public void TaskEmployee_CompletedDate_CanBeNull()
        {
            // Arrange
            var taskEmployee = new TaskEmployee
            {
                CompletedDate = null
            };

            // Assert
            Assert.Null(taskEmployee.CompletedDate);
        }

    }
}
