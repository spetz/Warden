using System;
using Newtonsoft.Json;
using Warden.Web.Core.Domain;

namespace Warden.Web.Core.Queries
{
    public class BrowseWardenCheckResults : PagedQueryBase
    {
        public Guid OrganizationId { get; set; }
        public string WardenName { get; set; }
        public Guid WardenId { get; set; }
        public string WatcherName { get; set; }
        public WatcherType? WatcherType { get; set; }
    }
}