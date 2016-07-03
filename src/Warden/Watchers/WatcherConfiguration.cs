using System;

namespace Warden.Watchers
{
    /// <summary>
    /// Internal configuration of the watchers used by Warden.
    /// </summary>
    public class WatcherConfiguration
    {
        private static readonly TimeSpan MinimalInterval = TimeSpan.FromMilliseconds(1);

        /// <summary>
        /// Instance of configured watcher.
        /// </summary>
        public IWatcher Watcher { get; protected set; }

        /// <summary>
        /// Optional hooks specific for the configured watcher.
        /// </summary>
        public WatcherHooksConfiguration Hooks { get; protected set; }

        /// <summary>
        /// Optional interval (5 seconds by default) after which the next check will be invoked.
        /// </summary>
        public TimeSpan Interval { get; protected set; }

        protected WatcherConfiguration(IWatcher watcher)
        {
            if (watcher == null)
                throw new ArgumentNullException(nameof(watcher), "Watcher can not be null.");

            Watcher = watcher;
            Hooks = WatcherHooksConfiguration.Empty;
        }

        public void SetInterval(TimeSpan interval)
        {
            if (interval < MinimalInterval)
                throw new ArgumentException("Interval can not be less than 1 ms.", nameof(interval));

            Interval = interval;
        }

        /// <summary>
        /// Factory method for creating a new instance of fluent builder for the WatcherConfiguration.
        /// </summary>
        /// <returns>Instance of fluent builder for the WatcherConfiguration.</returns>
        public static Builder Create(IWatcher watcher) => new Builder(watcher);

        /// <summary>
        /// Fluent builder for the WatcherConfiguration.
        /// </summary>
        /// <returns>Instance of fluent builder for the WatcherConfiguration.</returns>
        public class Builder
        {
            private readonly WatcherConfiguration _configuration;

            protected internal Builder(IWatcher watcher)
            {
                _configuration = new WatcherConfiguration(watcher);
            }

            /// <summary>
            /// Sets hooks specific for the particular watcher.
            /// </summary>
            /// <param name="hooks">Configuration of the watcher hooks.</param>
            /// <returns>Instance of fluent builder for the WatcherConfiguration.</returns>
            public Builder WithHooks(WatcherHooksConfiguration hooks)
            {
                _configuration.Hooks = hooks;

                return this;
            }

            /// <summary>
            /// Sets custom interval after which the next check will be invoked.
            /// </summary>
            /// <param name="interval">Interval after which the next check will be invoked.</param>
            /// <returns>Instance of fluent builder for the WatcherConfiguration.</returns>
            public Builder WithInterval(TimeSpan interval)
            {
                if (interval < MinimalInterval)
                    throw new ArgumentException("Interval can not be less than 1 ms.", nameof(interval));

                _configuration.Interval = interval;

                return this;
            }

            /// <summary>
            /// Builds the WatcherConfiguration and return its instance.
            /// </summary>
            /// <returns>Instance of WatcherConfiguration.</returns>
            public WatcherConfiguration Build() => _configuration;
        }
    }
}