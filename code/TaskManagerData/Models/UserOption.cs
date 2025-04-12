using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskManagerData.Models
{
    /// <summary>
    /// Represents a user option for displaying in the desktop combobox UI component.
    /// </summary>
    public class UserOption
    {
        /// <summary>
        /// Gets or sets the user associated with this option.
        /// </summary>
        public User? User { get; set; }

        /// <summary>
        /// Gets or sets the ID of the user.
        /// </summary>
        public string DisplayName => User?.UserName ?? "N/A";

        /// <summary>
        /// Initializes a new instance of the <see cref="UserOption"/> class.
        /// </summary>
        /// <returns>The display name of the user</returns>
        public override string ToString() => DisplayName; 
    }
}
