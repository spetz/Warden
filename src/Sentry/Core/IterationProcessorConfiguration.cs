using System;
using System.Collections.Generic;

namespace Sentry.Core
{
    public class IterationProcessorConfiguration
    {
        public ICollection<WatcherConfiguration> Watchers { get; protected set; }
        public WatcherHooksConfiguration GlobalWatcherHooks { get; protected set; }
        public Func<DateTime> DateTimeProvider { get; protected set; }

        protected internal IterationProcessorConfiguration()
        {
            GlobalWatcherHooks = WatcherHooksConfiguration.Empty;
            Watchers = new List<WatcherConfiguration>();
            DateTimeProvider = () => DateTime.UtcNow;
        }

        public static Builder Create() => new Builder();

        public class Builder
        {
            private readonly IterationProcessorConfiguration _configuration = new IterationProcessorConfiguration();

            public Builder SetWatchers(params WatcherConfiguration[] watchers)
            {
                _configuration.Watchers = watchers;

                return this;
            }

            public Builder SetGlobalWatcherHooks(WatcherHooksConfiguration configuration)
            {
                _configuration.GlobalWatcherHooks = configuration;

                return this;
            }


            public Builder SetDateTimeProvider(Func<DateTime> dateTimeProvider)
            {
                _configuration.DateTimeProvider = dateTimeProvider;

                return this;
            }

            public IterationProcessorConfiguration Build() => _configuration;
        }
    }
}