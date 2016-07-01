using System;
using System.Collections.Generic;
using System.Linq;
using Warden.Integrations;
using Warden.Watchers;

namespace Warden.Core
{
    /// <summary>
    /// Configuration of the Warden.
    /// </summary>
    public class WardenConfiguration
    {
        private static readonly IIntegrator DefaultIntegrator = new Integrator();
        private static readonly TimeSpan MinimalIterationDelay = TimeSpan.FromMilliseconds(1);
        private static readonly TimeSpan DefaultIterationDelay = TimeSpan.FromSeconds(5);

        /// <summary>
        /// Set of unique watcher configurations.
        /// </summary>
        public ISet<WatcherConfiguration> Watchers { get; protected set; }

        /// <summary>
        /// Configuration of Warden hooks.
        /// </summary>
        public WardenHooksConfiguration Hooks { get; protected set; }

        /// <summary>
        /// Configuration of hooks that are common for all of the watchers.
        /// </summary>
        public WatcherHooksConfiguration GlobalWatcherHooks { get; protected set; }

        /// <summary>
        /// Configuration of aggregated hooks including all of the watchers.
        /// </summary>
        public AggregatedWatcherHooksConfiguration AggregatedWatcherHooks { get; protected set; }

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
        public Func<IIterationProcessor> IterationProcessorProvider { get; protected set; }

        /// <summary>
        /// Custom provider for the IIntegrator.
        /// </summary>
        public Func<IIntegrator> IntegratorProvider { get; protected set; }


        protected internal WardenConfiguration()
        {
            Hooks = WardenHooksConfiguration.Empty;
            GlobalWatcherHooks = WatcherHooksConfiguration.Empty;
            AggregatedWatcherHooks = AggregatedWatcherHooksConfiguration.Empty;
            Watchers = new HashSet<WatcherConfiguration>();
            IterationDelay = DefaultIterationDelay;
            DateTimeProvider = () => DateTime.UtcNow;
            IntegratorProvider = () => DefaultIntegrator;
        }

        /// <summary>
        /// Factory method for creating a new instance of fxluent builder for the WardenConfiguration.
        /// </summary>
        /// <returns>Instance of fluent builder for the WardenConfiguration.</returns>
        public static Builder Create() => new Builder();

        /// <summary>
        /// Fluent builder for the WardenConfiguration.
        /// </summary>
        public class Builder
        {
            private readonly WardenConfiguration _configuration = new WardenConfiguration();

            /// <summary>
            /// Adds the watcher to the collection. 
            /// Hooks for this particular watcher can be configured via the lambda expression.
            /// </summary>
            /// <param name="watcher">Instance of IWatcher.</param>
            /// <param name="hooks">Optional lambda expression for configuring the watcher hooks.</param>
            /// <param name="interval">Optional interval (5 seconds by default) after which the next check will be invoked.</param>
            /// <returns>Instance of fluent builder for the WardenConfiguration.</returns>
            public Builder AddWatcher(IWatcher watcher, Action<WatcherHooksConfiguration.Builder> hooks = null, TimeSpan? interval = null)
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
                    .WithInterval(interval ?? DefaultIterationDelay)
                    .Build();

                _configuration.Watchers.Add(watcherConfiguration);

                return this;
            }

            /// <summary>
            /// Register the IIntegration in the IIntegrator. 
            /// </summary>
            /// <param name="integration">Instance of IIntegration.</param>
            /// <returns>Instance of fluent builder for the WardenConfiguration.</returns>
            public Builder AddIntegration<T>(T integration) where T : class, IIntegration
            {
                _configuration.IntegratorProvider().Register(integration);

                return this;
            }

            /// <summary>
            /// Configure the hooks specific for the Warden.
            /// </summary>
            /// <param name="hooks">Lambda expression for configuring the Warden hooks.</param>
            /// <returns>Instance of fluent builder for the WardenConfiguration.</returns>
            public Builder SetHooks(Action<WardenHooksConfiguration.Builder> hooks)
            {
                var hooksConfigurationBuilder = new WardenHooksConfiguration.Builder();
                hooks(hooksConfigurationBuilder);
                _configuration.Hooks = hooksConfigurationBuilder.Build();

                return this;
            }

