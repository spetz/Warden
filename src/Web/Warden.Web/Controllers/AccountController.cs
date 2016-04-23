using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Authentication.Cookies;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Mvc;
using Warden.Web.Core.Services;
using Warden.Web.Framework;
using Warden.Web.Framework.Filters;
using Warden.Web.ViewModels;

namespace Warden.Web.Controllers
{
    public class AccountController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IOrganizationService _organizationService;
        private readonly IApiKeyService _apiKeyService;

        public AccountController(IUserService userService,
            IOrganizationService organizationService,
            IApiKeyService apiKeyService)
        {
            _userService = userService;
            _organizationService = organizationService;
            _apiKeyService = apiKeyService;
        }

        [HttpGet]
        [Route("login")]
        [ImportModelStateFromTempData]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ExportModelStateToTempData]
        [Route("login")]
        public async Task<IActionResult> Login(LoginViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                Notify(FlashNotificationType.Error, "Invalid credentials.");

                return RedirectToAction("Login");
            }

            try
            {
                await _userService.LoginAsync(viewModel.Email, viewModel.Password);
                var user = await _userService.GetAsync(viewModel.Email);
                var claims = new[] { new Claim(ClaimTypes.Name, user.Id.ToString("N")), new Claim(ClaimTypes.NameIdentifier, ""),  };
                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                await HttpContext.Authentication.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(identity));
            }
            catch (Exception ex)
            {
                Notify(FlashNotificationType.Error, "Invalid credentials.");
                return RedirectToAction("Login");
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        [Route("register")]
        [ImportModelStateFromTempData]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ExportModelStateToTempData]
        [Route("register")]
        public async Task<IActionResult> Register(RegisterViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                Notify(FlashNotificationType.Error, "Invalid password and/or email.");

                return RedirectToAction("Register");
            }

            try
            {
                await _userService.RegisterAsync(viewModel.Email, viewModel.Password);
            }
            catch (Exception ex)
            {
                Notify(FlashNotificationType.Error, "Invalid password and/or email already in use.");

                return RedirectToAction("Register");
            }
            try
            {
                var user = await _userService.GetAsync(viewModel.Email);
                await _organizationService.CreateDefaultAsync(user.Id);
                var organization = await _organizationService.GetDefaultAsync(user.Id);
                await _apiKeyService.CreateAsync(organization.Id);
            }
            catch (Exception ex)
            {
                //Not important
            }

            return RedirectToAction("Login");
        }

        [HttpDelete]
        [Authorize]
        [Route("logout")]
        public async Task Logout()
        {
            await HttpContext.Authentication.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }
    }
}