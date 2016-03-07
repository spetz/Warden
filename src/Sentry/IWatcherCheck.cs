using System;

namespace Sentry
{
    public interface IWatcherCheck
    {
        string WatcherName { get; }
        Type WatcherType { get; }
    }

    public class WatcherCheck : IWatcherCheck
    {
        public string WatcherName { get; }
        public Type WatcherType { get; }

        protected WatcherCheck(IWatcher watcher)
        {
            if (watcher == null)
                throw new ArgumentNullException(nameof(watcher), "Watcher can not be null.");

            if (string.IsNullOrEmpty(watcher.Name))
                throw new ArgumentException("Watcher name can not be empty.");

            WatcherName = watcher.Name;
            WatcherType = watcher.GetType();
        }

        public static WatcherCheck Create(IWatcher watcher)
            => new WatcherCheck(watcher);
    }
}