using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManagerData.Models;

namespace TaskManagerDesktop
{

    /// <summary>
    /// Represents the session state of the application.
    /// </summary>
    public static class Session
    {

        /// <summary>
        /// Gets or sets the current user of the application.
        /// </summary>
        public static User? CurrentUser { get; set; }
    }
}
