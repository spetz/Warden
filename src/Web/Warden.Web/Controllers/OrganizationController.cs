using System;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using Warden.Web.Core.Domain;
using Warden.Web.Core.Dto;
using Warden.Web.Core.Queries;
using Warden.Web.Core.Services;
using Warden.Web.Extensions;
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

            return await _organizationService.CreateAsync(viewModel.Name, UserId)
                .ExecuteAsync(
                    onSuccess: async () =>
                    {
                        var organization = await _organizationService.GetAsync(viewModel.Name, UserId);
                        Notify(FlashNotificationType.Success, "Organization has been created.");
                        return RedirectToAction("Details", new {id = organization.Id});
                    },
                    onFailure: ex =>
                    {
                        Notify(FlashNotificationType.Error, ex.Message);
                        return RedirectToAction("Create");
                    });
        }

        [HttpGet]
        [Route("{id}/edit")]
        [ImportModelStateFromTempData]
        public async Task<IActionResult> Edit(Guid id)
        {
            if (!await IsOrganizationOwnerAsync(id))
                return new HttpUnauthorizedResult();

            var organization = await _organizationService.GetAsync(id);
            var viewModel = new EditOrganizationViewModel
            {
                Name = organization.Name
            };

            return View(viewModel);
        }

        [HttpPost]
        [Route("{id}/edit")]
        [ExportModelStateToTempData]
        public async Task<IActionResult> Edit(Guid id, EditOrganizationViewModel viewModel)
        {
            if (!await IsOrganizationOwnerAsync(id))
                return new HttpUnauthorizedResult();

            if (!ModelState.IsValid)
                return RedirectToAction("Edit", new {id});

            return await _organizationService.EditAsync(id, viewModel.Name)
                .Execute(
                    onSuccess: () =>
                    {
                        Notify(FlashNotificationType.Success, "Organization has been updated.");
                        return RedirectToAction("Details", new {id});
                    },
                    onFailure: ex =>
                    {
                        Notify(FlashNotificationType.Error, ex.Message);
                        return RedirectToAction("Edit", new {id});
                    });
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

            return await _organizationService.AddUserAsync(id, viewModel.Email)
                .Execute(
                    onSuccess: () =>
                    {
                        Notify(FlashNotificationType.Success, "User has been added to the organization.");
                        return RedirectToAction("Details", new {id = organization.Id});
                    },
                    onFailure: ex =>
                    {
                        Notify(FlashNotificationType.Error, ex.Message);
                        return RedirectToAction("AddUser");
                    });
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

            return await _organizationService.AddWardenAsync(id, viewModel.Name)
                .Execute(
                    onSuccess: () =>
                    {
                        Notify(FlashNotificationType.Success, "Warden has been added to the organization.");
                        return RedirectToAction("Details", new {id = organization.Id});
                    },
                    onFailure: ex =>
                    {
                        Notify(FlashNotificationType.Error, ex.Message);
                        return RedirectToAction("AddWarden");
                    });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("{id}/delete")]
        public async Task<IActionResult> Delete(Guid id)
        {
            if (!await IsOrganizationOwnerAsync(id))
                return new HttpUnauthorizedResult();

            return await _organizationService.DeleteAsync(id)
                .Execute(
                    onSuccess: () =>
                    {
                        Notify(FlashNotificationType.Info, "Organization has been removed.");
                        return RedirectToAction("Index");
                    },
                    onFailure: ex =>
                    {
                        Notify(FlashNotificationType.Error, ex.Message);
                        return RedirectToAction("Index");
                    });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("{id}/users/delete")]
        public async Task<IActionResult> DeleteUser(Guid id, Guid userId)
        {
            if (!await IsOrganizationOwnerAsync(id))
                return new HttpUnauthorizedResult();

            return await _organizationService.DeleteUserAsync(id, userId)
                .Execute(
                    onSuccess: () =>
                    {
                        Notify(FlashNotificationType.Info, "User has been removed from the organization.");
                        return RedirectToAction("Details", new {id});
                    },
                    onFailure: ex =>
                    {
                        Notify(FlashNotificationType.Error, ex.Message);
                        return RedirectToAction("Details", new {id});
                    });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("{id}/auto-register-new-warden/enable")]
        public async Task<IActionResult> EnableAutoRegisterNewWarden(Guid id)
        {
            if (!await IsOrganizationOwnerAsync(id))
                return new HttpUnauthorizedResult();

            return await _organizationService.EnableAutoRegisterNewWardenAsync(id)
                .Execute(
                    onSuccess: () =>
                    {
                        Notify(FlashNotificationType.Info, "Automatic registration of new Wardens has been enabled.");
                        return RedirectToAction("Details", new {id});
                    },
                    onFailure: ex =>
                    {
                        Notify(FlashNotificationType.Error, ex.Message);
                        return RedirectToAction("Details", new {id});
                    });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("{id}/auto-register-new-warden/disable")]
        public async Task<IActionResult> DisableAutoRegisterNewWarden(Guid id)
        {
            if (!await IsOrganizationOwnerAsync(id))
                return new HttpUnauthorizedResult();

            return await _organizationService.DisableAutoRegisterNewWardenAsync(id)
                .Execute(
                    onSuccess: () =>
                    {
                        Notify(FlashNotificationType.Info, "Automatic registration of new Wardens has been disabled.");
                        return RedirectToAction("Details", new {id});
                    },
                    onFailure: ex =>
                    {
                        Notify(FlashNotificationType.Error, ex.Message);
                        return RedirectToAction("Details", new {id});
                    });
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