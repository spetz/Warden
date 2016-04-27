using System;

namespace Warden.Web.Core.Dto
{
    public class WatcherStatsDto : WatcherDto
    {
        public double TotalUptime { get; set; }
        public double TotalDowntime { get; set; }
        public double TotalChecks { get; set; }
        public double TotalValidChecks { get; set; }
        public double TotalInvalidChecks { get; set; }
    }
}