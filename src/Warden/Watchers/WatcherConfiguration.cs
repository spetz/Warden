using System;

namespace Warden.Watchers
{
    /// <summary>
    /// Internal configuration of the watchers used by Warden.
    /// </summary>
    public class WatcherConfiguration
    {
        /// <summary>
        /// Instance of configured watcher.
        /// </summary>
        public IWatcher Watcher { get; protected set; }

        /// <summary>
        /// Optional hooks specific for the configured watcher.
        /// </summary>
        public WatcherHooksConfiguration Hooks { get; protected set; }

        protected WatcherConfiguration(IWatcher watcher)
        {
            if (watcher == null)
                throw new ArgumentNullException(nameof(watcher), "Watcher can not be null.");

            Watcher = watcher;
            Hooks = WatcherHooksConfiguration.Empty;
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
            /// <param name="hooks">Lambda expression for configuring the watcher hooks.</param>
            /// <returns>Instance of fluent builder for the WatcherConfiguration.</returns>
            public Builder WithHooks(WatcherHooksConfiguration hooks)
            {
                _configuration.Hooks = hooks;

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