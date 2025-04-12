namespace TaskManagerData.Models
{
    /// <summary>
    /// Task that will be added to a stage on a project board
    /// </summary>
    public class Task
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the creator user identifier.
        /// </summary>
        public int CreatorUserId { get; set; }

        /// <summary>
        /// Gets or sets the creator user.
        /// </summary>
        public User CreatorUser { get; set; }

        /// <summary>
        /// Gets or sets the collection of employees associations linked to this task.
        /// A task can be associated with multiple employees.
        /// </summary>
        public ICollection<TaskEmployee> TaskEmployees { get; set; } = new List<TaskEmployee>();

        /// <summary>
        /// Gets or sets the collection of stages associations linked to this task.
        /// A task can be associated with multiple stages.
        /// </summary>
        public ICollection<TaskStage> TaskStages { get; set; } = new List<TaskStage>();

        /// <summary>
        /// Gets or sets the comments.
        /// </summary>
        /// <value>
        /// The comments.
        /// </value>
        public ICollection<Comment>? Comments { get; set; } = new List<Comment>();
    }
    
}
