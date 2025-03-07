using System.ComponentModel.DataAnnotations;

namespace TaskManagerWebsite.ViewModels
{
    /// <summary>
    /// Represents the data model for creating an admin user.
    /// </summary>
    public class UserViewModel
    {
        /// <summary>
        /// Gets or sets the username for the admin.
        /// </summary>
        [Required(ErrorMessage = "User name is required. Please provide a unique user name for this admin.")]
        [Display(Name = "User Name")]
        public required string UserName { get; set; }

        /// <summary>
        /// Gets or sets the email address of the admin.
        /// </summary>
        [Required(ErrorMessage = "Email address is required. Please provide a valid email for this admin.")]
        [EmailAddress(ErrorMessage = "Please provide a valid email address (e.g., user@example.com).")]
        [Display(Name = "Email")]
        public required string Email { get; set; }

        /// <summary>
        /// Gets or sets the password for the admin.
        /// </summary>
        [Required(ErrorMessage = "A password is required. Please use at least 8 characters, mixing letters and digits.")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public required string Password { get; set; }

        /// <summary>
        /// Gets or sets the confirmation password, which must match the password.
        /// </summary>
        [Required(ErrorMessage = "Please confirm your password to ensure accuracy.")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match. Make sure both fields are the same.")]
        [Display(Name = "Confirm Password")]
        public required string ConfirmPassword { get; set; }
    }
}