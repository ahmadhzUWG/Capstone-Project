using System.Web.Mvc;
using System.Web.Security;
using TaskManagerWebsite.Models;

namespace TaskManagerWebsite.Controllers
{
    public class AccountController : Controller
    {
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
                if (model.Username == "admin" && model.Password == "password123")
                {
                    FormsAuthentication.SetAuthCookie(model.Username, false); // Persistent Login

                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }

                    return RedirectToAction("Index", "Home");
                }

                ModelState.AddModelError("", "Invalid login attempt.");
            }

            return View(model);
        }



    // GET: Account/Logout
    public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Login");
        }
    }
}