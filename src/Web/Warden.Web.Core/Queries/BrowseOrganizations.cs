using System;
using Newtonsoft.Json;
using Warden.Web.Core.Domain;

namespace Warden.Web.Core.Queries
{
    public class BrowseOrganizations : PagedQueryBase
    {
        [JsonIgnore]
        public Guid UserId { get; set; }

        [JsonIgnore]
        public Guid OwnerId { get; set; }
    }
}