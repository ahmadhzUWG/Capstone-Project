using System.ComponentModel.DataAnnotations;

namespace TaskManagerWebsite.ViewModels
{
    /// <summary>
    /// Represents the data model for finding a username by email.
    /// </summary>
    public class FindUsernameViewModel
    {
        /// <summary>
        /// Gets or sets the email address for finding the associated username.
        /// </summary>
        [Required(ErrorMessage = "Email is required.")]
        public required string Email { get; set; } = string.Empty;
    }
}
