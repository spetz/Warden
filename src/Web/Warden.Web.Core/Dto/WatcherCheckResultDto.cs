using Warden.Web.Core.Domain;

namespace Warden.Web.Core.Dto
{
    public class WatcherCheckResultDto
    {
        public string WatcherName { get; set; }
        public string WatcherGroup { get; set; }
        public string WatcherType { get; set; }
        public string Description { get; set; }
        public bool IsValid { get; set; }

        public WatcherCheckResultDto()
        {
        }

        public WatcherCheckResultDto(WatcherCheckResult result)
        {
            WatcherName = result.Watcher.Name;
            WatcherType = result.Watcher.Type.ToString().ToLowerInvariant();
            WatcherGroup = result.Watcher.Group;
            Description = result.Description;
            IsValid = result.IsValid;
        }
    }
}