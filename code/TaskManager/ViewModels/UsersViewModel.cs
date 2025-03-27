using System.ComponentModel.DataAnnotations;
using TaskManagerWebsite.Models;

namespace TaskManagerWebsite.ViewModels
{
    /// <summary>
    /// Represents the data model for creating an admin user.
    /// </summary>
    public class UsersViewModel
    {
        /// <summary>
        /// Gets or sets the users.
        /// </summary>
        /// <value>
        /// The users.
        /// </value>
        public List<User> Users { get; set; }

        /// <summary>
        /// Gets or sets the success message.
        /// </summary>
        /// <value>
        /// The success message.
        /// </value>
        public string SuccessMessage { get; set; }

        /// <summary>
        /// Gets or sets the error message.
        /// </summary>
        /// <value>
        /// The error message.
        /// </value>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets the undeleted user identifier.
        /// </summary>
        /// <value>
        /// The undeleted user identifier.
        /// </value>
        public int UndeletedUserId { get; set; }

        /// <summary>
        /// Gets or sets the related groups.
        /// </summary>
        /// <value>
        /// The related groups.
        /// </value>
        public List<Group> RelatedGroups { get; set; }

        /// <summary>
        /// Gets or sets the related projects.
        /// </summary>
        /// <value>
        /// The related projects.
        /// </value>
        public List<Project?> RelatedProjects { get; set; }
    }
}