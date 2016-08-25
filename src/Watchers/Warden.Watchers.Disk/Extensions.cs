using System;
using Warden.Core;

namespace Warden.Watchers.Disk
{
    /// <summary>
    /// Custom extension methods for the Disk watcher.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Extension method for adding the Disk watcher to the the WardenConfiguration with the default name of Disk Watcher.
        /// </summary>
        /// <param name="builder">Instance of the Warden configuration builder.</param>
        /// <param name="hooks">Optional lambda expression for configuring the watcher hooks.</param>
        /// <param name="interval">Optional interval (5 seconds by default) after which the next check will be invoked.</param>
        /// <param name="group">Optional name of the group that DiskWatcher belongs to.</param>
        /// <returns>Instance of fluent builder for the WardenConfiguration.</returns>
        public static WardenConfiguration.Builder AddDiskWatcher(
            this WardenConfiguration.Builder builder,
            Action<WatcherHooksConfiguration.Builder> hooks = null,
            TimeSpan? interval = null,
            string group = null)
        {
            builder.AddWatcher(DiskWatcher.Create(group: group), hooks, interval);

            return builder;
        }

        /// <summary>
        /// Extension method for adding the Disk watcher to the the WardenConfiguration..
        /// </summary>
        /// <param name="builder">Instance of the Warden configuration builder.</param>
        /// <param name="name">Name of the DiskWatcher.</param>
        /// <param name="hooks">Optional lambda expression for configuring the watcher hooks.</param>
        /// <param name="interval">Optional interval (5 seconds by default) after which the next check will be invoked.</param>
        /// <param name="group">Optional name of the group that DiskWatcher belongs to.</param>
        /// <returns>Instance of fluent builder for the WardenConfiguration.</returns>
        public static WardenConfiguration.Builder AddDiskWatcher(
            this WardenConfiguration.Builder builder,
            string name,
            Action<WatcherHooksConfiguration.Builder> hooks = null,
            TimeSpan? interval = null,
            string group = null)
        {
            builder.AddWatcher(DiskWatcher.Create(name, group: group), hooks, interval);

            return builder;
        }

        /// <summary>
        /// Extension method for adding the Disk watcher to the the WardenConfiguration.
        /// </summary>
        /// <param name="builder">Instance of the Warden configuration builder.</param>
        /// <param name="configurator">Lambda expression for configuring the DiskWatcher.</param>
        /// <param name="hooks">Optional lambda expression for configuring the watcher hooks.</param>
        /// <param name="interval">Optional interval (5 seconds by default) after which the next check will be invoked.</param>
        /// <param name="group">Optional name of the group that DiskWatcher belongs to.</param>
        /// <returns>Instance of fluent builder for the WardenConfiguration.</returns>
        public static WardenConfiguration.Builder AddDiskWatcher(
            this WardenConfiguration.Builder builder,
            Action<DiskWatcherConfiguration.Default> configurator,
            Action<WatcherHooksConfiguration.Builder> hooks = null,
            TimeSpan? interval = null,
            string group = null)
        {
            builder.AddWatcher(DiskWatcher.Create(configurator, group), hooks, interval);

            return builder;
        }


        /// <summary>
        /// Extension method for adding the Disk watcher to the the WardenConfiguration.
        /// </summary>
        /// <param name="builder">Instance of the Warden configuration builder.</param>
        /// <param name="name">Name of the DiskWatcher.</param>
        /// <param name="configurator">Lambda expression for configuring the DiskWatcher.</param>
        /// <param name="hooks">Optional lambda expression for configuring the watcher hooks.</param>
        /// <param name="interval">Optional interval (5 seconds by default) after which the next check will be invoked.</param>
        /// <param name="group">Optional name of the group that DiskWatcher belongs to.</param>
        /// <returns>Instance of fluent builder for the WardenConfiguration.</returns>
        public static WardenConfiguration.Builder AddDiskWatcher(
            this WardenConfiguration.Builder builder,
            string name,
            Action<DiskWatcherConfiguration.Default> configurator,
            Action<WatcherHooksConfiguration.Builder> hooks = null,
            TimeSpan? interval = null,
            string group = null)
        {
            builder.AddWatcher(DiskWatcher.Create(name, configurator, group), hooks, interval);

            return builder;
        }

        /// <summary>
        /// Extension method for adding the Disk watcher to the the WardenConfiguration with the default name of Disk Watcher.
        /// </summary>
        /// <param name="builder">Instance of the Warden configuration builder.</param>
        /// <param name="configuration">Configuration of DiskWatcher.</param>
        /// <param name="hooks">Optional lambda expression for configuring the watcher hooks.</param>
        /// <param name="interval">Optional interval (5 seconds by default) after which the next check will be invoked.</param>
        /// <param name="group">Optional name of the group that DiskWatcher belongs to.</param>
        /// <returns>Instance of fluent builder for the WardenConfiguration.</returns>
        public static WardenConfiguration.Builder AddDiskWatcher(
            this WardenConfiguration.Builder builder,
            DiskWatcherConfiguration configuration,
            Action<WatcherHooksConfiguration.Builder> hooks = null,
            TimeSpan? interval = null,
            string group = null)
        {
            builder.AddWatcher(DiskWatcher.Create(configuration, group), hooks, interval);

            return builder;
        }

        /// <summary>
        /// Extension method for adding the Disk watcher to the the WardenConfiguration.
        /// </summary>
        /// <param name="builder">Instance of the Warden configuration builder.</param>
        /// <param name="name">Name of the DiskWatcher.</param>
        /// <param name="configuration">Configuration of DiskWatcher.</param>
        /// <param name="hooks">Optional lambda expression for configuring the watcher hooks.</param>
        /// <param name="interval">Optional interval (5 seconds by default) after which the next check will be invoked.</param>
        /// <param name="group">Optional name of the group that DiskWatcher belongs to.</param>
        /// <returns>Instance of fluent builder for the WardenConfiguration.</returns>
        public static WardenConfiguration.Builder AddDiskWatcher(
            this WardenConfiguration.Builder builder, string name,
            DiskWatcherConfiguration configuration,
            Action<WatcherHooksConfiguration.Builder> hooks = null,
            TimeSpan? interval = null,
            string group = null)
        {
            builder.AddWatcher(DiskWatcher.Create(name, configuration, group), hooks, interval);

            return builder;
        }
    }
}