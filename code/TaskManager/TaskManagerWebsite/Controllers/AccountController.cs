using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using TaskManagerWebsite.Models; // Assuming this namespace contains your database context and User model

namespace TaskManagerWebsite.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _db = new ApplicationDbContext(); // Your EF DbContext

        // GET: Account/Login
        public ActionResult Login()
        {
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                var user = _db.Users.SingleOrDefault(u => u.Username == model.Username && u.Password == model.Password);

                if (user != null) // Simple password verification (No Hashing)
                {
                    FormsAuthentication.SetAuthCookie(user.Username, false); // Set authentication cookie

                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }

                    return RedirectToAction("Index", "Home");
                }

                ModelState.AddModelError("", "Invalid username or password.");
            }

            return View(model);
        }

        // GET: Account/Logout
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            Session.Clear();
            return RedirectToAction("Login");
        }
    }
}