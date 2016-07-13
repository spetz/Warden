using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Warden.Web.Core.Domain;
using Warden.Web.Core.Services;
using Warden.Web.Extensions;
using Warden.Web.Framework;
using Warden.Web.Framework.Filters;
using Warden.Web.ViewModels;

namespace Warden.Web.Controllers
{
    [Route("settings")]
    public class SettingsController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IApiKeyService _apiKeyService;

        public SettingsController(IUserService userService, IApiKeyService apiKeyService)
        {
            _userService = userService;
            _apiKeyService = apiKeyService;
        }

        [HttpGet]
        [Route("")]
        public async Task<IActionResult> Index()
        {
            var user = await _userService.GetAsync(UserId);

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ExportModelStateToTempData]
        [Route("api-keys")]
        public async Task<IActionResult> CreateApiKey()
        {
            await _apiKeyService.CreateAsync(UserId).Execute(
                onSuccess: () => Notify(FlashNotificationType.Success, "API key has been created."),
                onFailure: ex => Notify(FlashNotificationType.Error, ex.Message));

            return RedirectToAction("Index");
        }

        [HttpGet]
        [ImportModelStateFromTempData]
        [Route("password")]
        public IActionResult Password()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ExportModelStateToTempData]
        [Route("password")]
        public async Task<IActionResult> Password(ChangePasswordViewModel viewModel)
        {
            if (!ModelState.IsValid)
                return RedirectToAction("Password");

            return await _userService.SetNewPasswordAsync(UserId, viewModel.ActualPassword, viewModel.NewPassword)
                .Execute(
                    onSuccess: () =>
                    {
                        Notify(FlashNotificationType.Success, "Password has been changed.");
                        return RedirectToAction("Index");
                    },
                    onFailure: ex =>
                    {
                        Notify(FlashNotificationType.Error, ex.Message);
                        return RedirectToAction("Password");
                    });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("api-keys/delete")]
        public async Task<IActionResult> DeleteApiKey(string key)
        {
            var apiKey = await _apiKeyService.GetAsync(key);
            if (apiKey?.UserId != UserId)
                return BadRequest("Invalid API key.");

            await _apiKeyService.DeleteAsync(key);

            Notify(FlashNotificationType.Info, "API key has been removed.");
            return RedirectToAction("Index");
        }
    }
}