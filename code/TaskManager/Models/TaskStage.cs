namespace TaskManagerWebsite.Models
{
    /// <summary>
    /// Task information when assigned to a Stage
    /// </summary>
    public class TaskStage
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the stage identifier.
        /// </summary>
        public int StageId { get; set; }

        /// <summary>
        /// Gets or sets the task identifier.
        /// </summary>
        public int TaskId { get; set; }

        /// <summary>
        /// Gets or sets the entered date.
        /// </summary>
        public DateTime EnteredDate { get; set; }

        /// <summary>
        /// Gets or sets the completed date.
        /// </summary>
        public DateTime? CompletedDate { get; set; }

        /// <summary>
        /// Gets or sets the user Id of who updated the task last
        /// </summary>
        public int? UpdatedByUserId { get; set; }
    }
    
}
