namespace TaskManagerData.Models
{
    /// <summary>
    /// Represents a single column (stage) on a Kanban board.
    /// </summary>
    public class Stage
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
        /// Gets or sets the position.
        /// </summary>
        public int Position { get; set; }

        /// <summary>
        /// Gets or sets the project board identifier.
        /// </summary>
        public int ProjectBoardId { get; set; }

        /// <summary>
        /// Gets or sets the project board.
        /// </summary>
        public ProjectBoard ProjectBoard { get; set; }

        /// <summary>
        /// Gets or sets the creator group identifier.
        /// </summary>
        public int? CreatorGroupId { get; set; }

        /// <summary>
        /// Gets or sets the creator group.
        /// </summary>
        public Group? CreatorGroup { get; set; }

        /// <summary>
        /// Gets or sets the creator user identifier.
        /// </summary>
        public int CreatorUserId { get; set; }

        /// <summary>
        /// Gets or sets the creator user.
        /// </summary>
        public User CreatorUser { get; set; }

        /// <summary>
        /// Gets or sets the assigned group identifier.
        /// </summary>
        public int? AssignedGroupId { get; set; }

        /// <summary>
        /// Gets or sets the assigned group.
        /// </summary>
        public Group AssignedGroup { get; set; }

        /// <summary>
        /// Gets or sets the collection of stages associations linked to this task.
        /// A task can be associated with multiple stages.
        /// </summary>
        public ICollection<TaskStage> TaskStages { get; set; } = new List<TaskStage>();
    }
    
}
