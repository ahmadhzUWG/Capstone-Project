using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using TaskManagerWebsite.Data;
using TaskManagerWebsite.Models;
using TaskManagerWebsite.ViewModels;

namespace TaskManagerWebsite.Controllers
{
    /// <summary>
    /// Handles general navigation and informational pages within the application.
    /// </summary>
    /// <seealso cref="Microsoft.AspNetCore.Mvc.Controller" />
    public class HomeController : Controller
    {
        /// <summary>
        /// The logger
        /// </summary>
        private readonly ILogger<HomeController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="HomeController" /> class.
        /// </summary>
        /// <param name="logger">The logger instance for logging application events.</param>
        public HomeController(ILogger<HomeController> logger)
        {
            this._logger = logger;
        }

        /// <summary>
        /// Displays the home page of the application.
        /// </summary>
        /// <returns>
        /// The home page view.
        /// </returns>
        public async Task<IActionResult> Index()
        {
            return View();
        }

        /// <summary>
        /// Displays an access denied page when a user lacks the necessary permissions.
        /// </summary>
        /// <returns>
        /// The access denied view.
        /// </returns>
        public IActionResult AccessDenied()
        {
            return View();
        }

        /// <summary>
        /// Handles application errors and displays an error page.
        /// </summary>
        /// <returns>
        /// An error view with the request ID.
        /// </returns>
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
