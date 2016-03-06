using System;
using System.Collections.Generic;

namespace Sentry.Core
{
    public class SentryConfiguration
    {
        public ICollection<WatcherConfiguration> Watchers { get; protected set; }
        public HooksConfiguration Hooks { get; protected set; }

        protected internal SentryConfiguration()
        {
            Watchers = new List<WatcherConfiguration>();
            Hooks = HooksConfiguration.Empty;
        }

        public static SentryConfiguration Empty => new SentryConfiguration();
        public static Builder Create() => new Builder(Empty);

        public class Builder
        {
            private readonly SentryConfiguration _configuration;

            protected internal Builder(SentryConfiguration configuration)
            {
                _configuration = configuration;
            }

            public Builder AddWatcher(IWatcher watcher, Action<HooksConfiguration.Builder> hooks = null)
            {
                var hooksConfiguration = HooksConfiguration.Empty;
                if (hooks != null)
                {
                    var hooksConfigurationBuilder = new HooksConfiguration.Builder();
                    hooks(hooksConfigurationBuilder);
                    hooksConfiguration = hooksConfigurationBuilder.Build();
                }

                var watcherConfiguration = WatcherConfiguration.Create(watcher)
                    .WithHooks(hooksConfiguration)
                    .Build();

                _configuration.Watchers.Add(watcherConfiguration);

                return this;
            }

            public Builder SetGlobalHooks(Action<HooksConfiguration.Builder> hooks)
            {
                var hooksConfigurationBuilder = new HooksConfiguration.Builder();
                hooks(hooksConfigurationBuilder);
                _configuration.Hooks = hooksConfigurationBuilder.Build();

                return this;
            }

            public SentryConfiguration Build() => _configuration;
        }
    }
}