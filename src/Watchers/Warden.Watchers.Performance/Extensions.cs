using System;
using Warden.Core;

namespace Warden.Watchers.Performance
{
    /// <summary>
    /// Custom extension methods for the Performance watcher.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Extension method for adding the Performance watcher to the the WardenConfiguration with the default name of Performance Watcher.
        /// </summary>
        /// <param name="builder">Instance of the Warden configuration builder.</param>
        /// <param name="hooks">Optional lambda expression for configuring the watcher hooks.</param>
        /// <param name="delay">Delay between resource usage calculation while using the default performance counter (100 ms by default).</param>
        /// <param name="interval">Optional interval (5 seconds by default) after which the next check will be invoked.</param>
        /// <returns>Instance of fluent builder for the WardenConfiguration.</returns>
        public static WardenConfiguration.Builder AddPerformanceWatcher(
            this WardenConfiguration.Builder builder, 
            Action<WatcherHooksConfiguration.Builder> hooks = null,
            TimeSpan? delay = null,
            TimeSpan? interval = null)
        {
            builder.AddWatcher(PerformanceWatcher.Create(delay), hooks, interval);

            return builder;
        }

        /// <summary>
        /// Extension method for adding the Performance watcher to the the WardenConfiguration with the default name of Performance Watcher.
        /// </summary>
        /// <param name="builder">Instance of the Warden configuration builder.</param>
        /// <param name="name">Name of the PerformanceWatcher.</param>
        /// <param name="hooks">Optional lambda expression for configuring the watcher hooks.</param>
        /// <param name="delay">Delay between resource usage calculation while using the default performance counter (100 ms by default).</param>
        /// <param name="interval">Optional interval (5 seconds by default) after which the next check will be invoked.</param>
        /// <returns>Instance of fluent builder for the WardenConfiguration.</returns>
        public static WardenConfiguration.Builder AddPerformanceWatcher(
            this WardenConfiguration.Builder builder, 
            string name, 
            Action<WatcherHooksConfiguration.Builder> hooks = null, 
            TimeSpan? delay = null,
            TimeSpan? interval = null)
        {
            builder.AddWatcher(PerformanceWatcher.Create(name, delay), hooks, interval);

            return builder;
        }

        /// <summary>
        /// Extension method for adding the Performance watcher to the the WardenConfiguration with the default name of Performance Watcher.
        /// </summary>
        /// <param name="builder">Instance of the Warden configuration builder.</param>
        /// <param name="configurator">Lambda expression for configuring the PerformanceWatcher.</param>
        /// <param name="hooks">Optional lambda expression for configuring the watcher hooks.</param>
        /// <param name="delay">Delay between resource usage calculation while using the default performance counter (100 ms by default).</param>
        /// <param name="interval">Optional interval (5 seconds by default) after which the next check will be invoked.</param>
        /// <returns>Instance of fluent builder for the WardenConfiguration.</returns>
        public static WardenConfiguration.Builder AddPerformanceWatcher(
            this WardenConfiguration.Builder builder,
            Action<PerformanceWatcherConfiguration.Default> configurator,
            Action<WatcherHooksConfiguration.Builder> hooks = null, 
            TimeSpan? delay = null,
            TimeSpan? interval = null)
        {
            builder.AddWatcher(PerformanceWatcher.Create(delay, configurator), hooks, interval);

            return builder;
        }

        /// <summary>
        /// Extension method for adding the Performance watcher to the the WardenConfiguration with the default name of Performance Watcher.
        /// </summary>
        /// <param name="builder">Instance of the Warden configuration builder.</param>
        /// <param name="name">Name of the PerformanceWatcher.</param>
        /// <param name="configurator">Lambda expression for configuring the PerformanceWatcher.</param>
        /// <param name="hooks">Optional lambda expression for configuring the watcher hooks.</param>
        /// <param name="delay">Delay between resource usage calculation while using the default performance counter (100 ms by default).</param>
        /// <param name="interval">Optional interval (5 seconds by default) after which the next check will be invoked.</param>
        /// <returns>Instance of fluent builder for the WardenConfiguration.</returns>
        public static WardenConfiguration.Builder AddPerformanceWatcher(
            this WardenConfiguration.Builder builder, 
            string name,
            Action<PerformanceWatcherConfiguration.Default> configurator,
            Action<WatcherHooksConfiguration.Builder> hooks = null, 
            TimeSpan? delay = null,
            TimeSpan? interval = null)
        {
            builder.AddWatcher(PerformanceWatcher.Create(name, delay, configurator), hooks, interval);

            return builder;
        }

        /// <summary>
        /// Extension method for adding the Performance watcher to the the WardenConfiguration with the default name of Performance Watcher.
        /// </summary>
        /// <param name="builder">Instance of the Warden configuration builder.</param>
        /// <param name="configuration">Configuration of PerformanceWatcher.</param>
        /// <param name="hooks">Optional lambda expression for configuring the watcher hooks.</param>
        /// <param name="interval">Optional interval (5 seconds by default) after which the next check will be invoked.</param>
        /// <returns>Instance of fluent builder for the WardenConfiguration.</returns>
        public static WardenConfiguration.Builder AddPerformanceWatcher(
            this WardenConfiguration.Builder builder,
            PerformanceWatcherConfiguration configuration,
            Action<WatcherHooksConfiguration.Builder> hooks = null,
            TimeSpan? interval = null)
        {
            builder.AddWatcher(PerformanceWatcher.Create(configuration), hooks, interval);

            return builder;
        }

        /// <summary>
        /// Extension method for adding the Performance watcher to the the WardenConfiguration with the default name of Performance Watcher.
        /// </summary>
        /// <param name="builder">Instance of the Warden configuration builder.</param>
        /// <param name="name">Name of the PerformanceWatcher.</param>
        /// <param name="configuration">Configuration of PerformanceWatcher.</param>
        /// <param name="hooks">Optional lambda expression for configuring the watcher hooks.</param>
        /// <param name="interval">Optional interval (5 seconds by default) after which the next check will be invoked.</param>
        /// <returns>Instance of fluent builder for the WardenConfiguration.</returns>
        public static WardenConfiguration.Builder AddPerformanceWatcher(
            this WardenConfiguration.Builder builder, 
            string name,
            PerformanceWatcherConfiguration configuration,
            Action<WatcherHooksConfiguration.Builder> hooks = null,
            TimeSpan? interval = null)
        {
            builder.AddWatcher(PerformanceWatcher.Create(name, configuration), hooks, interval);

            return builder;
        }
    }
}