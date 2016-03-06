using System;

namespace Sentry
{
    public interface IWatcherOutcome
    {
        string Name { get; }
        string Description { get; }
    }

    public class WatcherOutcome : IWatcherOutcome
    {
        public string Name { get; }
        public string Description { get; }

        protected WatcherOutcome(string name, string description)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Watcher name can not be empty.");

            Name = name;
            Description = description;
        }

        public static WatcherOutcome Create(string watcherName, string description = "")
            => new WatcherOutcome(watcherName, description);
    }
}