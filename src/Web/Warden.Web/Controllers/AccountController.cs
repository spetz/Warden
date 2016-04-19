using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Mvc;
using Warden.Web.ViewModels;

namespace Warden.Web.Controllers
{
    public class AccountController : Controller
    {
        [HttpGet]
        [Route("login")]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("login")]
        public IActionResult Login(LoginViewModel viewModel)
        {
            if (!ModelState.IsValid)
                return RedirectToAction("Login");

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        [Route("register")]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("register")]
        public IActionResult Register(RegisterViewModel viewModel)
        {
            if (!ModelState.IsValid)
                return RedirectToAction("Register");

            return RedirectToAction("Login");
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        [Route("logout")]
        public IActionResult Logout()
        {
            return RedirectToAction("Login");
        }
    }
}