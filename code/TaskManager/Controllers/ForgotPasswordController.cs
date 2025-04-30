using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using TaskManagerData.Models;
using TaskManagerData.Services;
using TaskManagerWebsite.ViewModels;

namespace TaskManagerWebsite.Controllers
{
    /// <summary>
    /// Manages the password recovery process for users who have forgotten their password.
    /// </summary>
    public class ForgotPasswordController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly EmailService emailService;
        private readonly UserManager<User> userManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="ForgotPasswordController"/> class.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="emailService"></param>
        /// <param name="userManager"></param>
        public ForgotPasswordController(ApplicationDbContext context, EmailService emailService, UserManager<User> userManager)
        {
            this.context = context;
            this.emailService = emailService;
            this.userManager = userManager;
        }

        /// <summary>
        ///   Verifies the one-time code sent to the user for password recovery.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyOneTimeCode(ForgotPasswordViewModel model)
        {
            ModelState.Remove(nameof(model.Username));
            ModelState.Remove(nameof(model.Password));
            ModelState.Remove(nameof(model.ConfirmPassword));
            ModelState.Remove(nameof(model.VerificationAttempts));

            if (ModelState.IsValid)
            {
                var enteredCode = model.OneTimeCode.Trim();
                var user = this.userManager.Users.FirstOrDefault(u => u.UserName == model.Username);
                if (user != null)
                {
                    var email = user.Email;

                    var codeMatches = await this.context.PasswordResets
                        .AnyAsync(p => p.Email == email && p.Username == model.Username && p.Code == enteredCode);

                    if (codeMatches)
                    {
                        TempData["SuccessMessage"] = "Successfully verified One-Time Code";
                        model.VerifiedOneTime = true;
                    }
                    else
                    {
                        model.VerificationAttempts++;
                        ModelState.AddModelError("OneTimeCode", $"The One-Time Code is incorrect. Attempts left: {3 - model.VerificationAttempts}");
                        if (model.VerificationAttempts >= 3)
                        {
                            TempData["FailedAttempts"] = "You have exceeded the maximum number of attempts. Please try again.";
                            return RedirectToAction("Index", "ForgotPassword");
                        }
                        return View("Index", model);
                    }
                }
                else
                {
                    ModelState.AddModelError("OneTimeCode", "User not found.");
                    return View("Index", model);
                }
            }
            else
            {
                ModelState.AddModelError("OneTimeCode", "The One-Time Code is incorrect. Please try again.");
            }

            return View("Index", model);
        }

        /// <summary>
        ///   Sends a one-time code to the user for password recovery.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendOneTimeCode(ForgotPasswordViewModel model)
        {
            ModelState.Remove(nameof(model.Password));
            ModelState.Remove(nameof(model.ConfirmPassword));
            ModelState.Remove(nameof(model.OneTimeCode));
            ModelState.Remove(nameof(model.VerificationAttempts));

            if (ModelState.IsValid)
            {
                var user = this.userManager.Users.FirstOrDefault(u => u.UserName == model.Username);
                if (user != null)
                {
                    var email = user.Email;
                    var code = new Random().Next(100000, 1000000);
                    var subject = "Task Manager - One-Time Code";
                    var body = "Your One-Time Code is: " + code;

                    await this.emailService.SendEmailAsync(email, subject, body);

                    var previousPasswordReset = await this.context.PasswordResets
                        .FirstOrDefaultAsync(p => p.Email == user.Email && p.Username == model.Username);
                    if (previousPasswordReset != null)
                    {
                        this.context.PasswordResets.Remove(previousPasswordReset);
                        await this.context.SaveChangesAsync();
                    }
                    
                    var passwordReset = new PasswordReset
                    {
                        Code = code.ToString(),
                        Email = email,
                        Username = model.Username
                    };

                    this.context.PasswordResets.Add(passwordReset);
                    await this.context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Successfully sent One-Time Code";
                    model.SentOneTime = true;
                }
                else
                {
                    ModelState.AddModelError("Username", "There is no user with provided Username");
                }
                
            }

            return View("Index", model);
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
            ModelState.Remove(nameof(model.Username));
            ModelState.Remove(nameof(model.Password));
            ModelState.Remove(nameof(model.ConfirmPassword));
            ModelState.Remove(nameof(model.OneTimeCode));
            ModelState.Remove(nameof(model.VerificationAttempts));
            ModelState.Remove(nameof(model.SentOneTime));
            ModelState.Remove(nameof(model.VerifiedOneTime));
            

            model.VerifiedOneTime = true;
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

                    return View(model);
                }

                var subject = "Task Manager - Password Reset";
                var body = "Your password has been changed";
                await this.emailService.SendEmailAsync(user.Email, subject, body);

                var passwordReset = await this.context.PasswordResets
                    .FirstOrDefaultAsync(p => p.Email == user.Email && p.Username == model.Username);
                this.context.PasswordResets.Remove(passwordReset);
                await this.context.SaveChangesAsync();
            }

            return RedirectToAction("Index", "Login");
        }
    }
}
