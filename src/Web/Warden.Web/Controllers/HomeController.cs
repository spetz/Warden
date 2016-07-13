using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Warden.Web.Core.Services;

namespace Warden.Web.Controllers
{
    public class HomeController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IOrganizationService _organizationService;

        public HomeController(IUserService userService, 
            IOrganizationService organizationService)
        {
            _userService = userService;
            _organizationService = organizationService;
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("")]
        public async Task<IActionResult> Index()
        {
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Login", "Account");

            var user = await _userService.GetAsync(UserId);
            if (user.RecentlyViewedOrganizationId == Guid.Empty && user.RecentlyViewedWardenId == Guid.Empty)
                return RedirectToAction("Index", "Organization");

            var organization = await _organizationService.GetAsync(user.RecentlyViewedOrganizationId);
            if (organization == null)
                return RedirectToAction("Index", "Organization");

            return organization.Wardens.Any(x => x.Id == user.RecentlyViewedWardenId)
                ? RedirectToAction("Details", "Dashboard", new
                {
                    organizationId = user.RecentlyViewedOrganizationId,
                    wardenId = user.RecentlyViewedWardenId
                })
                : RedirectToAction("Details", "Organization", new {id = user.RecentlyViewedOrganizationId});
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("about")]
        public IActionResult About()
        {
            return View();
        }
    }
}