namespace Warden.Web.Core.Domain
{
    public class WatcherCheckResult
    {
        public string WatcherName { get; protected set; }
        public WatcherType WatcherType { get; protected set; }
        public string Description { get; protected set; }
        public bool IsValid { get; protected set; }
    }
}