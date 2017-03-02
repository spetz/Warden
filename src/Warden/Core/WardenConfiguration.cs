using System;
using System.Collections.Generic;
using System.Linq;
using Warden.Integrations;
using Warden.Utils;
using Warden.Watchers;

namespace Warden.Core
{
    /// <summary>
    /// Configuration of the Warden.
    /// </summary>
    public class WardenConfiguration
    {
        private static readonly IIntegrator DefaultIntegrator = new Integrator();
        private static readonly TimeSpan MinimalInterval = TimeSpan.FromMilliseconds(1);
        private static readonly TimeSpan DefaultInterval = TimeSpan.FromSeconds(5);

        /// Configurator used to build configuration.
        /// Made as a required fallback inorder to handle the compatibility with a new Reconfigure() functionality.
        internal WardenConfiguration.Builder Configurator { get; }

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
        /// Interval between the next check (ExecuteAsync()) common for all of the watchers.
        /// </summary>
        public TimeSpan Interval { get; protected set; }

        /// <summary>
        /// Flag determining whether the  custom watchers intervals should be overriden (false by default).
        /// </summary>
        public bool OverrideCustomIntervals { get; protected set; }

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

        /// <summary>
        /// Custom provider for the IWardenLogger.
        /// </summary>
        public Func<IWardenLogger> WardenLoggerProvider { get; protected set; }


        /// <summary>
        /// Initialize a new instance of the WardenConfiguration using the provided configuration builder.
        /// </summary>
        /// <param name="name">Customizable name of the Warden.</param>
        /// <param name="configuration">Configuration of Warden</param>
        protected internal WardenConfiguration(WardenConfiguration.Builder configurator)
        {
            Configurator = configurator;
            Hooks = WardenHooksConfiguration.Empty;
            GlobalWatcherHooks = WatcherHooksConfiguration.Empty;
            AggregatedWatcherHooks = AggregatedWatcherHooksConfiguration.Empty;
            Watchers = new HashSet<WatcherConfiguration>();
            Interval = DefaultInterval;
            DateTimeProvider = () => DateTime.UtcNow;
            IntegratorProvider = () => DefaultIntegrator;
            WardenLoggerProvider = () => new EmptyWardenLogger();
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
            private readonly WardenConfiguration _configuration;

            public Builder()
            {
                _configuration = new WardenConfiguration(this);
            }

            /// <summary>
            /// Adds the watcher to the collection. 
            /// Hooks for this particular watcher can be configured via the lambda expression.
            /// </summary>
            /// <param name="watcher">Instance of IWatcher.</param>
            /// <param name="hooks">Optional lambda expression for configuring the watcher hooks.</param>
            /// <param name="interval">Optional interval (5 seconds by default) after which the next check will be invoked.</param>
            /// <returns>Instance of fluent builder for the WardenConfiguration.</returns>
            public Builder AddWatcher(IWatcher watcher, Action<WatcherHooksConfiguration.Builder> hooks = null, 
                TimeSpan? interval = null)
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
                    .WithInterval(interval ?? DefaultInterval)
                    .Build();

                _configuration.Watchers.Add(watcherConfiguration);

                return this;
            }

            /// <summary>
            /// Removes the watcher from the collection. 
            /// </summary>
            /// <param name="watcher">Name of IWatcher (case sensitive).</param>
            /// <returns>Instance of fluent builder for the WardenConfiguration.</returns>
            public Builder RemoveWatcher(string name)
            {
                var watcher = _configuration.Watchers.FirstOrDefault(x => x.Watcher.Name == name);
                if(watcher == null)
                {
                    throw new ArgumentException($"Watcher: '{name}' was not found.", nameof(name));
                }
                _configuration.Watchers.Remove(watcher);

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
            /// Sets the interval between the next check (ExecuteAsync()) common for all of the watchers.
            /// </summary>
            /// <param name="interval">Interval between the next check (ExecuteAsync()) for the watcher (5 seconds by default).</param>
            /// <param name="overrideCustomIntervals">Overrides already set custom interval for all of the watchers (false by default).</param>
            /// <returns>Instance of fluent builder for the WardenConfiguration.</returns>
            public Builder WithInterval(TimeSpan interval, bool overrideCustomIntervals = false)
            {
                if (interval < MinimalInterval)
                {
                    throw new ArgumentException("Interval can not be less than 1 ms.", nameof(interval));
                }

                _configuration.Interval = interval;
                _configuration.OverrideCustomIntervals = overrideCustomIntervals;

                return this;
            }

            /// <summary>
            /// Sets a minimal interval (1 ms) between the next check (ExecuteAsync()) common for all of the watchers executed in a loop by IIterationProcessor.
            /// <param name="overrideCustomIntervals">Overrides already set custom interval for all of the watchers (false by default).</param>
            /// </summary>
            /// <returns>Instance of fluent builder for the WardenConfiguration.</returns>
            public Builder WithMinimalInterval(bool overrideCustomIntervals = false)
            {
                _configuration.Interval = MinimalInterval;
                _configuration.OverrideCustomIntervals = overrideCustomIntervals;

                return this;
            }

            /// <summary>
            /// Sets the Console Logger for Warden.
            /// <param name="minLevel">Minimal level of the messages that will be logged (all by default).</param>
            ///  /// <param name="useColors">Flag determining whether to use colors for different levels (true by default).</param>
            /// </summary>
            /// <returns>Instance of fluent builder for the WardenConfiguration.</returns>
            public Builder WithConsoleLogger(WardenLoggerLevel minLevel = WardenLoggerLevel.All, bool useColors = true)
            {
                _configuration.WardenLoggerProvider = () => new ConsoleWardenLogger(minLevel, useColors);

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
                {
                    throw new ArgumentNullException(nameof(dateTimeProvider), "DateTime provider can not be null.");
                }

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
                {
                    throw new ArgumentNullException(nameof(iterationProcessorProvider), "Iteration processor provider can not be null.");
                }

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
                {
                    throw new ArgumentNullException(nameof(integratorProvider), "Integrator processor can not be null.");
                }

                _configuration.IntegratorProvider = integratorProvider;

                return this;
            }

            /// <summary>
            /// Sets the custom provider for IWardenLogger.
            /// </summary>
            /// <param name="wardenLoggerProvider">Custom provider for IWardenLogger.</param>
            /// <returns>Instance of fluent builder for the WardenConfiguration.</returns>
            public Builder SetLogger(Func<IWardenLogger> wardenLoggerProvider)
            {
                if (wardenLoggerProvider == null)
                {
                    throw new ArgumentNullException(nameof(wardenLoggerProvider), "Warden logger can not be null.");
                }

                _configuration.WardenLoggerProvider = wardenLoggerProvider;

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
                    .WithInterval(_configuration.Interval, _configuration.OverrideCustomIntervals)
                    .SetLogger(_configuration.WardenLoggerProvider)
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