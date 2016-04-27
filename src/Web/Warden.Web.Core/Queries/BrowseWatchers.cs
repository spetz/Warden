using System;
using Warden.Web.Core.Domain;

namespace Warden.Web.Core.Queries
{
    public class BrowseWatchers: PagedQueryBase
    {
        public Guid OrganizationId { get; set; }
        public Guid WardenId { get; set; }
        public WatcherType? WatcherType { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
    }
}