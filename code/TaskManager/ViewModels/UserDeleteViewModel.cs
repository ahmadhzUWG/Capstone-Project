using TaskManagerData.Models;

namespace TaskManagerWebsite.ViewModels
{
    /// <summary>
    /// Represents the data model for deleting a user.
    /// </summary>
    public class UserDeleteViewModel
    {
        /// <summary>
        /// Gets or sets the user.
        /// </summary>
        /// <value>
        /// The user.
        /// </value>

        public required User User { get; set; }
        /// <summary>
        /// Gets or sets the related groups.
        /// </summary>
        /// <value>
        /// The related groups.
        /// </value>
        public required List<Group> RelatedGroups { get; set; }

        /// <summary>
        /// Gets or sets the related projects.
        /// </summary>
        /// <value>
        /// The related projects.
        /// </value>
        public required List<Project> RelatedProjects { get; set; }
    }
}
