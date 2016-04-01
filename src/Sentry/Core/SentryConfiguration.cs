using System;
using System.Collections.Generic;
using System.Linq;

namespace Sentry.Core
{
    /// <summary>
    /// Configuration of the Sentry.
    /// </summary>
    public class SentryConfiguration
    {
        /// <summary>
        /// Set of unique watcher configurations.
        /// </summary>
        public ISet<WatcherConfiguration> Watchers { get; protected set; }

        /// <summary>
        /// Configuration of Sentry hooks.
        /// </summary>
        public SentryHooksConfiguration Hooks { get; protected set; }

        /// <summary>
        /// Configuration of hooks that are common for all of the watchers.
        /// </summary>
        public WatcherHooksConfiguration GlobalWatcherHooks { get; protected set; }

        /// <summary>
        /// Delay between each iteration (5 seconds by default).
        /// </summary>
        public TimeSpan IterationDelay { get; protected set; }

        /// <summary>
        /// Total number of iterations (infinite by default).
        /// </summary>
        public long? IterationsCount { get; protected set; }

        /// <summary>
        /// Custom provider for the DateTime (UTC by default).
        /// </summary>
        public Func<DateTime> DateTimeProvider { get; protected set; }

        /// <summary>
        /// Custom provider for IIterationProcessor.
        /// </summary>
        public Func<IIterationProcessor> IterationProcessor { get; protected set; }

        protected internal SentryConfiguration()
        {
            Hooks = SentryHooksConfiguration.Empty;
            GlobalWatcherHooks = WatcherHooksConfiguration.Empty;
            Watchers = new HashSet<WatcherConfiguration>();
            IterationDelay = new TimeSpan(0, 0, 5);
            DateTimeProvider = () => DateTime.UtcNow;
        }

        /// <summary>
        /// Factory method for creating a new instance of fluent builder for the SentryConfiguration.
        /// </summary>
        /// <returns>Instance of fluent builder for the SentryConfiguration.</returns>
        public static Builder Create() => new Builder();

        /// <summary>
        /// Fluent builder for the SentryConfiguration.
        /// </summary>
        public class Builder
        {
            private readonly SentryConfiguration _configuration = new SentryConfiguration();

            /// <summary>
            /// Adds the watcher to the collection. 
            /// Hooks for this particular watcher can be configured via the lambda expression.
            /// </summary>
            /// <param name="watcher">Instance of IWatcher.</param>
            /// <param name="hooks">Optional lambda expression for configuring the watcher hooks.</param>
            /// <returns>Instance of fluent builder for the SentryConfiguration.</returns>
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

            /// <summary>
            /// Allows to set the custom provider for the DateTime.
            /// </summary>
            /// <param name="dateTimeProvider"></param>
            /// <returns>Instance of fluent builder for the SentryConfiguration.</returns>
            public Builder SetCustomDateTimeProvider(Func<DateTime> dateTimeProvider)
            {
                if (dateTimeProvider == null)
                    throw new ArgumentNullException(nameof(dateTimeProvider), "DateTime provider can not be null.");

                _configuration.DateTimeProvider = dateTimeProvider;

                return this;
            }

            /// <summary>
            /// Configure the hooks specific for the Sentry.
            /// </summary>
            /// <param name="hooks">Lambda expression for configuring the Sentry hooks.</param>
            /// <returns>Instance of fluent builder for the SentryConfiguration.</returns>
            public Builder SetHooks(Action<SentryHooksConfiguration.Builder> hooks)
            {
                var hooksConfigurationBuilder = new SentryHooksConfiguration.Builder();
                hooks(hooksConfigurationBuilder);
                _configuration.Hooks = hooksConfigurationBuilder.Build();

                return this;
            }

            /// <summary>
            /// Configure the hooks specific for the Sentry.
            /// </summary>
            /// <param name="hooks">Lambda expression for configuring the Sentry hooks.</param>
            /// <returns>Instance of fluent builder for the SentryConfiguration.</returns>
            public Builder SetHooks(Action<SentryHooksConfiguration.Builder, IIntegrator> hooks)
            {
                var hooksConfigurationBuilder = new SentryHooksConfiguration.Builder();
                hooks(hooksConfigurationBuilder, new Integrator());
                _configuration.Hooks = hooksConfigurationBuilder.Build();

                return this;
            }

