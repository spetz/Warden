using System;

namespace Warden.Web.ViewModels
{
    public class DashboardViewModel
    {
        public Guid OrganizationId { get; set; }
        public string OrganizationName { get; set; }
        public Guid WardenId { get; set; }
        public string WardenName { get; set; }
        public string ApiKey { get; set; }
        public int TotalWatchers { get; set; }
    }
}