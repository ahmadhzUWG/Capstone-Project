using TaskManagerData.Models;

namespace TaskManagerWebsite.ViewModels.ProjectViewModels
{
    /// <summary>
    /// The ProjectBoardViewModel
    /// </summary>
    public class ProjectBoardViewModel
    {
        /// <summary>
        /// Gets or sets the project.
        /// </summary>
        public Project Project { get; set; }

        /// <summary>
        /// Gets or sets the stage form.
        /// </summary>
        public CreateStageViewModel StageForm { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance can add stage.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance can add stage; otherwise, <c>false</c>.
        /// </value>
        public bool CanAddStage { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance can delete any task.
        /// </summary>
        public bool CanDeleteAnyTask { get; set; }

        /// <summary>
        /// Gets or sets the logged in employee identifier.
        /// </summary>
        /// <value>
        /// The logged in employee identifier.
        /// </value>
        public int LoggedInEmployeeId { get; set; }

        /// <summary>
        /// Gets or sets the logged in user group identifier.
        /// </summary>
        /// <value>
        /// The logged in user group identifier.
        /// </value>
        public int LoggedInUserGroupId { get; set; }
    }
}