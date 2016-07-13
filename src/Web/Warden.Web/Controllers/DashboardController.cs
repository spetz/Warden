using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Warden.Web.Core.Services;
using Warden.Web.ViewModels;

namespace Warden.Web.Controllers
{
    [Route("dashboards")]
    public class DashboardController : ControllerBase
    {
        private readonly IOrganizationService _organizationService;
        private readonly IUserService _userService;

        public DashboardController(IOrganizationService organizationService,
            IUserService userService)
        {
            _organizationService = organizationService;
            _userService = userService;
        }

        [HttpGet]
        [Route("unavailable")]
        public IActionResult Unavailable()
        {
            return View();
        }

        [HttpGet]
        [Route("default")]
        public async Task<IActionResult> Default()
        {
            var user = await _userService.GetAsync(UserId);
            if (user.RecentlyViewedOrganizationId == Guid.Empty && user.RecentlyViewedWardenId == Guid.Empty)
                return RedirectToAction("Unavailable");

            var organization = await _organizationService.GetAsync(user.RecentlyViewedOrganizationId);
            if (organization == null)
                return RedirectToAction("Unavailable");

            if(!organization.Wardens.Any())
                return RedirectToAction("Unavailable");

            var wardenId = organization.Wardens.FirstOrDefault(x => x.Id == user.RecentlyViewedWardenId)?.Id ??
                           organization.Wardens.First().Id;

            return RedirectToAction("Details", new
            {
                organizationId = user.RecentlyViewedOrganizationId, wardenId
            });
        }

        [HttpGet]
        [Route("organizations/{organizationId}/wardens/{wardenId}")]
        public async Task<IActionResult> Details(Guid organizationId, Guid wardenId)
        {
            if (organizationId == Guid.Empty)
                return NotFound();
            var isUserInOrganization = await _organizationService.IsUserInOrganizationAsync(organizationId, UserId);
            if (!isUserInOrganization)
                return NotFound();
            var organization = await _organizationService.GetAsync(organizationId);
            if (organization == null)
                return NotFound();
            var warden = organization.Wardens.FirstOrDefault(x => x.Id == wardenId);
            if (warden == null)
                return NotFound();

            var user = await _userService.GetAsync(UserId);
            if(!user.ApiKeys.Any())
                return NotFound();

            await _userService.SetRecentlyViewedWardenInOrganizationAsync(UserId, organizationId, wardenId);
            var viewModel = new DashboardViewModel
            {
                OrganizationId = organizationId,
                OrganizationName = organization.Name,
                WardenId = warden.Id,
                WardenName = warden.Name,
                TotalWatchers = warden.Watchers.Count(),
                ApiKey = user.ApiKeys.First()
            };

            return View(viewModel);
        }
    }
}