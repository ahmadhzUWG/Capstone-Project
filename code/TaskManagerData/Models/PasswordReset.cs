using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskManagerData.Models
{
    /// <summary>
    /// Represents the data model for resetting a password.
    /// </summary>
    public class PasswordReset
    {
        /// <summary>
        /// Gets or sets the unique identifier for the password reset request.
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the one time code sent to the user's email address.
        /// </summary>
        [StringLength(6, MinimumLength = 6)]
        public string Code { get; set; }

        /// <summary>
        /// Gets or sets the email address associated with the password reset request.
        /// </summary>
        [EmailAddress]
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the username associated with the password reset request.
        /// </summary>
        [Required]
        public string Username { get; set; }
    }
}
