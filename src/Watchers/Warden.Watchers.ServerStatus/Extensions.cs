namespace Warden.Watchers.ServerStatus
{
    using System;
    using global::Warden.Core;

    public static class Extensions
    {

        /// <summary>
        /// Extension method for adding the Port Availability watcher to the the WardenConfiguration with the default name of Web Watcher.
        /// Uses the default HTTP GET request.
        /// </summary>
        /// <param name="builder">Instance of the Warden configuration builder.</param>
        /// <param name="host">The host name for server..</param>
        /// <param name="hooks">Optional lambda expression for configuring the watcher hooks.</param>
        /// <returns>Instance of fluent builder for the WardenConfiguration.</returns>
        public static WardenConfiguration.Builder AddPortAvailabilityWatcher(
            this WardenConfiguration.Builder builder, string host,
            Action<WatcherHooksConfiguration.Builder> hooks = null)
        {
            builder.AddWatcher(PortAvailabilityWatcher.Create(host), hooks);

            return builder;
        }


        /// <summary>
        /// Extension method for adding the Port Availability watcher to the the WardenConfiguration..
        /// </summary>
        /// <param name="builder">Instance of the Warden configuration builder.</param>
        /// <param name="name">Name of the Port Availability watcher.</param>
        /// <param name="host">A host name of server to check.</param>
        /// <param name="hooks">Optional lambda expression for configuring the watcher hooks.</param>
        /// <returns>Instance of fluent builder for the WardenConfiguration.</returns>
        public static WardenConfiguration.Builder AddPortAvailabilityWatcher(
            this WardenConfiguration.Builder builder,
            string name, string host,
            Action<WatcherHooksConfiguration.Builder> hooks = null)
        {
            builder.AddWatcher(PortAvailabilityWatcher.Create(name, host), hooks);

            return builder;
        }

        /// <summary>
        /// Extension method for adding the Port Availability watcher to the the WardenConfiguration with the default name of Web Watcher.
        /// Uses the default HTTP GET request.
        /// </summary>
        /// <param name="builder">Instance of the Warden configuration builder.</param>
        /// <param name="host">A host name of server to check.</param>
        /// <param name="configurator">Lambda expression for configuring the WebWatcher.</param>
        /// <param name="hooks">Optional lambda expression for configuring the watcher hooks.</param>
        public static WardenConfiguration.Builder AddPortAvailabilityWatcher(
            this WardenConfiguration.Builder builder, string host,
            Action<PortAvailabilityConfiguration.Default> configurator,
            Action<WatcherHooksConfiguration.Builder> hooks = null)
        {
            builder.AddWatcher(PortAvailabilityWatcher.Create(host, configurator), hooks);

            return builder;
        }

        /// <summary>
        /// Extension method for adding the Port Availability watcher to the the WardenConfiguration.
        /// </summary>
        /// <param name="builder">Instance of the Warden configuration builder.</param>
        /// <param name="name">Name of the WebWatcher.</param>
        /// <param name="host">A host name of server to check.</param>
        /// <param name="configurator">Lambda expression for configuring the WebWatcher.</param>
        /// <param name="hooks">Optional lambda expression for configuring the watcher hooks.</param>
        /// <returns>Instance of fluent builder for the WardenConfiguration.</returns>
        public static WardenConfiguration.Builder AddPortAvailabilityWatcher(
            this WardenConfiguration.Builder builder,
            string name, string host,
            Action<PortAvailabilityConfiguration.Default> configurator,
            Action<WatcherHooksConfiguration.Builder> hooks = null)
        {
            builder.AddWatcher(PortAvailabilityWatcher.Create(name, host, configurator), hooks);

            return builder;
        }

        /// <summary>
        /// Extension method for adding the Port Availability watcher to the the WardenConfiguration.
        /// </summary>
        /// <param name="builder">Instance of the Warden configuration builder.</param>
        /// <param name="name">Name of the WebWatcher.</param>
        /// <param name="configuration">Configuration of WebWatcher.</param>
        /// <param name="hooks">Optional lambda expression for configuring the watcher hooks.</param>
        /// <returns>Instance of fluent builder for the WardenConfiguration.</returns>
        public static WardenConfiguration.Builder AddPortAvailabilityWatcher(
            this WardenConfiguration.Builder builder, string name,
            PortAvailabilityConfiguration configuration,
            Action<WatcherHooksConfiguration.Builder> hooks = null)
        {
            builder.AddWatcher(PortAvailabilityWatcher.Create(name, configuration), hooks);

            return builder;
        }


    }
}
