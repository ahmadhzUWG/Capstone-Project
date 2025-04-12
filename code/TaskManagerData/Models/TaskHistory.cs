namespace TaskManagerData.Models
{
    /// <summary>
    /// Tracks the data necessary to record and display as task history.
    /// </summary>
    public class TaskHistory
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the task identifier.
        /// </summary>
        public int TaskId { get; set; }

        /// <summary>
        /// Gets or sets the task.
        /// </summary>
        public Task Task { get; set; }

        /// <summary>
        /// Gets or sets the timestamp.
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Gets or sets the user identifier.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Gets or sets the user.
        /// </summary>
        public User User { get; set; }

        /// <summary>
        /// Gets or sets the action that the user committed for the task.
        /// </summary>
        public string Action { get; set; }
    }

}
