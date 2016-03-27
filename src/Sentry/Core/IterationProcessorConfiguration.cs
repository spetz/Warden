using System;
using System.Collections.Generic;

namespace Sentry.Core
{
    /// <summary>
    /// Configuration of the IterationProcessor.
    /// </summary>
    public class IterationProcessorConfiguration
    {
        /// <summary>
        /// Set of unique watcher configurations.
        /// </summary>
        public ISet<WatcherConfiguration> Watchers { get; protected set; }

        /// <summary>
        /// Configuration of hooks that are common for all of the watchers.
        /// </summary>
        public WatcherHooksConfiguration GlobalWatcherHooks { get; protected set; }

        /// <summary>
        /// Custom provider for the DateTime (UTC by default).
        /// </summary>
        public Func<DateTime> DateTimeProvider { get; protected set; }

        protected internal IterationProcessorConfiguration()
        {
            GlobalWatcherHooks = WatcherHooksConfiguration.Empty;
            Watchers = new HashSet<WatcherConfiguration>();
            DateTimeProvider = () => DateTime.UtcNow;
        }

        /// <summary>
        /// Factory method for creating a new intance of fluent builder for the IterationProcessorConfiguration.
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
            /// Provider for the custom DateTime.
            /// </summary>
            /// <param name="dateTimeProvider">Custom DateTime provider.</param>
            /// <returns>Instance of fluent builder for the IterationProcessorConfiguration.</returns>
            public Builder SetDateTimeProvider(Func<DateTime> dateTimeProvider)
            {
                _configuration.DateTimeProvider = dateTimeProvider;

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