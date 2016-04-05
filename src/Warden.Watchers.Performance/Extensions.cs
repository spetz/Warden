using System;
using Warden.Configurations;

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
        /// <returns>Instance of fluent builder for the WardenConfiguration.</returns>
        public static WardenConfiguration.Builder AddPerformanceWatcher(
            this WardenConfiguration.Builder builder, Action<WatcherHooksConfiguration.Builder> hooks = null)
        {
            builder.AddWatcher(PerformanceWatcher.Create(), hooks);

            return builder;
        }
    }
}