using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Mvc;
using Warden.Web.Core.Services;
using Warden.Web.ViewModels;

namespace Warden.Web.Controllers
{
    [Authorize]
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
        [Route("")]
        public async Task<IActionResult> Index()
        {
            return View();
        }

        [HttpGet]
        [Route("default")]
        public async Task<IActionResult> Default()
        {
            var user = await _userService.GetAsync(UserId);
            if (user.RecentlyViewedOrganizationId == Guid.Empty)
                return RedirectToAction("Index");

            return RedirectToAction("Details", new { id = user.RecentlyViewedOrganizationId });
        }

        [HttpGet]
        [Route("organizations/{id}")]
        public async Task<IActionResult> Details(Guid id)
        {
            if (id == Guid.Empty)
                return HttpNotFound();
            var isUserInOrganization = await _organizationService.IsUserInOrganizationAsync(id, UserId);
            if(!isUserInOrganization)
                return HttpNotFound();
            var organization = await _organizationService.GetAsync(id);
            if(organization == null)
                return HttpNotFound();
            if(!organization.ApiKeys.Any())
                return HttpNotFound();
            var viewModel = new DashboardViewModel
            {
                OrganizationId = id,
                OrganizationName = organization.Name,
                ApiKey = organization.ApiKeys.First()
            };

            return View(viewModel);
        }
    }
}