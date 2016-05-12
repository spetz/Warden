using System.Threading.Tasks;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Mvc;
using Warden.Web.Core.Services;
using Warden.Web.Extensions;
using Warden.Web.Framework;
using Warden.Web.Framework.Filters;
using Warden.Web.ViewModels;

namespace Warden.Web.Controllers
{
    [AllowAnonymous]
    [Route("reset-password")]
    public class ResetPasswordController : ControllerBase
    {
        private readonly IUserService _userService;
        private const string ResetPasswordAccessKey = "reset-password";
        private const string SetNewPasswordAccessKey = "set-new-password";

        public ResetPasswordController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        [ImportModelStateFromTempData]
        [Route("")]
        public IActionResult Initiate()
        {
            if (User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ExportModelStateToTempData]
        [Route("")]
        public async Task<IActionResult> Initiate(ResetPasswordViewModel viewModel)
        {
            if (!ModelState.IsValid)
                return RedirectToAction("Initiate");

            var ipAddress = Request.HttpContext?.Connection?.RemoteIpAddress?.ToString();
            var userAgent = Request.Headers["User-Agent"];

            return await _userService.InitiatePasswordResetAsync(viewModel.Email,
                ipAddress, userAgent)
                .Execute(
                    onSuccess: () =>
                    {
                        SetAccess(ResetPasswordAccessKey);

                        return RedirectToAction("Status");
                    },
                    onFailure: ex =>
                    {
                        Notify(FlashNotificationType.Error, ex.Message);

                        return RedirectToAction("Initiate");
                    });
        }

        [HttpGet]
        [Route("status")]
        public IActionResult Status()
        {
            var hasAccess = HasAccess(ResetPasswordAccessKey);
            if (!hasAccess)
                return RedirectToAction("Initiate");

            SetAccess(ResetPasswordAccessKey);

            return View();
        }

        [HttpGet]
        [Route("set-new")]
        public async Task<IActionResult> SetNew(string email, string token)
        {
            return await _userService.ValidatePasswordResetTokenAsync(email, token)
                .Execute(
                    onSuccess: () =>
                    {
                        SetAccess(SetNewPasswordAccessKey);
                        var viewModel = new SetNewPasswordViewModel
                        {
                            Email = email,
                            Token = token
                        };

                        return View(viewModel);
                    },
                    onFailure: ex =>
                    {
                        Notify(FlashNotificationType.Error, ex.Message);

                        return RedirectToAction("Initiate");
                    });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ExportModelStateToTempData]
        [Route("set-new")]
        public async Task<IActionResult> SetNew(SetNewPasswordViewModel viewModel)
        {
            if (!HasAccess(SetNewPasswordAccessKey))
                return RedirectToAction("Initiate");
            if (!ModelState.IsValid)
                return RedirectToAction("SetNew");

            var ipAddress = Request.HttpContext?.Connection?.RemoteIpAddress?.ToString();
            var userAgent = Request.Headers["User-Agent"];

            return await _userService.CompletePasswordResetAsync(viewModel.Email,
                viewModel.Token, viewModel.Password, ipAddress, userAgent)
                .Execute(
                    onSuccess: () =>
                    {
                        SetAccess(ResetPasswordAccessKey);

                        return RedirectToAction("Complete");
                    },
                    onFailure: ex =>
                    {
                        Notify(FlashNotificationType.Error, ex.Message);

                        return RedirectToAction("SetNew", new {email = viewModel.Email, token = viewModel.Token});
                    });
        }

        [HttpGet]
        [Route("complete")]
        public IActionResult Complete()
        {
            var hasAccess = HasAccess(SetNewPasswordAccessKey);
            if (!hasAccess)
                return RedirectToAction("Initiate");

            SetAccess(SetNewPasswordAccessKey);

            return View();
        }

        private void SetAccess(string key)
        {
            TempData[key] = key;
        }

        private bool HasAccess(string key) => TempData[key] as string == key;
    }
}