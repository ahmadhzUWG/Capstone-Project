using TaskManagerData.Models;

namespace TaskManagerWebsite.ViewModels.ProjectViewModels
{
    /// <summary>
    /// View model for checking if a user has permission to access a stage
    /// </summary>
    public class StagePermissionViewModel
    {
        /// <summary>
        /// The stage that the user is trying to access
        /// </summary>
        public Stage Stage { get; set; }

        /// <summary>
        /// The user that is trying to access the stage
        /// </summary>
        public bool? IsUserAssignedToGroup { get; set; }

        /// <summary>
        /// Whether the user is an admin 
        /// </summary>
        public bool IsAdmin { get; set; }
    }
}
