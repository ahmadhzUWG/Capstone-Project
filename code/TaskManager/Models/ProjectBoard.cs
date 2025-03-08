namespace TaskManagerWebsite.Models
{
    /// <summary>
    /// Represents a Kanban Board for a Project
    /// </summary>
    public class ProjectBoard
    {
        public int Id { get; set; }

        /// <summary>
        /// Foreign key to the associated project.
        /// </summary>
        public int ProjectId { get; set; }

        /// <summary>
        /// Associated project.
        /// </summary>
        public Project Project { get; set; }

        /// <summary>
        /// Gets or sets the board creator identifier.
        /// </summary>
        public int BoardCreatorId { get; set; }

        /// <summary>
        /// Gets or sets the board creator.
        /// </summary>
        public User BoardCreator { get; set; }

        /// <summary>
        /// Collection of stages (columns) on this board.
        /// </summary>
        public ICollection<Stage> Stages { get; set; }
    }
}
