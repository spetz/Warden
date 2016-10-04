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
        /// <param name="machineName">Optional name of the remote machine.</param>
        /// <param name="interval">Optional interval (5 seconds by default) after which the next check will be invoked.</param>
        /// <param name="group">Optional name of the group that PerformanceWatcher belongs to.</param>
        /// <returns>Instance of fluent builder for the WardenConfiguration.</returns>
        public static WardenConfiguration.Builder AddPerformanceWatcher(
            this WardenConfiguration.Builder builder, 
            Action<WatcherHooksConfiguration.Builder> hooks = null,
            TimeSpan? delay = null,
            string machineName = null,
            TimeSpan? interval = null,
            string group = null)
        {
            builder.AddWatcher(PerformanceWatcher.Create(delay, machineName, group: group), hooks, interval);

            return builder;
        }

        /// <summary>
        /// Extension method for adding the Performance watcher to the the WardenConfiguration with the default name of Performance Watcher.
        /// </summary>
        /// <param name="builder">Instance of the Warden configuration builder.</param>
        /// <param name="name">Name of the PerformanceWatcher.</param>
        /// <param name="hooks">Optional lambda expression for configuring the watcher hooks.</param>
        /// <param name="delay">Delay between resource usage calculation while using the default performance counter (100 ms by default).</param>
        /// <param name="machineName">Optional name of the remote machine.</param>
        /// <param name="interval">Optional interval (5 seconds by default) after which the next check will be invoked.</param>
        /// <param name="group">Optional name of the group that PerformanceWatcher belongs to.</param>
        /// <returns>Instance of fluent builder for the WardenConfiguration.</returns>
        public static WardenConfiguration.Builder AddPerformanceWatcher(
            this WardenConfiguration.Builder builder, 
            string name, 
            Action<WatcherHooksConfiguration.Builder> hooks = null, 
            TimeSpan? delay = null,
            string machineName = null,
            TimeSpan? interval = null,
            string group = null)
        {
            builder.AddWatcher(PerformanceWatcher.Create(name, delay, machineName, group: group), hooks, interval);

            return builder;
        }

        /// <summary>
        /// Extension method for adding the Performance watcher to the the WardenConfiguration with the default name of Performance Watcher.
        /// </summary>
        /// <param name="builder">Instance of the Warden configuration builder.</param>
        /// <param name="configurator">Lambda expression for configuring the PerformanceWatcher.</param>
        /// <param name="hooks">Optional lambda expression for configuring the watcher hooks.</param>
        /// <param name="delay">Delay between resource usage calculation while using the default performance counter (100 ms by default).</param>
        /// <param name="machineName">Optional name of the remote machine.</param>
        /// <param name="interval">Optional interval (5 seconds by default) after which the next check will be invoked.</param>
        /// <param name="group">Optional name of the group that PerformanceWatcher belongs to.</param>
        /// <returns>Instance of fluent builder for the WardenConfiguration.</returns>
        public static WardenConfiguration.Builder AddPerformanceWatcher(
            this WardenConfiguration.Builder builder,
            Action<PerformanceWatcherConfiguration.Default> configurator,
            Action<WatcherHooksConfiguration.Builder> hooks = null, 
            TimeSpan? delay = null,
            string machineName = null,
            TimeSpan? interval = null,
            string group = null)
        {
            builder.AddWatcher(PerformanceWatcher.Create(delay, machineName, configurator, group), hooks, interval);

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
        /// <param name="machineName">Optional name of the remote machine.</param>
        /// <param name="interval">Optional interval (5 seconds by default) after which the next check will be invoked.</param>
        /// <param name="group">Optional name of the group that PerformanceWatcher belongs to.</param>
        /// <returns>Instance of fluent builder for the WardenConfiguration.</returns>
        public static WardenConfiguration.Builder AddPerformanceWatcher(
            this WardenConfiguration.Builder builder, 
            string name,
            Action<PerformanceWatcherConfiguration.Default> configurator,
            Action<WatcherHooksConfiguration.Builder> hooks = null, 
            TimeSpan? delay = null,
            string machineName = null,
            TimeSpan? interval = null,
            string group = null)
        {
            builder.AddWatcher(PerformanceWatcher.Create(name, delay, machineName, configurator, group), hooks, interval);

            return builder;
        }

        /// <summary>
        /// Extension method for adding the Performance watcher to the the WardenConfiguration with the default name of Performance Watcher.
        /// </summary>
        /// <param name="builder">Instance of the Warden configuration builder.</param>
        /// <param name="configuration">Configuration of PerformanceWatcher.</param>
        /// <param name="hooks">Optional lambda expression for configuring the watcher hooks.</param>
        /// <param name="interval">Optional interval (5 seconds by default) after which the next check will be invoked.</param>
        /// <param name="group">Optional name of the group that PerformanceWatcher belongs to.</param>
        /// <returns>Instance of fluent builder for the WardenConfiguration.</returns>
        public static WardenConfiguration.Builder AddPerformanceWatcher(
            this WardenConfiguration.Builder builder,
            PerformanceWatcherConfiguration configuration,
            Action<WatcherHooksConfiguration.Builder> hooks = null,
            TimeSpan? interval = null,
            string group = null)
        {
            builder.AddWatcher(PerformanceWatcher.Create(configuration, group), hooks, interval);

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
        /// <param name="group">Optional name of the group that PerformanceWatcher belongs to.</param>
        /// <returns>Instance of fluent builder for the WardenConfiguration.</returns>
        public static WardenConfiguration.Builder AddPerformanceWatcher(
            this WardenConfiguration.Builder builder, 
            string name,
            PerformanceWatcherConfiguration configuration,
            Action<WatcherHooksConfiguration.Builder> hooks = null,
            TimeSpan? interval = null,
            string group = null)
        {
            builder.AddWatcher(PerformanceWatcher.Create(name, configuration, group), hooks, interval);

            return builder;
        }
    }
}