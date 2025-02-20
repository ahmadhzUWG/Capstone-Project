using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using TaskManagerWebsite.Data;
using TaskManagerWebsite.Models;
using TaskManagerWebsite.ViewModels;

namespace TaskManager.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ApplicationDbContext _context;


        public HomeController(ILogger<HomeController> logger, UserManager<User> userManager, SignInManager<User> signInManager, ApplicationDbContext context)
        {
            this._logger = logger;
            this._context = context;
            this._userManager = userManager;
            this._signInManager = signInManager;
        }

        public async Task<IActionResult> Index()
        {
            string userRole = "Guest";

            if (this._signInManager.IsSignedIn(User))
            {
                var user = await this._userManager.GetUserAsync(User);
                if (user != null && await this._context.Admins.AnyAsync(a => a.UserId == user.Id))
                {
                    userRole = "Admin";
                }
                else
                {
                    userRole = "User";
                }
            }

            ViewBag.UserRole = userRole;
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
