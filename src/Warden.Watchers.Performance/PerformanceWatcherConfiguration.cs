using System;
using System.Threading.Tasks;

namespace Warden.Watchers.Performance
{
    /// <summary>
    /// Configuration of the PerformanceWatcher.
    /// </summary>
    public class PerformanceWatcherConfiguration
    {
        /// <summary>
        /// Delay between resource usage calculation while using the default performance counter (100 ms by default).
        /// </summary>
        public TimeSpan Delay { get; protected set; }

        /// <summary>
        /// Predicate that has to be satisfied in order to return the valid result.
        /// </summary>
        public Func<ResourceUsage, bool> EnsureThat { get; protected set; }

        /// <summary>
        /// Async predicate that has to be satisfied in order to return the valid result.
        /// </summary>
        public Func<ResourceUsage, Task<bool>> EnsureThatAsync { get; protected set; }

        /// <summary>
        /// Custom provider for the IPerformance.
        /// </summary>
        public Func<IPerformance> PerformanceProvider { get; protected set; }

        protected internal PerformanceWatcherConfiguration(TimeSpan? delay = null)
        {
            Delay = delay ?? TimeSpan.FromMilliseconds(100);
            PerformanceProvider = () => new Performance(Delay);
        }

        /// <summary>
        /// Factory method for creating a new instance of fluent builder for the PerformanceWatcherConfiguration.
        /// </summary>
        /// <param name="delay">Delay between resource usage calculation while using the default performance counter (100 ms by default).</param>
        /// <returns>Instance of fluent builder for the PerformanceWatcherConfiguration.</returns>
        public static Builder Create(TimeSpan? delay = null) => new Builder(delay);

        public abstract class Configurator<T> : WatcherConfigurator<T, PerformanceWatcherConfiguration>
            where T : Configurator<T>
        {
            protected Configurator(TimeSpan? delay = null)
            {
                Configuration = new PerformanceWatcherConfiguration(delay);
            }

            protected Configurator(PerformanceWatcherConfiguration configuration) : base(configuration)
            {
            }

            /// <summary>
            /// Sets the predicate that has to be satisfied in order to return the valid result.
            /// </summary>
            /// <param name="ensureThat">Lambda expression predicate.</param>
            /// <returns>Instance of fluent builder for the PerformanceWatcherConfiguration.</returns>
            public T EnsureThat(Func<ResourceUsage, bool> ensureThat)
            {
                if (ensureThat == null)
                    throw new ArgumentException("Ensure that predicate can not be null.", nameof(ensureThat));

                Configuration.EnsureThat = ensureThat;

                return Configurator;
            }

            /// <summary>
            /// Sets the async predicate that has to be satisfied in order to return the valid result.
            /// </summary>
            /// <param name="ensureThat">Lambda expression predicate.</param>
            /// <returns>Instance of fluent builder for the PerformanceWatcherConfiguration.</returns>
            public T EnsureThatAsync(Func<ResourceUsage, Task<bool>> ensureThat)
            {
                if (ensureThat == null)
                    throw new ArgumentException("Ensure that async predicate can not be null.", nameof(ensureThat));

                Configuration.EnsureThatAsync = ensureThat;

                return Configurator;
            }

            /// <summary>
            /// Sets the custom provider for the IPerformance.
            /// </summary>
            /// <param name="performanceProvider">Custom provider for the IPerformance.</param>
            /// <returns>Lambda expression returning an instance of the IPerformance.</returns>
            /// <returns>Instance of fluent builder for the PerformanceWatcherConfiguration.</returns>
            public T WithPerformanceProvider(Func<IPerformance> performanceProvider)
            {
                if (performanceProvider == null)
                {
                    throw new ArgumentNullException(nameof(performanceProvider),
                        "IPerformance provider can not be null.");
                }

                Configuration.PerformanceProvider = performanceProvider;

                return Configurator;
            }
        }

        /// <summary>
        /// Default PerformanceWatcherConfiguration fluent builder used while configuring watcher via lambda expression.
        /// </summary>
        public class Default : Configurator<Default>
        {
            public Default(PerformanceWatcherConfiguration configuration) : base(configuration)
            {
                SetInstance(this);
            }
        }

        /// <summary>
        /// Extended PerformanceWatcherConfiguration fluent builder used while configuring watcher directly.
        /// </summary>
        public class Builder : Configurator<Builder>
        {
            public Builder(TimeSpan? delay = null) : base(delay)
            {
                SetInstance(this);
            }

            /// <summary>
            /// Builds the PerformanceWatcherConfiguration and return its instance.
            /// </summary>
            /// <returns>Instance of PerformanceWatcherConfiguration.</returns>
            public PerformanceWatcherConfiguration Build() => Configuration;

            /// <summary>
            /// Operator overload to provide casting the Builder configurator into Default configurator.
            /// </summary>
            /// <param name="builder">Instance of extended Builder configurator.</param>
            /// <returns>Instance of Default builder configurator.</returns>
            public static explicit operator Default(Builder builder) => new Default(builder.Configuration);
        }
    }
}