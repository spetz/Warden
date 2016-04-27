using System;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using Warden.Web.Core.Services;
using Warden.Web.ViewModels;

namespace Warden.Web.Controllers
{
    [Route("organizations/{organizationId}/wardens/{wardenId}/watchers")]
    public class WatcherController : ControllerBase
    {
        private readonly IWatcherService _watcherService;
        private readonly IOrganizationService _organizationService;

        public WatcherController(IWatcherService watcherService, IOrganizationService organizationService)
        {
            _watcherService = watcherService;
            _organizationService = organizationService;
        }

        [HttpGet]
        [Route("")]
        public async Task<IActionResult> Index(Guid organizationId, Guid wardenId)
        {
            var hasAccess = await _organizationService.IsUserInOrganizationAsync(organizationId, UserId);
            if (!hasAccess)
                return HttpUnauthorized();

            var watchers = await _watcherService.GetAllAsync(organizationId, wardenId);
            var viewModel = new WatchersViewModel
            {
                OrganizationId =  organizationId,
                WardenId = wardenId,
                Watchers = watchers
            };

            return View(viewModel);
        }

        [HttpGet]
        [Route("{watcherName}")]
        public async Task<IActionResult> Details(Guid organizationId, Guid wardenId, string watcherName)
        {
            var hasAccess = await _organizationService.IsUserInOrganizationAsync(organizationId, UserId);
            if (!hasAccess)
                return HttpUnauthorized();

            return View();
        }
    }
}