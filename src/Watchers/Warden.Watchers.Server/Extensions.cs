using System;
using Warden.Core;

namespace Warden.Watchers.Server
{
    /// <summary>
    /// Custom extension methods for the Server watcher.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Gets the hostname (if possible) from url-formatted input string.
        /// </summary>
        /// <param name="hostname">The URL formatted string.</param>
        /// <returns>The URL string without protocol part. If URL has invalid format, returns original input value.</returns>
        internal static string GetHostname(this string hostname)
        {
            if (hostname == null)
                throw new ArgumentNullException(nameof(hostname));

            try
            {
                var uri = new Uri(hostname);

                return uri.Host;
            }
            catch (UriFormatException)
            {
                return hostname;
            }
        }

        /// <summary>
        /// Validates the hostname.
        /// </summary>
        /// <param name="hostname">The URL formatted string.</param>
        /// <returns>The URL string without protocol part. If URL has invalid format an exception is thrown.</returns>
        internal static void ValidateHostname(this string hostname)
        {
            if (string.IsNullOrEmpty(hostname))
                throw new ArgumentException("Hostname can not be empty.", nameof(hostname));

            if (hostname.Contains("://"))
            {
                throw new ArgumentException("The hostname should not contain protocol. " +
                                            $"Did you mean \" {hostname.GetHostname()}\"?", nameof(hostname));
            }
        }

        /// <summary>
        /// Extension method for adding the Server watcher to the the WardenConfiguration with the default name of Server Watcher.
        /// </summary>
        /// <param name="builder">Instance of the Warden configuration builder.</param>
        /// <param name="hostname">Hostname to be resolved.</param>
        /// <param name="port">Optional port number of the hostname (0 means not specified).</param>
        /// <param name="hooks">Optional lambda expression for configuring the watcher hooks.</param>
        /// <param name="interval">Optional interval (5 seconds by default) after which the next check will be invoked.</param>
        /// <param name="group">Optional name of the group that ServerWatcher belongs to.</param>
        /// <returns>Instance of fluent builder for the WardenConfiguration.</returns>
        public static WardenConfiguration.Builder AddServerWatcher(
            this WardenConfiguration.Builder builder,
            string hostname,
            int port = 0,
            Action<WatcherHooksConfiguration.Builder> hooks = null,
            TimeSpan? interval = null,
            string group = null)
        {
            builder.AddWatcher(ServerWatcher.Create(hostname, port, group: group), hooks, interval);

            return builder;
        }

        /// <summary>
        /// Extension method for adding the Server watcher to the the WardenConfiguration..
        /// </summary>
        /// <param name="builder">Instance of the Warden configuration builder.</param>
        /// <param name="name">Name of the Server watcher.</param>
        /// <param name="hostname">Hostname to be resolved.</param>
        /// <param name="port">Optional port number of the hostname (0 means not specified).</param>
        /// <param name="hooks">Optional lambda expression for configuring the watcher hooks.</param>
        /// <param name="interval">Optional interval (5 seconds by default) after which the next check will be invoked.</param>
        /// <param name="group">Optional name of the group that ServerWatcher belongs to.</param>
        /// <returns>Instance of fluent builder for the WardenConfiguration.</returns>
        public static WardenConfiguration.Builder AddServerWatcher(
            this WardenConfiguration.Builder builder,
            string name,
            string hostname,
            int port = 0,
            Action<WatcherHooksConfiguration.Builder> hooks = null,
            TimeSpan? interval = null,
            string group = null)
        {
            builder.AddWatcher(ServerWatcher.Create(name, hostname, port, group: group), hooks, interval);

            return builder;
        }

        /// <summary>
        /// Extension method for adding the Server watcher to the the WardenConfiguration with the default name of Server watcher.
        /// </summary>
        /// <param name="builder">Instance of the Warden configuration builder.</param>
        /// <param name="hostname">Hostname to be resolved.</param>
        /// <param name="configurator">Lambda expression for configuring the ServerWatcher.</param>
        /// <param name="hooks">Optional lambda expression for configuring the watcher hooks.</param>
        /// <param name="port">Optional port number of the hostname (0 means not specified).</param>
        /// <param name="interval">Optional interval (5 seconds by default) after which the next check will be invoked.</param>
        /// <param name="group">Optional name of the group that ServerWatcher belongs to.</param>
        /// <returns>Instance of fluent builder for the WardenConfiguration.</returns> 
        public static WardenConfiguration.Builder AddServerWatcher(
            this WardenConfiguration.Builder builder,
            string hostname,
            Action<ServerWatcherConfiguration.Default> configurator,
            Action<WatcherHooksConfiguration.Builder> hooks = null,
            int port = 0,
            TimeSpan? interval = null,
            string group = null)
        {
            builder.AddWatcher(ServerWatcher.Create(hostname, port, configurator, group),
                hooks, interval);

            return builder;
        }

        /// <summary>
        /// Extension method for adding the Server watcher to the the WardenConfiguration.
        /// </summary>
        /// <param name="builder">Instance of the Warden configuration builder.</param>
        /// <param name="name">Name of the ServerWatcher.</param>
        /// <param name="hostname">Hostname to be resolved.</param>
        /// <param name="configurator">Lambda expression for configuring the ServerWatcher.</param>
        /// <param name="hooks">Optional lambda expression for configuring the watcher hooks.</param>
        /// <param name="port">Optional port number of the hostname (0 means not specified).</param>
        /// <param name="interval">Optional interval (5 seconds by default) after which the next check will be invoked.</param>
        /// <param name="group">Optional name of the group that ServerWatcher belongs to.</param>
        /// <returns>Instance of fluent builder for the WardenConfiguration.</returns>
        public static WardenConfiguration.Builder AddServerWatcher(
            this WardenConfiguration.Builder builder,
            string name,
            string hostname,
            Action<ServerWatcherConfiguration.Default> configurator,
            Action<WatcherHooksConfiguration.Builder> hooks = null,
            int port = 0,
            TimeSpan? interval = null,
            string group = null)
        {
            builder.AddWatcher(ServerWatcher.Create(name, hostname, port, configurator, group),
                hooks, interval);

            return builder;
        }

        /// <summary>
        /// Extension method for adding the Server watcher to the the WardenConfiguration.
        /// </summary>
        /// <param name="builder">Instance of the Warden configuration builder.</param>
        /// <param name="name">Name of the ServerWatcher.</param>
        /// <param name="configuration">Configuration of ServerWatcher.</param>
        /// <param name="hooks">Optional lambda expression for configuring the watcher hooks.</param>
        /// <param name="interval">Optional interval (5 seconds by default) after which the next check will be invoked.</param>
        /// <param name="group">Optional name of the group that ServerWatcher belongs to.</param>
        /// <returns>Instance of fluent builder for the WardenConfiguration.</returns>
        public static WardenConfiguration.Builder AddServerWatcher(
            this WardenConfiguration.Builder builder,
            string name,
            ServerWatcherConfiguration configuration,
            Action<WatcherHooksConfiguration.Builder> hooks = null,
            TimeSpan? interval = null,
            string group = null)
        {
            builder.AddWatcher(ServerWatcher.Create(name, configuration, group), hooks, interval);

            return builder;
        }
    }
}
