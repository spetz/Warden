using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Authentication.Cookies;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Mvc;
using Warden.Web.Core.Services;
using Warden.Web.ViewModels;

namespace Warden.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserService _userService;

        public AccountController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        [Route("login")]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("login")]
        public async Task<IActionResult> Login(LoginViewModel viewModel)
        {
            if (!ModelState.IsValid)
                return RedirectToAction("Login");

            try
            {
                await _userService.LoginAsync(viewModel.Email, viewModel.Password);
                var user = await _userService.GetAsync(viewModel.Email);
                var claims = new[] { new Claim(ClaimTypes.Name, user.Id.ToString("N")) };
                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                await HttpContext.Authentication.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(identity));
            }
            catch (Exception ex)
            {
                return RedirectToAction("Login");
            }

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
        [ValidateAntiForgeryToken]
        [Authorize]
        [Route("logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.Authentication.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Login");
        }
    }
}