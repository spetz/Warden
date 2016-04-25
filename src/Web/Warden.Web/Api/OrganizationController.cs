using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using Warden.Web.Core.Dto;
using Warden.Web.Core.Services;

namespace Warden.Web.Api
{
    [Route("api/organizations")]
    public class OrganizationController : ApiController
    {
        private readonly IOrganizationService _organizationService;

        public OrganizationController(IOrganizationService organizationService, IApiKeyService apiKeyService)
            : base(apiKeyService)
        {
            _organizationService = organizationService;
        }

        [HttpGet]
        [Route("current")]
        public async Task<OrganizationDto> Get()
        {
            var organization = await _organizationService.GetAsync(UserId);
            if (organization != null) return organization;
            Response.StatusCode = 404;

            return null;
        }
    }
}