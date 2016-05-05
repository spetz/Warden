using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using Warden.Web.Core.Dto;
using Warden.Web.Core.Queries;
using Warden.Web.Core.Services;

namespace Warden.Web.Api
{
    [Route("api/organizations")]
    public class OrganizationController : ApiController
    {
        private readonly IOrganizationService _organizationService;

        public OrganizationController(IOrganizationService organizationService,
            IApiKeyService apiKeyService,
            IUserService userService)
            : base(apiKeyService, userService)
        {
            _organizationService = organizationService;
        }

        [HttpGet]
        [Route("")]
        public async Task<IEnumerable<OrganizationDto>> GetAll()
        {
            var organizations = await _organizationService.BrowseAsync(new BrowseOrganizations
            {
                UserId = UserId
            });

            return organizations.Items;
        }
    }
}