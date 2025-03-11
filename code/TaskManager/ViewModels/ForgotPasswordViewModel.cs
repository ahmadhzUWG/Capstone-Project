using System.ComponentModel.DataAnnotations;
using Xunit.Sdk;

namespace TaskManagerWebsite.ViewModels
{
    /// <summary>
    /// Represents the data model for resetting a password.
    /// </summary>
    public class ForgotPasswordViewModel
    {
        /// <summary>
        ///   Gets or sets the email address for the admin.
        /// </summary>
        [Required(ErrorMessage = "Username is required")]
        public required string Username { get; set; } = string.Empty;

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
