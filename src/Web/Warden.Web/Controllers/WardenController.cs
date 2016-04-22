using System;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using System.Linq;
using Warden.Web.Core.Dto;
using Warden.Web.Core.Queries;
using Warden.Web.Core.Services;
using Warden.Web.ViewModels;

namespace Warden.Web.Controllers
{
    [Route("organizations/{organizationId}/wardens")]
    public class WardenController : ControllerBase
    {
        private readonly IOrganizationService _organizationService;
        private readonly IWardenIterationService _wardenIterationService;

        public WardenController(IOrganizationService organizationService,
            IWardenIterationService wardenIterationService)
        {
            _organizationService = organizationService;
            _wardenIterationService = wardenIterationService;
        }

        [HttpGet]
        [Route("{wardenId}")]
        public async Task<IActionResult> Details(Guid organizationId, Guid wardenId)
        {
            var warden = await GetWardenForUserAsync(organizationId, wardenId);
            if (warden == null)
                return HttpNotFound();

            var iterations = await _wardenIterationService.BrowseAsync(new BrowseWardenIterations
            {
                OrganizationId = organizationId,
                WardenName = warden.Name
            });
            var viewModel = new WardenViewModel
            {
                Id = wardenId,
                OrganizationId = organizationId,
                Name = warden.Name,
                Enabled = warden.Enabled,
                CreatedAt = warden.CreatedAt,
                Iterations = iterations
            };

            return View(viewModel);
        }

        [HttpGet]
        [Route("{wardenId}/iterations/{iterationId}")]
        public async Task<IActionResult> Iteration(Guid organizationId, Guid wardenId, Guid iterationId)
        {
            var warden = await GetWardenForUserAsync(organizationId, wardenId);
            if (warden == null)
                return HttpNotFound();
            var iteration = await _wardenIterationService.GetAsync(iterationId);
            if (iteration == null)
                return HttpNotFound();

            return View(iteration);
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