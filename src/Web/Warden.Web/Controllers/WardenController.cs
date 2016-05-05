using System;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using System.Linq;
using Warden.Web.Core.Dto;
using Warden.Web.Core.Queries;
using Warden.Web.Core.Services;
using Warden.Web.Extensions;
using Warden.Web.Framework;
using Warden.Web.Framework.Filters;
using Warden.Web.ViewModels;

namespace Warden.Web.Controllers
{
    [Route("organizations/{organizationId}/wardens")]
    public class WardenController : ControllerBase
    {
        private readonly IOrganizationService _organizationService;
        private readonly IWardenService _wardenService;
        private readonly IWatcherService _watcherService;

        public WardenController(IOrganizationService organizationService,
            IWardenService wardenService, IWatcherService watcherService)
        {
            _organizationService = organizationService;
            _wardenService = wardenService;
            _watcherService = watcherService;
        }

        [HttpGet]
        [Route("{wardenId}")]
        public async Task<IActionResult> Details(Guid organizationId, Guid wardenId, 
            int page = 1, int results = 10)
        {
            var warden = await GetWardenForUserAsync(organizationId, wardenId);
            if (warden == null)
                return HttpNotFound();

            var stats = await _wardenService.GetStatsAsync(new GetWardenStats
            {
                OrganizationId = organizationId,
                WardenName = warden.Name
            });
            var watchers = await _watcherService.GetAllAsync(organizationId, wardenId);
            var iterations = await _wardenService.BrowseIterationsAsync(new BrowseWardenIterations
            {
                OrganizationId = organizationId,
                WardenName = warden.Name,
                Page = page,
                Results = results
            });
            var viewModel = new WardenViewModel
            {
                Id = wardenId,
                OrganizationId = organizationId,
                Stats = stats,
                CreatedAt = warden.CreatedAt,
                Iterations = iterations,
                Watchers = watchers
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("{wardenId}/disable")]
        public async Task<IActionResult> Disable(Guid organizationId, Guid wardenId)
        {
            if (!ModelState.IsValid)
                return RedirectToAction("Details", new {organizationId, wardenId});

            var warden = await GetWardenForUserAsync(organizationId, wardenId);
            if (warden == null)
                return HttpNotFound();

            return await _organizationService.DisableWardenAsync(organizationId, warden.Name)
                .Execute(
                    onSuccess: () =>
                    {
                        Notify(FlashNotificationType.Info, "Warden has been disabled.");
                        return RedirectToAction("Details", new {organizationId, wardenId});
                    },
                    onFailure: ex =>
                    {
                        Notify(FlashNotificationType.Error, ex.Message);
                        return RedirectToAction("Details", new {organizationId, wardenId});
                    });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("{wardenId}/enable")]
        public async Task<IActionResult> Enable(Guid organizationId, Guid wardenId)
        {
            if (!ModelState.IsValid)
                return RedirectToAction("Details", new {organizationId, wardenId});

            var warden = await GetWardenForUserAsync(organizationId, wardenId);
            if (warden == null)
                return HttpNotFound();

            return await _organizationService.EnableWardenAsync(organizationId, warden.Name)
                .Execute(
                    onSuccess: () =>
                    {
                        Notify(FlashNotificationType.Success, "Warden has been enabled.");
                        return RedirectToAction("Details", new {organizationId, wardenId});
                    },
                    onFailure: ex =>
                    {
                        Notify(FlashNotificationType.Error, ex.Message);
                        return RedirectToAction("Details", new {organizationId, wardenId});
                    });
        }

        [HttpGet]
        [Route("{wardenId}/iterations/{iterationId}")]
        public async Task<IActionResult> Iteration(Guid organizationId, Guid wardenId, Guid iterationId)
        {
            var warden = await GetWardenForUserAsync(organizationId, wardenId);
            if (warden == null)
                return HttpNotFound();
            var iteration = await _wardenService.GetIterationAsync(iterationId);
            if (iteration == null)
                return HttpNotFound();

            var viewModel = new WardenIterationViewModel
            {
                OrganizationId = organizationId,
                WardenId = wardenId,
                Iteration = iteration
            };

            return View(viewModel);
        }

        [HttpGet]
        [Route("{wardenId}/edit")]
        [ImportModelStateFromTempData]
        public async Task<IActionResult> Edit(Guid organizationId, Guid wardenId)
        {
            var warden = await GetWardenForUserAsync(organizationId, wardenId);
            if (warden == null)
                return HttpNotFound();

            var viewModel = new EditWardenViewModel
            {
                Name = warden.Name
            };

            return View(viewModel);
        }

        [HttpPost]
        [Route("{wardenId}/edit")]
        [ExportModelStateToTempData]
        public async Task<IActionResult> Edit(Guid organizationId, Guid wardenId, EditWardenViewModel viewModel)
        {
            if (!ModelState.IsValid)
                return RedirectToAction("Edit", new {organizationId, wardenId});

            return await _wardenService.EditAsync(organizationId, wardenId, viewModel.Name)
                .Execute(
                    onSuccess: () =>
                    {
                        Notify(FlashNotificationType.Success, "Warden has been updated.");
                        return RedirectToAction("Details", new {organizationId, wardenId});
                    },
                    onFailure: ex =>
                    {
                        Notify(FlashNotificationType.Error, ex.Message);
                        return RedirectToAction("Edit", new {organizationId, wardenId});
                    });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("{wardenId}/delete")]
        public async Task<IActionResult> Delete(Guid organizationId, Guid wardenId)
        {
            return await _organizationService.DeleteWardenAsync(organizationId, wardenId)
                .Execute(
                    onSuccess: () =>
                    {
                        Notify(FlashNotificationType.Info, "Warden has been removed.");
                        return RedirectToAction("Details", "Organization", new {id = organizationId});
                    },
                    onFailure: ex =>
                    {
                        Notify(FlashNotificationType.Error, ex.Message);
                        return RedirectToAction("Details", new {organizationId, wardenId});
                    });
        }

        private async Task<WardenDto> GetWardenForUserAsync(Guid organizationId, Guid wardenId)
        {
            if (organizationId == Guid.Empty)
                return null;
            var isUserInOrganization = await _organizationService.IsUserInOrganizationAsync(organizationId, UserId);
            if (!isUserInOrganization)
                return null;
            var organization = await _organizationService.GetAsync(organizationId);
            var warden = organization?.Wardens.FirstOrDefault(x => x.Id == wardenId);

            return warden;
        }
    }
}