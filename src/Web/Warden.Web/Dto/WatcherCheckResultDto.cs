using System.Linq;

namespace Warden.Web.Dto
{
    public class WatcherCheckResultDto
    {
        public string WatcherName { get; set; }
        public string WatcherType { get; set; }

        public string WatcherTypeName
            => string.IsNullOrWhiteSpace(WatcherType)
                ? string.Empty
                : WatcherType.Split(',').FirstOrDefault()?.Split('.').LastOrDefault();

        public string Description { get; set; }
        public bool IsValid { get; set; }
    }
}