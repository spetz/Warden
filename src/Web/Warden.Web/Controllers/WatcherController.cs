using System;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using Warden.Web.Core.Queries;
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
        [Route("{watcherName}")]
        public async Task<IActionResult> Details(Guid organizationId, Guid wardenId, string watcherName,
            int page = 1, int results = 10)
        {
            var hasAccess = await _organizationService.IsUserInOrganizationAsync(organizationId, UserId);
            if (!hasAccess)
                return HttpUnauthorized();

            var stats = await _watcherService.GetStatsAsync(new GetWatcherStats
            {
                OrganizationId = organizationId,
                WardenId = wardenId,
                WatcherName = watcherName
            });

            if (stats == null)
                return HttpNotFound();

            var checks = await _watcherService.BrowseChecksAsync(new BrowseWardenCheckResults
            {
                OrganizationId = organizationId,
                WardenId = wardenId,
                WatcherName = watcherName,
                Page = page,
                Results = results
            });

            var viewModel = new WatcherViewModel
            {
                OrganizationId = organizationId,
                WardenId = wardenId,
                Stats = stats,
                Checks = checks
            };

            return View(viewModel);
        }
    }
}