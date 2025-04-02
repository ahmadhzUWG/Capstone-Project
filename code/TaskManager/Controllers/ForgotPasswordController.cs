using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using TaskManagerWebsite.Models;
using TaskManagerWebsite.Services;
using TaskManagerWebsite.ViewModels;

namespace TaskManagerWebsite.Controllers
{
    /// <summary>
    /// Manages the password recovery process for users who have forgotten their password.
    /// </summary>
    public class ForgotPasswordController : Controller
    {
        private readonly EmailService emailService;
        private readonly UserManager<User> userManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="ForgotPasswordController"/> class.
        /// </summary>
        /// <param name="emailService"></param>
        /// <param name="userManager"></param>
        public ForgotPasswordController(EmailService emailService, UserManager<User> userManager)
        {
            this.emailService = emailService;
            this.userManager = userManager;
        }

        /// <summary>
        ///    Displays the password recovery page.
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Processes the password recovery request.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(ForgotPasswordViewModel model)
        {
            var user = await this.userManager.FindByNameAsync(model.Username);

            if (user != null)
            {
                var token = await this.userManager.GeneratePasswordResetTokenAsync(user);

                var result = await this.userManager.ResetPasswordAsync(user, token, model.Password);

                if (result.Succeeded)
                {
                    TempData["SuccessMessage"] = "Password reset successfully";
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }

                    TempData["ErrorMessage"] = "There was an error resetting your password.";
                    return View(model);
                }

                var subject = "Task Manager - Password Reset";
                var body = "Your password has been changed";
                await this.emailService.SendEmailAsync(user.Email, subject, body);
            }
            else
            {
                ModelState.AddModelError("Username", "There is no user with provided Username");
            }

            return View(model);
        }
    }
}
