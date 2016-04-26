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
    [Route("api/organizations/{organizationId}/wardens/")]
    public class WardenController : ApiController
    {
        private readonly IWardenService _wardenService;
        private readonly IOrganizationService _organizationService;
        private readonly ISignalRService _signalRService;

        public WardenController(IWardenService wardenService, 
            IOrganizationService organizationService,
            IApiKeyService apiKeyService,
            ISignalRService signalRService)
            : base(apiKeyService)
        {
            _wardenService = wardenService;
            _organizationService = organizationService;
            _signalRService = signalRService;
        }


        [HttpGet]
        [Route("{wardenName}/stats")]
        public async Task<WardenStatsDto> GetStats(Guid organizationId, string wardenName, [FromUri] GetWardenStats query)
        {
            var isAuthorized = await _organizationService.IsUserInOrganizationAsync(organizationId, UserId);
            if (!isAuthorized)
            {
                Response.StatusCode = 401;

                return null;
            }

            var stats = await _wardenService.GetStatsAsync(query);

            return stats;
        }
    }
}
