using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Authentication.Cookies;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Http.Authentication;
using Microsoft.AspNet.Mvc;
using Warden.Web.Core.Services;
using Warden.Web.Extensions;
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
        [AllowAnonymous]
        [Route("login")]
        [ImportModelStateFromTempData]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ExportModelStateToTempData]
        [AllowAnonymous]
        [Route("login")]
        public async Task<IActionResult> Login(LoginViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                Notify(FlashNotificationType.Error, "Invalid credentials.");

                return RedirectToAction("Login");
            }

            var ipAddress = Request.HttpContext?.Connection?.RemoteIpAddress?.ToString();
            var userAgent = Request.Headers["User-Agent"];

            return await _userService.LoginAsync(viewModel.Email,
                viewModel.Password, ipAddress, userAgent)
                .ExecuteAsync(
                    onSuccess: async () =>
                    {
                        var user = await _userService.GetAsync(viewModel.Email);
                        var claims = new[]
                        {new Claim(ClaimTypes.Name, user.Id.ToString("N")), new Claim(ClaimTypes.NameIdentifier, ""),};
                        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                        await HttpContext.Authentication.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                            new ClaimsPrincipal(identity), new AuthenticationProperties
                            {
                                IsPersistent = viewModel.RememberMe
                            });

                        return RedirectToAction("Index", "Home");
                    },
                    onFailure: ex =>
                    {
                        Notify(FlashNotificationType.Error, "Invalid credentials.");

                        return RedirectToAction("Login");
                    });
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("register")]
        [ImportModelStateFromTempData]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [ExportModelStateToTempData]
        [Route("register")]
        public async Task<IActionResult> Register(RegisterViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                Notify(FlashNotificationType.Error, "Invalid credentials.");

                return RedirectToAction("Register");
            }

            return await _userService.RegisterAsync(viewModel.Email, viewModel.Password)
                .ExecuteAsync(
                    onSuccess: async () =>
                    {
                        Notify(FlashNotificationType.Success, "Your account has been created.");
                        try
                        {
                            var user = await _userService.GetAsync(viewModel.Email);
                            await _apiKeyService.CreateAsync(viewModel.Email);
                            await _organizationService.CreateDefaultAsync(user.Id);
                        }
                        catch (Exception exception)
                        {
                            //Not important
                            Logger.Error(exception);
                        }

                        return RedirectToAction("Login");
                    },
                    onFailure: ex =>
                    {
                        Notify(FlashNotificationType.Error, ex.Message);
                        return RedirectToAction("Register");
                    });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("logout")]
        public async Task<IActionResult> Logout()
        {
            Logger.Info($"User '{UserId}' has logged out.");
            await HttpContext.Authentication.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            Notify(FlashNotificationType.Info, "You have been logged out.");

            return RedirectToAction("Index", "Home");
        }
    }
}