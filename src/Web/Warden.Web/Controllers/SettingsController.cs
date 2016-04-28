using System;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using Warden.Web.Core.Domain;
using Warden.Web.Core.Services;
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
            await _apiKeyService.CreateAsync(UserId);

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

            try
            {
                await _userService.SetNewPasswordAsync(UserId, viewModel.ActualPassword, viewModel.NewPassword);
                Notify(FlashNotificationType.Success, "Password has been changed.");

                return RedirectToAction("Index");

            }
            catch (ServiceException exception)
            {
                Notify(FlashNotificationType.Error, exception.Message);

                return RedirectToAction("Password");
            }
        }

        [HttpPost]
        [Route("api-keys/delete")]
        public async Task<IActionResult> DeleteApiKey(string key)
        {
            var apiKey = await _apiKeyService.GetAsync(key);
            if (apiKey?.UserId != UserId)
                return HttpBadRequest("Invalid API key.");

            await _apiKeyService.DeleteAsync(key);

            return RedirectToAction("Index");
        }
    }
}