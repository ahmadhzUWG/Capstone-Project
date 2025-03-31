namespace TaskManagerWebsite.Models
{
    /// <summary>
    /// Employee information when assigned to a task
    /// </summary>
    public class TaskEmployee
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Gets or sets the employee identifier.
        /// </summary>
        public int EmployeeId { get; set; }

        /// <summary>
        /// Gets or sets the employee.
        /// </summary>
        public User Employee { get; set; }

        /// <summary>
        /// Gets or sets the task identifier.
        /// </summary>
        public int TaskId { get; set; }

        /// <summary>
        /// Gets or sets the task.
        /// </summary>
        public Task Task { get; set; }

        /// <summary>
        /// Gets or sets the assigned date.
        /// </summary>
        public DateTime AssignedDate { get; set; }
        /// <summary>
        /// Gets or sets the completed date.
        /// </summary>
        public DateTime? CompletedDate { get; set; }
    }
}
