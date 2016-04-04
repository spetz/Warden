using System;
using Warden.Configurations;

namespace Warden.Watchers.Cpu
{
    /// <summary>
    /// Custom extension methods for the CPU watcher.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Extension method for adding the MongoDB watcher to the the WardenConfiguration with the default name of CPU Watcher.
        /// </summary>
        /// <param name="builder">Instance of the Warden configuration builder.</param>
        /// <param name="hooks">Optional lambda expression for configuring the watcher hooks.</param>
        /// <returns>Instance of fluent builder for the WardenConfiguration.</returns>
        public static WardenConfiguration.Builder AddCpuWatcher(
            this WardenConfiguration.Builder builder, Action<WatcherHooksConfiguration.Builder> hooks = null)
        {
            builder.AddWatcher(CpuWatcher.Create(), hooks);

            return builder;
        }
    }
}