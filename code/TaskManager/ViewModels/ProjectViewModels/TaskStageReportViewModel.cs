namespace TaskManagerWebsite.ViewModels.ProjectViewModels
{
    /// <summary>
    /// 
    /// </summary>
    public class TaskStageReportViewModel
    {
        /// <summary>
        /// Gets or sets the report title.
        /// </summary>
        /// <value>
        /// The report title.
        /// </value>
        public string ReportTitle { get; set; }
        /// <summary>
        /// Gets or sets the report items.
        /// </summary>
        /// <value>
        /// The report items.
        /// </value>
        public List<TaskStageReportItemViewModel> ReportItems { get; set; } = new List<TaskStageReportItemViewModel>();

        // Optional properties for back navigation.
        /// <summary>
        /// Gets or sets the project identifier.
        /// </summary>
        /// <value>
        /// The project identifier.
        /// </value>
        public int? ProjectId { get; set; }
        /// <summary>
        /// Gets or sets the group identifier.
        /// </summary>
        /// <value>
        /// The group identifier.
        /// </value>
        public int? GroupId { get; set; }
    }
}
