using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNet.Mvc;
using Warden.Web.Core.Dto;
using Warden.Web.Core.Queries;
using Warden.Web.Core.Services;

namespace Warden.Web.Api
{
    [Route("api/organizations/{organizationId}/wardens/{wardenName}/iterations")]
    public class WardenIterationController : ApiController
    {
        private readonly IWardenIterationService _wardenIterationService;
        private readonly IOrganizationService _organizationService;

        public WardenIterationController(IWardenIterationService wardenIterationService, 
            IOrganizationService organizationService,
            IApiKeyService apiKeyService)
            : base(apiKeyService)
        {
            _wardenIterationService = wardenIterationService;
            _organizationService = organizationService;
        }

        [HttpPost]
        public async Task<IActionResult> Create(Guid organizationId, string wardenName, [FromBody] WardenIterationDto iteration)
        {
            var isAuthorized = await _organizationService.IsUserInOrganizationAsync(organizationId, UserId);
            if (!isAuthorized)
                return HttpUnauthorized();

            await _wardenIterationService.CreateAsync(iteration, organizationId);

            return new HttpStatusCodeResult(204);
        }

        [HttpGet]
        public async Task<IEnumerable<WardenIterationDto>> GetAll(Guid organizationId, string wardenName, [FromUri] BrowseWardenIterations query)
        {
            var isAuthorized = await _organizationService.IsUserInOrganizationAsync(organizationId, UserId);
            if (!isAuthorized)
            {
                Response.StatusCode = 401;

                return null;
            }

            var iterations = await _wardenIterationService.BrowseAsync(query);

            return iterations.Items;
        }
    }
}
