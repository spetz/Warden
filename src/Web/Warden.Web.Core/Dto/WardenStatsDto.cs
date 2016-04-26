using System;
using System.Collections.Generic;

namespace Warden.Web.Core.Dto
{
    public class WardenStatsDto
    {
        public Guid OrganizationId { get; set; }
        public string WardenName { get; set; }
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public double TotalUptime { get; set; }
        public double TotalDowntime { get; set; }
        public IEnumerable<WatcherStatsDto> Watchers { get; set; }
    }
}