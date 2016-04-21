using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Mvc;
using Warden.Web.Core.Services;

namespace Warden.Web.Controllers
{
    [Authorize]
    [Route("organizations")]
    public class OrganizationController : ControllerBase
    {
        private readonly IOrganizationService _organizationService;
        private readonly IUserService _userService;

        public OrganizationController(IOrganizationService organizationService, 
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
        [Route("{id}")]
        public async Task<IActionResult> Details(Guid id)
        {
            if (id == Guid.Empty)
                return HttpNotFound();
            var isUserInOrganization = await _organizationService.IsUserInOrganizationAsync(id, UserId);
            if (!isUserInOrganization)
                return HttpNotFound();
            var organization = await _organizationService.GetAsync(id);
            if (organization == null)
                return HttpNotFound();

            await _userService.SetRecentlyViewedOrganization(UserId, id);

            return View(organization);
        }

        [HttpGet]
        [Route("create")]
        public async Task<IActionResult> Create()
        {
            return View();
        }
    }
}