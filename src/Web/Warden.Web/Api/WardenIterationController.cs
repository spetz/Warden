using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNetCore.Mvc;
using Warden.Web.Core.Dto;
using Warden.Web.Core.Queries;
using Warden.Web.Core.Services;

namespace Warden.Web.Api
{
    [Route("api/organizations/{organizationId}/wardens/{wardenName}/iterations")]
    public class WardenIterationController : ApiController
    {
        private readonly IWardenService _wardenService;
        private readonly IOrganizationService _organizationService;
        private readonly ISignalRService _signalRService;

        public WardenIterationController(IWardenService wardenService, 
            IOrganizationService organizationService,
            IApiKeyService apiKeyService,
            IUserService userService,
            ISignalRService signalRService)
            : base(apiKeyService, userService)
        {
            _wardenService = wardenService;
            _organizationService = organizationService;
            _signalRService = signalRService;
        }

        [HttpPost]
        public async Task<IActionResult> Create(Guid organizationId, string wardenName, [FromBody] WardenIterationDto iteration)
        {
            var isAuthorized = await _organizationService.IsUserInOrganizationAsync(organizationId, UserId);
            if (!isAuthorized)
                return Unauthorized();

            var createdIteration = await _wardenService.SaveIterationAsync(iteration, organizationId);
            _signalRService.SendIterationCreated(organizationId, createdIteration);

            return new StatusCodeResult(204);
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

            var iterations = await _wardenService.BrowseIterationsAsync(query);

            return iterations.Items;
        }
    }
}
