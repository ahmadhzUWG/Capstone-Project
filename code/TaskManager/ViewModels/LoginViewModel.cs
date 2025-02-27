using System.ComponentModel.DataAnnotations;

namespace TaskManagerWebsite.ViewModels
{
    /// <summary>
    /// Represents the data model for user login.
    /// </summary>
    public class LoginViewModel
    {
        /// <summary>
        /// Gets or sets the username for login authentication.
        /// This field is required.
        /// </summary>
        [Required]
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the password for login authentication.
        /// This field is required and will be masked as a password input.
        /// </summary>
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }
}
