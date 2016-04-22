using System;
using System.Threading.Tasks;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Mvc;
using Warden.Web.Core.Dto;
using Warden.Web.Core.Queries;
using Warden.Web.Core.Services;
using Warden.Web.ViewModels;

namespace Warden.Web.Controllers
{
    [Authorize]
    [Route("organizations")]
    public class OrganizationController : ControllerBase
    {
        private readonly IOrganizationService _organizationService;
        private readonly IUserService _userService;
        private readonly IApiKeyService _apiKeyService;

        public OrganizationController(IOrganizationService organizationService, 
            IUserService userService, IApiKeyService apiKeyService)
        {
            _organizationService = organizationService;
            _userService = userService;
            _apiKeyService = apiKeyService;
        }

        [HttpGet]
        [Route("")]
        public async Task<IActionResult> Index()
        {
            var organizations = await _organizationService.BrowseAsync(new BrowseOrganizations
            {
                UserId = UserId
            });

            return View(organizations);
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> Details(Guid id)
        {
            var organization = await GetOrganizationForUserAsync(id);
            if (organization == null)
                return HttpNotFound();

            return View(organization);
        }

        [HttpGet]
        [Route("create")]
        public async Task<IActionResult> Create()
        {
            return View();
        }

        [HttpPost]
        [Route("create")]
        public async Task<IActionResult> Create(CreateOrganizationViewModel viewModel)
        {
            if (!ModelState.IsValid)
                return RedirectToAction("Create");

            await _organizationService.CreateAsync(viewModel.Name, UserId);
            var organization = await _organizationService.GetAsync(viewModel.Name, UserId);
            await _apiKeyService.CreateAsync(organization.Id);

            return RedirectToAction("Details", new {id = organization.Id});
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("{id}/api-keys")]
        public async Task<IActionResult> CreateApiKey(Guid id)
        {
            var organization = await GetOrganizationForUserAsync(id);
            if (organization == null)
                return HttpBadRequest($"Invalid organization id: '{id}'.");

            await _apiKeyService.CreateAsync(id);

            return RedirectToAction("Details", new {id});
        }

        [HttpGet]
        [Route("{id}/users")]
        public async Task<IActionResult> AddUser(Guid id)
        {
            var organization = await GetOrganizationForUserAsync(id);
            if (organization == null)
                return HttpBadRequest($"Invalid organization id: '{id}'.");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("{id}/users")]
        public async Task<IActionResult> AddUser(Guid id, AddUserToOrganizationViewModel viewModel)
        {
            if (!ModelState.IsValid)
                return RedirectToAction("AddUser");

            var organization = await GetOrganizationForUserAsync(id);
            if (organization == null)
                return HttpBadRequest($"Invalid organization id: '{id}'.");

            await _organizationService.AddUserAsync(id, viewModel.Email);

            return RedirectToAction("Details", new { id });
        }

        [HttpGet]
        [Route("{id}/wardens")]
        public async Task<IActionResult> AddWarden(Guid id)
        {
            var organization = await GetOrganizationForUserAsync(id);
            if (organization == null)
                return HttpBadRequest($"Invalid organization id: '{id}'.");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("{id}/wardens")]
        public async Task<IActionResult> AddWarden(Guid id, AddWardenToOrganizationViewModel viewModel)
        {
            if (!ModelState.IsValid)
                return RedirectToAction("AddWarden");

            var organization = await GetOrganizationForUserAsync(id);
            if (organization == null)
                return HttpBadRequest($"Invalid organization id: '{id}'.");

            await _organizationService.AddWardenAsync(id, viewModel.Name);

            return RedirectToAction("Details", new { id });
        }

        private async Task<OrganizationDto> GetOrganizationForUserAsync(Guid id)
        {
            if (id == Guid.Empty)
                return null;
            var isUserInOrganization = await _organizationService.IsUserInOrganizationAsync(id, UserId);
            if (!isUserInOrganization)
                return null;
            var organization = await _organizationService.GetAsync(id);

            return organization;
        }
    }
}