            /// <summary>
            /// Configure the hooks specific for the Warden.
            /// </summary>
            /// <param name="hooks">Lambda expression for configuring the Warden hooks.</param>
            /// <returns>Instance of fluent builder for the WardenConfiguration.</returns>
            public Builder SetHooks(Action<WardenHooksConfiguration.Builder, IIntegrator> hooks)
            {
                var hooksConfigurationBuilder = new WardenHooksConfiguration.Builder();
                hooks(hooksConfigurationBuilder, _configuration.IntegratorProvider());
                _configuration.Hooks = hooksConfigurationBuilder.Build();

                return this;
            }

            /// <summary>
            /// Configure the hooks that will be common for all of the watchers.
            /// </summary>
            /// <param name="hooks">Lambda expression for configuring the  global (common) watcher hooks.</param>
            /// <returns>Instance of fluent builder for the WardenConfiguration.</returns>
            public Builder SetGlobalWatcherHooks(Action<WatcherHooksConfiguration.Builder> hooks)
            {
                var hooksConfigurationBuilder = new WatcherHooksConfiguration.Builder();
                hooks(hooksConfigurationBuilder);
                _configuration.GlobalWatcherHooks = hooksConfigurationBuilder.Build();

                return this;
            }
            
            /// <summary>
            /// Configure the hooks specific for the Warden.
            /// </summary>
            /// <param name="hooks">Lambda expression for configuring the Warden hooks.</param>
            /// <returns>Instance of fluent builder for the WardenConfiguration.</returns>
            public Builder SetGlobalWatcherHooks(Action<WatcherHooksConfiguration.Builder, IIntegrator> hooks)
            {
                var hooksConfigurationBuilder = new WatcherHooksConfiguration.Builder();
                hooks(hooksConfigurationBuilder, _configuration.IntegratorProvider());
                _configuration.GlobalWatcherHooks = hooksConfigurationBuilder.Build();

                return this;
            }

            /// <summary>
            /// Configure aggregated hooks including all of the watchers
            /// </summary>
            /// <param name="hooks">Lambda expression for configuring aggregated watcher hooks.</param>
            /// <returns>Instance of fluent builder for the WardenConfiguration.</returns>
            public Builder SetAggregatedWatcherHooks(Action<AggregatedWatcherHooksConfiguration.Builder> hooks)
            {
                var hooksConfigurationBuilder = new AggregatedWatcherHooksConfiguration.Builder();
                hooks(hooksConfigurationBuilder);
                _configuration.AggregatedWatcherHooks = hooksConfigurationBuilder.Build();

                return this;
            }

            /// <summary>
            /// Configure aggregated hooks including all of the watchers
            /// </summary>
            /// <param name="hooks">Lambda expression for configuring the Warden hooks.</param>
            /// <returns>Instance of fluent builder for the WardenConfiguration.</returns>
            public Builder SetAggregatedWatcherHooks(Action<AggregatedWatcherHooksConfiguration.Builder, IIntegrator> hooks)
            {
                var hooksConfigurationBuilder = new AggregatedWatcherHooksConfiguration.Builder();
                hooks(hooksConfigurationBuilder, _configuration.IntegratorProvider());
                _configuration.AggregatedWatcherHooks = hooksConfigurationBuilder.Build();

                return this;
            }

            /// <summary>
            /// Sets the delay between iterations in a loop executed by IIterationProcessor.
            /// </summary>
            /// <param name="delay">Delay between each iteration (5 seconds by default).</param>
            /// <returns>Instance of fluent builder for the WardenConfiguration.</returns>
            public Builder SetIterationDelay(TimeSpan delay)
            {
                if (delay < MinimalIterationDelay)
                    throw new ArgumentException("Iteration delay can not be less than 1 ms.", nameof(delay));

                _configuration.IterationDelay = delay;

                return this;
            }

