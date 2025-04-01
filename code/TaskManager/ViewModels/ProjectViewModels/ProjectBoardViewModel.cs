using TaskManagerWebsite.Models;

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
        /// Gets or sets a value indicating whether this instance can add task.
        /// </summary>
        public bool CanAddTask { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance can delete any task.
        /// </summary>
        public bool CanDeleteAnyTask { get; set; }
    }
}