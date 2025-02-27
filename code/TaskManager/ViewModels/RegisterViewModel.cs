using System.ComponentModel.DataAnnotations;

namespace TaskManagerWebsite.ViewModels
{

    /// <summary>
    /// Represents the data model for user registration.
    /// </summary>
    public class RegisterViewModel
    {
        /// <summary>
        /// Gets or sets the username for the new user.
        /// This field is required.
        /// </summary>
        [Required]
        [Display(Name = "User Name")]
        public required string UserName { get; set; }

        /// <summary>
        /// Gets or sets the email address of the user.
        /// This field is required and must be a valid email format.
        /// </summary>
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public required string Email { get; set; }

        /// <summary>
        /// Gets or sets the password for the new account.
        /// This field is required and will be masked as a password input.
        /// </summary>
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public required string Password { get; set; }

        /// <summary>
        /// Gets or sets the confirmation password.
        /// This field is required and must match the password field.
        /// </summary>
        [Required]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        [Display(Name = "Confirm Password")]
        public required string ConfirmPassword { get; set; }
    }
}
