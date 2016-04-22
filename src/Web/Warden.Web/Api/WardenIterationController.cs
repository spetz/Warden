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
    [Route("api/wardens/iterations")]
    public class WardenIterationController : ApiController
    {
        private readonly IWardenIterationService _wardenIterationService;

        public WardenIterationController(IWardenIterationService wardenIterationService, IApiKeyService apiKeyService)
            : base(apiKeyService)
        {
            _wardenIterationService = wardenIterationService;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] WardenIterationDto iteration)
        {
            await _wardenIterationService.CreateAsync(iteration, OrganizationId);

            return new HttpStatusCodeResult(204);
        }

        [HttpGet]
        public async Task<IEnumerable<WardenIterationDto>> GetAll([FromUri] BrowseWardenIterations query)
        {
            query.OrganizationId = OrganizationId;
            var pagedResult = await _wardenIterationService.BrowseAsync(query);

            return pagedResult.Items;
        }
    }
}
