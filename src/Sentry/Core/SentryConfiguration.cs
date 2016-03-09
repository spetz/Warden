using System;
using System.Collections.Generic;

namespace Sentry.Core
{
    public class SentryConfiguration
    {
        public ICollection<WatcherConfiguration> Watchers { get; protected set; }
        public SentryHooksConfiguration Hooks { get; protected set; }
        public WatcherHooksConfiguration GlobalWatcherHooks { get; protected set; }
        public TimeSpan IterationDelay { get; protected set; }
        public long? IterationsCount { get; protected set; }
        public Func<DateTime> DateTimeProvider { get; protected set; }

        protected internal SentryConfiguration()
        {
            Hooks = SentryHooksConfiguration.Empty;
            GlobalWatcherHooks = WatcherHooksConfiguration.Empty;
            Watchers = new List<WatcherConfiguration>();
            IterationDelay = new TimeSpan(0, 0, 5);
            DateTimeProvider = () => DateTime.UtcNow;
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

            public Builder AddWatcher(IWatcher watcher, Action<WatcherHooksConfiguration.Builder> hooks = null)
            {
                var hooksConfiguration = WatcherHooksConfiguration.Empty;
                if (hooks != null)
                {
                    var hooksConfigurationBuilder = new WatcherHooksConfiguration.Builder();
                    hooks(hooksConfigurationBuilder);
                    hooksConfiguration = hooksConfigurationBuilder.Build();
                }

                var watcherConfiguration = WatcherConfiguration.Create(watcher)
                    .WithHooks(hooksConfiguration)
                    .Build();

                _configuration.Watchers.Add(watcherConfiguration);

                return this;
            }

            public Builder SetCustomDateTimeProvider(Func<DateTime> dateTimeProvider)
            {
                if(dateTimeProvider == null)
                    throw new ArgumentNullException(nameof(dateTimeProvider), "DateTime provider can not be null.");

                _configuration.DateTimeProvider = dateTimeProvider;

                return this;
            }

            public Builder SetHooks(Action<SentryHooksConfiguration.Builder> hooks)
            {
                var hooksConfigurationBuilder = new SentryHooksConfiguration.Builder();
                hooks(hooksConfigurationBuilder);
                _configuration.Hooks = hooksConfigurationBuilder.Build();

                return this;
            }

            public Builder SetGlobalWatcherHooks(Action<WatcherHooksConfiguration.Builder> hooks)
            {
                var hooksConfigurationBuilder = new WatcherHooksConfiguration.Builder();
                hooks(hooksConfigurationBuilder);
                _configuration.GlobalWatcherHooks = hooksConfigurationBuilder.Build();

                return this;
            }

            public Builder SetIterationDelay(TimeSpan delay)
            {
                _configuration.IterationDelay = delay;

                return this;
            }

            public Builder WithoutIterationDelay()
            {
                _configuration.IterationDelay = TimeSpan.Zero;

                return this;
            }

            public Builder SetIterationsCount(long iterationsCount)
            {
                if (iterationsCount < 0)
                {
                    throw new ArgumentException($"Sentry iterations count must be greater than 0 ({iterationsCount}).",
                        nameof(iterationsCount));
                }

                _configuration.IterationsCount = iterationsCount;

                return this;
            }

            public Builder RunOnlyOnce()
            {
                _configuration.IterationsCount = 1;

                return this;
            }

            public SentryConfiguration Build() => _configuration;
        }
    }
}