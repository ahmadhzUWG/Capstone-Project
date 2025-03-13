using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using TaskManagerWebsite.Data;
using TaskManagerWebsite.Models;
using TaskManagerWebsite.ViewModels;

namespace TaskManagerWebsite.Controllers
{

    /// <summary>
    /// Manages user authentication, including login and logout functionality.
    /// </summary>
    /// <seealso cref="Microsoft.AspNetCore.Mvc.Controller" />
    public class LoginController(UserManager<User> userManager, SignInManager<User> signInManager) : Controller
    {
        /// <summary>
        /// The user manager
        /// </summary>
        private readonly UserManager<User> _userManager = userManager;
        /// <summary>
        /// The sign in manager
        /// </summary>
        private readonly SignInManager<User> _signInManager = signInManager;


        /// <summary>
        /// Displays the login page and ensures the user is signed out before rendering.
        /// </summary>
        /// <returns>
        /// The login view.
        /// </returns>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (_signInManager.IsSignedIn(User))
            {
                return RedirectToAction("Index", "Home");
            }
            await _signInManager.SignOutAsync(); 
            
            return View();
        }

        /// <summary>
        /// Processes user login requests.
        /// </summary>
        /// <param name="model">The login credentials submitted by the user.</param>
        /// <param name="returnUrl">An optional return URL to redirect the user after successful login.</param>
        /// <returns>
        /// Redirects to the return URL or the home page upon successful login.
        /// If the login attempt fails, returns the login view with validation errors.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(LoginViewModel model, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByNameAsync(model.UserName);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(
                model.UserName, model.Password, isPersistent: false, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return LocalRedirect(returnUrl);
                }

                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return View(model);
        }

        /// <summary>
        /// Logs the user out and redirects them to the login page.
        /// </summary>
        /// <returns>
        /// Redirects to the login page after signing out.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Login");
        }
    }
}
