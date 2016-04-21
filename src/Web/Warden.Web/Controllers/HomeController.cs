using System;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using Warden.Web.Core.Services;

namespace Warden.Web.Controllers
{
    public class HomeController : ControllerBase
    {
        private readonly IUserService _userService;

        public HomeController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        [Route("")]
        public async Task<IActionResult> Index()
        {
            if (!User.Identity.IsAuthenticated)
                return View();

            var user = await _userService.GetAsync(UserId);
            if (user.RecentlyViewedOrganizationId == Guid.Empty)
                return RedirectToAction("Index", "Organization");

            return RedirectToAction("Details", "Dashboard", new {id = user.RecentlyViewedOrganizationId});
        }

        [HttpGet]
        [Route("about")]
        public IActionResult About()
        {
            return View();
        }
    }
}