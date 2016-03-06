using System;

namespace Sentry.Core
{
    public class WatcherConfiguration
    {
        public IWatcher Watcher { get; protected set; }
        public HooksConfiguration Hooks { get; protected set; }

        public static Builder Configure(IWatcher watcher) => new Builder(watcher);

        protected WatcherConfiguration(IWatcher watcher)
        {
            if (watcher == null)
                throw new ArgumentNullException(nameof(watcher));
            Watcher = watcher;
            Hooks = HooksConfiguration.Empty;
        }

        public class Builder
        {
            private readonly WatcherConfiguration _configuration;

            protected internal Builder(IWatcher watcher)
            {
                _configuration = new WatcherConfiguration(watcher);
            }

            public Builder WithHooks(HooksConfiguration hooks)
            {
                _configuration.Hooks = hooks;

                return this;
            }

            public WatcherConfiguration Build()
            {
                return _configuration;
            }
        }
    }
}