            /// <summary>
            /// Sets a minimal delay (1 ms) between iterations in a loop executed by IIterationProcessor.
            /// </summary>
            /// <returns>Instance of fluent builder for the WardenConfiguration.</returns>
            public Builder WithMinimalIterationDelay()
            {
                _configuration.IterationDelay = MinimalIterationDelay;

                return this;
            }

            /// <summary>
            /// Sets the total number of iterations in a loop executed by IIterationProcessor.
            /// </summary>
            /// <param name="iterationsCount">Total number of iterations (infinite by default).</param>
            /// <returns>Instance of fluent builder for the WardenConfiguration.</returns>
            public Builder SetIterationsCount(long iterationsCount)
            {
                if (iterationsCount < 0)
                {
                    throw new ArgumentException($"Warden iterations count must be greater than 0 ({iterationsCount}).",
                        nameof(iterationsCount));
                }

                _configuration.IterationsCount = iterationsCount;

                return this;
            }

            /// <summary>
            /// Sets the total number of iterations equal to 1 that will be executed by IIterationProcessor.
            /// </summary>
            /// <returns>Instance of fluent builder for the WardenConfiguration.</returns>
            public Builder RunOnlyOnce()
            {
                _configuration.IterationsCount = 1;

                return this;
            }

            /// <summary>
            /// Allows to set the custom provider for the DateTime.
            /// </summary>
            /// <param name="dateTimeProvider"></param>
            /// <returns>Instance of fluent builder for the WardenConfiguration.</returns>
            public Builder SetDateTimeProvider(Func<DateTime> dateTimeProvider)
            {
                if (dateTimeProvider == null)
                    throw new ArgumentNullException(nameof(dateTimeProvider), "DateTime provider can not be null.");

                _configuration.DateTimeProvider = dateTimeProvider;

                return this;
            }

            /// <summary>
            /// Sets the custom provider for IIterationProcessor.
            /// </summary>
            /// <param name="iterationProcessorProvider">Custom provider for IIterationProcessor.</param>
            /// <returns>Instance of fluent builder for the WardenConfiguration.</returns>
            public Builder SetIterationProcessorProvider(Func<IIterationProcessor> iterationProcessorProvider)
            {
                if (iterationProcessorProvider == null)
                    throw new ArgumentNullException(nameof(iterationProcessorProvider), "Iteration processor provider can not be null.");

                _configuration.IterationProcessorProvider = iterationProcessorProvider;

                return this;
            }

            /// <summary>
            /// Sets the custom provider for IIterationProcessor.
            /// </summary>
            /// <param name="integratorProvider">Custom provider for IIntegrator.</param>
            /// <returns>Instance of fluent builder for the WardenConfiguration.</returns>
            public Builder SetIntegratorProvider(Func<IIntegrator> integratorProvider)
            {
                if (integratorProvider == null)
                    throw new ArgumentNullException(nameof(integratorProvider), "Integrator processor can not be null.");

                _configuration.IntegratorProvider = integratorProvider;

                return this;
            }

            private void InitializeDefaultWardenIterationProcessorIfRequired()
            {
                if (_configuration.IterationProcessorProvider != null)
                    return;

                var iterationProcessorConfiguration = IterationProcessorConfiguration
                    .Create()
                    .SetWatchers(_configuration.Watchers.ToArray())
                    .SetGlobalWatcherHooks(_configuration.GlobalWatcherHooks)
                    .SetAggregatedWatcherHooks(_configuration.AggregatedWatcherHooks)
                    .SetDateTimeProvider(_configuration.DateTimeProvider)
                    .SetIterationDelay(_configuration.IterationDelay)
                    .Build();

                _configuration.IterationProcessorProvider = () => new IterationProcessor(iterationProcessorConfiguration);
            }

            /// <summary>
            /// Builds the WardenConfiguration and return its instance.
            /// </summary>
            /// <returns>Instance of WardenConfiguration.</returns>
            public WardenConfiguration Build()
            {
                InitializeDefaultWardenIterationProcessorIfRequired();

                return _configuration;
            }
        }
    }
}