            /// <summary>
            /// Configure the hooks that will be common for all of the watchers.
            /// </summary>
            /// <param name="hooks">Lambda expression for configuring the  global (common) watcher hooks.</param>
            /// <returns>Instance of fluent builder for the SentryConfiguration.</returns>
            public Builder SetGlobalWatcherHooks(Action<WatcherHooksConfiguration.Builder> hooks)
            {
                var hooksConfigurationBuilder = new WatcherHooksConfiguration.Builder();
                hooks(hooksConfigurationBuilder);
                _configuration.GlobalWatcherHooks = hooksConfigurationBuilder.Build();

                return this;
            }

            public Builder SetGlobalWatcherHooks(Action<WatcherHooksConfiguration.Builder, IIntegrator> hooks)
            {
                var hooksConfigurationBuilder = new WatcherHooksConfiguration.Builder();
                hooks(hooksConfigurationBuilder, new Integrator());
                _configuration.GlobalWatcherHooks = hooksConfigurationBuilder.Build();

                return this;
            }

            /// <summary>
            /// Sets the delay between iterations in a loop executed by IIterationProcessor.
            /// </summary>
            /// <param name="delay">Delay between each iteration (5 seconds by default).</param>
            /// <returns>Instance of fluent builder for the SentryConfiguration.</returns>
            public Builder SetIterationDelay(TimeSpan delay)
            {
                _configuration.IterationDelay = delay;

                return this;
            }

            /// <summary>
            /// Sets no delay between iterations in a loop executed by IIterationProcessor.
            /// </summary>
            /// <returns>Instance of fluent builder for the SentryConfiguration.</returns>
            public Builder WithoutIterationDelay()
            {
                _configuration.IterationDelay = TimeSpan.Zero;

                return this;
            }

            /// <summary>
            /// Sets the total number of iterations in a loop executed by IIterationProcessor.
            /// </summary>
            /// <param name="iterationsCount">Total number of iterations (infinite by default).</param>
            /// <returns>Instance of fluent builder for the SentryConfiguration.</returns>
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

            /// <summary>
            /// Sets the total number of iterations equal to 1 that will be executed by IIterationProcessor.
            /// </summary>
            /// <returns>Instance of fluent builder for the SentryConfiguration.</returns>
            public Builder RunOnlyOnce()
            {
                _configuration.IterationsCount = 1;

                return this;
            }

            /// <summary>
            /// Sets the custom provider for IIterationProcessor.
            /// </summary>
            /// <param name="iterationProcessor">Custom provider for IIterationProcessor.</param>
            /// <returns>Instance of fluent builder for the SentryConfiguration.</returns>
            public Builder SetIterationProcessor(Func<IIterationProcessor> iterationProcessor)
            {
                if (iterationProcessor == null)
                    throw new ArgumentNullException(nameof(iterationProcessor), "Iteration processor can not be null.");

                _configuration.IterationProcessor = iterationProcessor;

                return this;
            }

            private void InitializeDefaultSentryIterationProcessorIfRequired()
            {
                if (_configuration.IterationProcessor != null)
                    return;

                var iterationProcessorConfiguration = IterationProcessorConfiguration
                    .Create()
                    .SetWatchers(_configuration.Watchers.ToArray())
                    .SetGlobalWatcherHooks(_configuration.GlobalWatcherHooks)
                    .SetDateTimeProvider(_configuration.DateTimeProvider)
                    .Build();

                _configuration.IterationProcessor = () => new IterationProcessor(iterationProcessorConfiguration);
            }

            /// <summary>
            /// Builds the SentryConfiguration and return its instance.
            /// </summary>
            /// <returns>Instance of SentryConfiguration.</returns>
            public SentryConfiguration Build()
            {
                InitializeDefaultSentryIterationProcessorIfRequired();

                return _configuration;
            }
        }
    }
}