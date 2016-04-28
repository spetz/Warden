using System;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using Warden.Web.Core.Domain;
using Warden.Web.Core.Dto;
using Warden.Web.Core.Queries;
using Warden.Web.Core.Services;
using Warden.Web.Framework;
using Warden.Web.Framework.Filters;
using Warden.Web.ViewModels;

namespace Warden.Web.Controllers
{
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
        [ImportModelStateFromTempData]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [Route("create")]
        [ExportModelStateToTempData]
        public async Task<IActionResult> Create(CreateOrganizationViewModel viewModel)
        {
            if (!ModelState.IsValid)
                return RedirectToAction("Create");

            await _organizationService.CreateAsync(viewModel.Name, UserId);
            var organization = await _organizationService.GetAsync(viewModel.Name, UserId);
            Notify(FlashNotificationType.Success, "Organization has been created.");

            return RedirectToAction("Details", new {id = organization.Id});
        }

        [HttpGet]
        [Route("{id}/users")]
        [ImportModelStateFromTempData]
        public async Task<IActionResult> AddUser(Guid id)
        {
            var organization = await GetOrganizationForUserAsync(id);
            if (organization == null)
                return HttpBadRequest($"Invalid organization id: '{id}'.");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ExportModelStateToTempData]
        [Route("{id}/users")]
        public async Task<IActionResult> AddUser(Guid id, AddUserToOrganizationViewModel viewModel)
        {
            if (!ModelState.IsValid)
                return RedirectToAction("AddUser");

            var organization = await GetOrganizationForUserAsync(id);
            if (organization == null)
                return HttpBadRequest($"Invalid organization id: '{id}'.");

            try
            {
                await _organizationService.AddUserAsync(id, viewModel.Email);
                Notify(FlashNotificationType.Success, "User has been added to the organization.");

                return RedirectToAction("Details", new { id });
            }
            catch (ServiceException exception)
            {
                Notify(FlashNotificationType.Error, exception.Message);

                return RedirectToAction("AddUser");
            }
        }

        [HttpGet]
        [Route("{id}/wardens")]
        [ImportModelStateFromTempData]
        public async Task<IActionResult> AddWarden(Guid id)
        {
            var organization = await GetOrganizationForUserAsync(id);
            if (organization == null)
                return HttpBadRequest($"Invalid organization id: '{id}'.");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ExportModelStateToTempData]
        [Route("{id}/wardens")]
        public async Task<IActionResult> AddWarden(Guid id, AddWardenToOrganizationViewModel viewModel)
        {
            if (!ModelState.IsValid)
                return RedirectToAction("AddWarden");

            var organization = await GetOrganizationForUserAsync(id);
            if (organization == null)
                return HttpBadRequest($"Invalid organization id: '{id}'.");

            await _organizationService.AddWardenAsync(id, viewModel.Name);
            Notify(FlashNotificationType.Success, "Warden has been added to the organization.");

            return RedirectToAction("Details", new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("{id}/delete")]
        public async Task<IActionResult> Delete(Guid id)
        {
            if (!await IsOrganizationOwnerAsync(id))
                return new HttpUnauthorizedResult();

            await _organizationService.DeleteAsync(id);
            Notify(FlashNotificationType.Info, "Organization has been removed.");

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("{id}/users/delete")]
        public async Task<IActionResult> DeleteUser(Guid id, Guid userId)
        {
            if (!await IsOrganizationOwnerAsync(id))
                return new HttpUnauthorizedResult();

            await _organizationService.DeleteUserAsync(id, userId);
            Notify(FlashNotificationType.Info, "User has been removed from organization.");

            return RedirectToAction("Details", new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("{id}/wardens/delete")]
        public async Task<IActionResult> DeleteWarden(Guid id, Guid wardenId)
        {
            if (!await IsOrganizationOwnerAsync(id))
                return new HttpUnauthorizedResult();

            await _organizationService.DeleteWardenAsync(id, wardenId);
            Notify(FlashNotificationType.Info, "Warden has been removed from organization.");

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

        private async Task<bool> IsOrganizationOwnerAsync(Guid id)
        {
            var organization = await GetOrganizationForUserAsync(id);
            if (organization == null)
                return false;

            return organization.OwnerId == UserId;
        }
    }
}