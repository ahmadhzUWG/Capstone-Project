using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TaskManagerData.Models;
using TaskManagerData.Services;

namespace TaskManagerWebsite.Controllers
{
    /// <summary>
    /// Manages the process of finding a username by email.
    /// </summary>
    public class FindUsernameController : Controller
    {
        private readonly EmailService emailService;
        private readonly UserManager<User> userManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="FindUsernameController"/> class.
        /// </summary>
        /// <param name="emailService"></param>
        /// <param name="userManager"></param>
        public FindUsernameController(EmailService emailService, UserManager<User> userManager)
        {
            this.emailService = emailService;
            this.userManager = userManager;
        }

        /// <summary>
        /// Displays the find username page.
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Processes the find username request.
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(string email)
        {
            if (ModelState.IsValid)
            {
                var user = this.userManager.Users.FirstOrDefault(u => u.Email == email);
                if (user != null)
                {
                    var subject = "Task Manager - Username Recovery";
                    var body = "Your username is: " + user.UserName;

                    await this.emailService.SendEmailAsync(email, subject, body);
                }

                TempData["SuccessMessage"] = "A email was sent with the username if there was a username associated with the given email";
            }

            return View();
        }
    }
}
