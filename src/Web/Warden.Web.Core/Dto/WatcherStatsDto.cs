namespace Warden.Web.Core.Dto
{
    public class WatcherStatsDto : WatcherDto
    {
        public double TotalUptime { get; set; }
        public double TotalDowntime { get; set; }
    }
}