using System;
using Warden.Configurations;

namespace Warden.Watchers.Process
{
    /// <summary>
    /// Custom extension methods for the Process watcher.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Extension method for adding the Process watcher to the the WardenConfiguration with the default name of Process Watcher.
        /// </summary>
        /// <param name="builder">Instance of the Warden configuration builder.</param>
        /// <param name="processName">Name of the process.</param>
        /// <param name="hooks">Optional lambda expression for configuring the watcher hooks.</param>
        /// <returns>Instance of fluent builder for the WardenConfiguration.</returns>
        public static WardenConfiguration.Builder AddProcessWatcher(
            this WardenConfiguration.Builder builder,
            string processName,
            Action<WatcherHooksConfiguration.Builder> hooks = null)
        {
            builder.AddWatcher(ProcessWatcher.Create(processName), hooks);

            return builder;
        }

        /// <summary>
        /// Extension method for adding the Process watcher to the the WardenConfiguration with the default name of Process Watcher.
        /// </summary>
        /// <param name="builder">Instance of the Warden configuration builder.</param>
        /// <param name="name">Name of the ProcessWatcher.</param>
        /// <param name="processName">Name of the process.</param>
        /// <param name="hooks">Optional lambda expression for configuring the watcher hooks.</param>
        /// <returns>Instance of fluent builder for the WardenConfiguration.</returns>
        public static WardenConfiguration.Builder AddProcessWatcher(
            this WardenConfiguration.Builder builder, string name, string processName,
            Action<WatcherHooksConfiguration.Builder> hooks = null)
        {
            builder.AddWatcher(ProcessWatcher.Create(name, processName), hooks);

            return builder;
        }

        /// <summary>
        /// Extension method for adding the Process watcher to the the WardenConfiguration with the default name of Process Watcher.
        /// </summary>
        /// <param name="builder">Instance of the Warden configuration builder.</param>
        /// <param name="processName">Name of the process.</param>
        /// <param name="configurator">Lambda expression for configuring the ProcessWatcher.</param>
        /// <param name="hooks">Optional lambda expression for configuring the watcher hooks.</param>
        /// <returns>Instance of fluent builder for the WardenConfiguration.</returns>
        public static WardenConfiguration.Builder AddProcessWatcher(
            this WardenConfiguration.Builder builder, string processName,
            Action<ProcessWatcherConfiguration.Default> configurator,
            Action<WatcherHooksConfiguration.Builder> hooks = null)
        {
            builder.AddWatcher(ProcessWatcher.Create(processName, configurator), hooks);

            return builder;
        }

        /// <summary>
        /// Extension method for adding the Process watcher to the the WardenConfiguration with the default name of Process Watcher.
        /// </summary>
        /// <param name="builder">Instance of the Warden configuration builder.</param>
        /// <param name="name">Name of the ProcessWatcher.</param>
        /// <param name="processName">Name of the process.</param>
        /// <param name="configurator">Lambda expression for configuring the ProcessWatcher.</param>
        /// <param name="hooks">Optional lambda expression for configuring the watcher hooks.</param>
        /// <returns>Instance of fluent builder for the WardenConfiguration.</returns>
        public static WardenConfiguration.Builder AddProcessWatcher(
            this WardenConfiguration.Builder builder, string name, string processName,
            Action<ProcessWatcherConfiguration.Default> configurator,
            Action<WatcherHooksConfiguration.Builder> hooks = null)
        {
            builder.AddWatcher(ProcessWatcher.Create(name, processName, configurator), hooks);

            return builder;
        }

        /// <summary>
        /// Extension method for adding the Process watcher to the the WardenConfiguration with the default name of Process Watcher.
        /// </summary>
        /// <param name="builder">Instance of the Warden configuration builder.</param>
        /// <param name="configuration">Configuration of ProcessWatcher.</param>
        /// <param name="hooks">Optional lambda expression for configuring the watcher hooks.</param>
        /// <returns>Instance of fluent builder for the WardenConfiguration.</returns>
        public static WardenConfiguration.Builder AddProcessWatcher(
            this WardenConfiguration.Builder builder,
            ProcessWatcherConfiguration configuration,
            Action<WatcherHooksConfiguration.Builder> hooks = null)
        {
            builder.AddWatcher(ProcessWatcher.Create(configuration), hooks);

            return builder;
        }

        /// <summary>
        /// Extension method for adding the Process watcher to the the WardenConfiguration with the default name of Process Watcher.
        /// </summary>
        /// <param name="builder">Instance of the Warden configuration builder.</param>
        /// <param name="name">Name of the ProcessWatcher.</param>
        /// <param name="configuration">Configuration of ProcessWatcher.</param>
        /// <param name="hooks">Optional lambda expression for configuring the watcher hooks.</param>
        /// <returns>Instance of fluent builder for the WardenConfiguration.</returns>
        public static WardenConfiguration.Builder AddProcessWatcher(
            this WardenConfiguration.Builder builder, string name,
            ProcessWatcherConfiguration configuration,
            Action<WatcherHooksConfiguration.Builder> hooks = null)
        {
            builder.AddWatcher(ProcessWatcher.Create(name, configuration), hooks);

            return builder;
        }
    }
}