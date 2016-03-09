namespace Sentry
{

    public interface IWatcherCheckResult : IWatcherCheck
    {
        bool IsValid { get; }
        string Description { get; }
    }

    public class WatcherCheckResult : WatcherCheck, IWatcherCheckResult
    {
        public string Description { get; }
        public bool IsValid { get; }

        protected WatcherCheckResult(IWatcher watcher, bool isValid, string description) : base(watcher)
        {
            Description = description;
            IsValid = isValid;
        }

        public static IWatcherCheckResult Create(IWatcher watcher, bool isValid, string description = "")
            => new WatcherCheckResult(watcher, isValid, description);
    }
}