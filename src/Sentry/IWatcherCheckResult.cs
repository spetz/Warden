using System;

namespace Sentry
{
    public interface IWatcherCheckResult
    {
        string WatcherName { get; }
        Type WatcherType { get; }
        string Description { get; }
    }

    public class WatcherCheckResult : IWatcherCheckResult
    {
        public string WatcherName { get; }
        public Type WatcherType { get; }
        public string Description { get; }

        protected WatcherCheckResult(IWatcher watcher, string description)
        {
            if(watcher == null)
                throw new ArgumentNullException(nameof(watcher), "Watcher can not be null.");

            if (string.IsNullOrEmpty(watcher.Name))
                throw new ArgumentException("Watcher name can not be empty.");

            WatcherName = watcher.Name;
            Description = description;
            WatcherType = watcher.GetType();
        }

        public static WatcherCheckResult Create(IWatcher watcher, string description = "")
            => new WatcherCheckResult(watcher, description);
    }
}