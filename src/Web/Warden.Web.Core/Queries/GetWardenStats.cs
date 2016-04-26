using System;
using Warden.Web.Core.Domain;

namespace Warden.Web.Core.Queries
{
    public class GetWardenStats : PagedQueryBase
    {
        public Guid OrganizationId { get; set; }
        public string WardenName { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
    }
}