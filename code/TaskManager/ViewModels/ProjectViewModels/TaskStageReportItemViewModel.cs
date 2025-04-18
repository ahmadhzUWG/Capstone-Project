namespace TaskManagerWebsite.ViewModels.ProjectViewModels
{
    /// <summary>
    /// 
    /// </summary>
    public class TaskStageReportItemViewModel
    {
        /// <summary>
        /// Gets or sets the task identifier.
        /// </summary>
        /// <value>
        /// The task identifier.
        /// </value>
        public int TaskId { get; set; }
        /// <summary>
        /// Gets or sets the name of the task.
        /// </summary>
        /// <value>
        /// The name of the task.
        /// </value>
        public string TaskName { get; set; }
        /// <summary>
        /// Gets or sets the name of the stage.
        /// </summary>
        /// <value>
        /// The name of the stage.
        /// </value>
        public string StageName { get; set; }
        /// <summary>
        /// Gets or sets the entered date.
        /// </summary>
        /// <value>
        /// The entered date.
        /// </value>
        public DateTime EnteredDate { get; set; }
        /// <summary>
        /// Gets or sets the completed date.
        /// </summary>
        /// <value>
        /// The completed date.
        /// </value>
        public DateTime? CompletedDate { get; set; }
        /// <summary>
        /// Gets or sets the updated by.
        /// </summary>
        /// <value>
        /// The updated by.
        /// </value>
        public string UpdatedBy { get; set; }
    }
}
