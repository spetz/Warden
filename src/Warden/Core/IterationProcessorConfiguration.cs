using System;
using System.Collections.Generic;
using Warden.Watchers;

namespace Warden.Core
{
    /// <summary>
    /// Configuration of the IterationProcessor.
    /// </summary>
    public class IterationProcessorConfiguration
    {
        private static readonly TimeSpan MinimalInterval = TimeSpan.FromMilliseconds(1);

        /// <summary>
        /// Set of unique watcher configurations.
        /// </summary>
        public ISet<WatcherConfiguration> Watchers { get; protected set; }

        /// <summary>
        /// Configuration of hooks that are common for all of the watchers.
        /// </summary>
        public WatcherHooksConfiguration GlobalWatcherHooks { get; protected set; }

        /// <summary>
        /// Configuration of aggregated hooks that are common for all of the watchers.
        /// </summary>
        public AggregatedWatcherHooksConfiguration AggregatedGlobalWatcherHooks { get; protected set; }

        /// <summary>
        /// Custom provider for the DateTime (UTC by default).
        /// </summary>
        public Func<DateTime> DateTimeProvider { get; protected set; }

        /// <summary>
        /// Default interval between the next check (ExecuteAsync()) common for all of the watchers.
        /// </summary>
        public TimeSpan DefaultInterval { get; protected set; }

        protected internal IterationProcessorConfiguration()
        {
            GlobalWatcherHooks = WatcherHooksConfiguration.Empty;
            AggregatedGlobalWatcherHooks = AggregatedWatcherHooksConfiguration.Empty;
            Watchers = new HashSet<WatcherConfiguration>();
            DateTimeProvider = () => DateTime.UtcNow;
        }

        /// <summary>
        /// Factory method for creating a new instance of fluent builder for the IterationProcessorConfiguration.
        /// </summary>
        /// <returns>Instance of fluent builder for the IterationProcessorConfiguration.</returns>
        public static Builder Create() => new Builder();

        /// <summary>
        /// Fluent builder for the IterationProcessorConfiguration.
        /// </summary>
        public class Builder
        {
            private readonly IterationProcessorConfiguration _configuration = new IterationProcessorConfiguration();

            /// <summary>
            /// Pass the collection of watchers that should be executed during the iteration (cycle).
            /// </summary>
            /// <param name="watchers">One or more custom watcher configurations.</param>
            /// <returns>Instance of fluent builder for the IterationProcessorConfiguration.</returns>
            public Builder SetWatchers(params WatcherConfiguration[] watchers)
            {
                _configuration.Watchers = new HashSet<WatcherConfiguration>(watchers);

                return this;
            }

            /// <summary>
            /// Configure the hooks that will be common for all of the watchers.
            /// </summary>
            /// <param name="configuration">Configuration of watcher hooks.</param>
            /// <returns>Instance of fluent builder for the IterationProcessorConfiguration.</returns>
            public Builder SetGlobalWatcherHooks(WatcherHooksConfiguration configuration)
            {
                _configuration.GlobalWatcherHooks = configuration;

                return this;
            }

            /// <summary>
            /// Configure aggregated hooks including all of the watchers
            /// </summary>
            /// <param name="configuration">Configuration of the aggregated watcher hooks.</param>
            /// <returns>Instance of fluent builder for the IterationProcessorConfiguration.</returns>
            public Builder SetAggregatedWatcherHooks(AggregatedWatcherHooksConfiguration configuration)
            {
                _configuration.AggregatedGlobalWatcherHooks = configuration;

                return this;
            }

            /// <summary>
            /// Provider for the custom DateTime.
            /// </summary>
            /// <param name="dateTimeProvider">Custom DateTime provider.</param>
            /// <returns>Instance of fluent builder for the IterationProcessorConfiguration.</returns>
            public Builder SetDateTimeProvider(Func<DateTime> dateTimeProvider)
            {
                _configuration.DateTimeProvider = dateTimeProvider;

                return this;
            }

            //TODO: Implement overrideCustomIntervals feature.
            /// <summary>
            /// Sets the default interval between the next check (ExecuteAsync()) common for all of the watchers.
            /// </summary>
            /// <param name="interval">Interval between the next check (ExecuteAsync()) for the watcher (5 seconds by default).</param>
            /// <param name="overrideCustomIntervals">Overrides already set custom interval for all of the watchers (false by default).</param>
            /// <returns>Instance of fluent builder for the WardenConfiguration.</returns>
            public Builder WithDefaultInterval(TimeSpan interval, bool overrideCustomIntervals = false)
            {
                if (interval < MinimalInterval)
                    throw new ArgumentException("Interval can not be less than 1 ms.", nameof(interval));

                _configuration.DefaultInterval = interval;

                return this;
            }

            /// <summary>
            /// Builds the IterationProcessorConfiguration and return its instance.
            /// </summary>
            /// <returns>Instance of IterationProcessorConfiguration.</returns>
            public IterationProcessorConfiguration Build() => _configuration;
        }
    }
}