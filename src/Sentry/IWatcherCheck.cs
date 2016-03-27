using System;

namespace Sentry
{
    /// <summary>
    /// Contains the specified watcher name and it's type. 
    /// Used only by the OnStart() and OnStartAsync() hooks which allows to identify which watcher is about to perform the check.
    /// </summary>
    public interface IWatcherCheck
    {
        /// <summary>
        /// Name of the watcher that performed the check.
        /// </summary>
        string WatcherName { get; }

        /// <summary>
        /// Type of the watcher that performed the check.
        /// </summary>
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

        /// <summary>
        /// Factory method for creating a new instance of the IWatcherCheck.
        /// </summary>
        /// <param name="watcher"></param>
        /// <returns></returns>
        public static IWatcherCheck Create(IWatcher watcher)
            => new WatcherCheck(watcher);
    }
}