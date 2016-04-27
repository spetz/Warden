using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using Warden.Web.Core.Services;
using Warden.Web.Framework.Filters;

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
    }
}