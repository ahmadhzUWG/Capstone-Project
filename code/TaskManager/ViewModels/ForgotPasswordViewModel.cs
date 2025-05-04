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

        /// <summary>
        /// Gets or sets the one-time code sent to the user's email address.
        /// </summary>
        [Required(ErrorMessage = "Please enter the One-Time code sent to the email associated with the username.")]
        [DataType(DataType.Password)]
        [Display(Name = "Enter One-Time Code")]
        public required string OneTimeCode { get; set; } = string.Empty;

        /// <summary>
        /// /// Gets or sets the boolean indicating whether the one-time code has been sent.
        /// </summary>
        public bool SentOneTime { get; set; } = false;

        /// <summary>
        /// Gets or sets the boolean indicating whether the one-time code has been verified.
        /// </summary>
        public bool VerifiedOneTime { get; set; } = false;

        /// <summary>
        /// /// Gets or sets the number of verification attempts made by the user.
        /// </summary>
        public int VerificationAttempts { get; set; } = 0;
    }
}
