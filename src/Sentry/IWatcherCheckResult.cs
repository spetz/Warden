namespace Sentry
{

    public interface IWatcherCheckResult : IWatcherCheck
    {
        string Description { get; }
    }

    public class WatcherCheckResult : WatcherCheck, IWatcherCheckResult
    {
        public string Description { get; }

        protected WatcherCheckResult(IWatcher watcher, string description) : base(watcher)
        {
            Description = description;
        }

        public static WatcherCheckResult Create(IWatcher watcher, string description = "")
            => new WatcherCheckResult(watcher, description);
    }
}