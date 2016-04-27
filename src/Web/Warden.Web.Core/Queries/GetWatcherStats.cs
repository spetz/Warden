using System;
using Warden.Web.Core.Domain;

namespace Warden.Web.Core.Queries
{
    public class GetWatcherStats : PagedQueryBase
    {
        public Guid OrganizationId { get; set; }
        public Guid WardenId { get; set; }
        public string WatcherName { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
    }
}