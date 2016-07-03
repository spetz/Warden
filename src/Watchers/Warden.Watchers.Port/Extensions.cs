using System;
using Warden.Core;

namespace Warden.Watchers.Port
{
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
        /// Extension method for adding the Port watcher to the the WardenConfiguration with the default name of Port Watcher.
        /// </summary>
        /// <param name="builder">Instance of the Warden configuration builder.</param>
        /// <param name="hostname">Hostname to be resolved.</param>
        /// <param name="port">Port number of the hostname.</param>
        /// <param name="hooks">Optional lambda expression for configuring the watcher hooks.</param>
        /// <returns>Instance of fluent builder for the WardenConfiguration.</returns>
        public static WardenConfiguration.Builder AddPortWatcher(
            this WardenConfiguration.Builder builder, string hostname, int port,
            Action<WatcherHooksConfiguration.Builder> hooks = null)
        {
            builder.AddWatcher(PortWatcher.Create(hostname, port), hooks);

            return builder;
        }

        /// <summary>
        /// Extension method for adding the Port watcher to the the WardenConfiguration..
        /// </summary>
        /// <param name="builder">Instance of the Warden configuration builder.</param>
        /// <param name="name">Name of the Port watcher.</param>
        /// <param name="hostname">Hostname to be resolved.</param>
        /// <param name="port">Port number of the hostname.</param>
        /// <param name="hooks">Optional lambda expression for configuring the watcher hooks.</param>
        /// <returns>Instance of fluent builder for the WardenConfiguration.</returns>
        public static WardenConfiguration.Builder AddPortWatcher(
            this WardenConfiguration.Builder builder,
            string name, string hostname, int port,
            Action<WatcherHooksConfiguration.Builder> hooks = null)
        {
            builder.AddWatcher(PortWatcher.Create(name, hostname, port), hooks);

            return builder;
        }

        /// <summary>
        /// Extension method for adding the Port watcher to the the WardenConfiguration with the default name of Port Watcher.
        /// </summary>
        /// <param name="builder">Instance of the Warden configuration builder.</param>
        /// <param name="hostname">Hostname to be resolved.</param>
        /// <param name="port">Port number of the hostname.</param>
        /// <param name="configurator">Lambda expression for configuring the PortWatcher.</param>
        /// <param name="hooks">Optional lambda expression for configuring the watcher hooks.</param>
        public static WardenConfiguration.Builder AddPortWatcher(
            this WardenConfiguration.Builder builder, string hostname, int port,
            Action<PortWatcherConfiguration.Default> configurator,
            Action<WatcherHooksConfiguration.Builder> hooks = null)
        {
            builder.AddWatcher(PortWatcher.Create(hostname, port, configurator), hooks);

            return builder;
        }

        /// <summary>
        /// Extension method for adding the Port watcher to the the WardenConfiguration.
        /// </summary>
        /// <param name="builder">Instance of the Warden configuration builder.</param>
        /// <param name="name">Name of the PortWatcher.</param>
        /// <param name="hostname">Hostname to be resolved.</param>
        /// <param name="port">Port number of the hostname.</param>
        /// <param name="configurator">Lambda expression for configuring the PortWatcher.</param>
        /// <param name="hooks">Optional lambda expression for configuring the watcher hooks.</param>
        /// <returns>Instance of fluent builder for the WardenConfiguration.</returns>
        public static WardenConfiguration.Builder AddPortWatcher(
            this WardenConfiguration.Builder builder,
            string name, string hostname, int port,
            Action<PortWatcherConfiguration.Default> configurator,
            Action<WatcherHooksConfiguration.Builder> hooks = null)
        {
            builder.AddWatcher(PortWatcher.Create(name, hostname, port, configurator), hooks);

            return builder;
        }

        /// <summary>
        /// Extension method for adding the Port watcher to the the WardenConfiguration.
        /// </summary>
        /// <param name="builder">Instance of the Warden configuration builder.</param>
        /// <param name="name">Name of the PortWatcher.</param>
        /// <param name="configuration">Configuration of PortWatcher.</param>
        /// <param name="hooks">Optional lambda expression for configuring the watcher hooks.</param>
        /// <returns>Instance of fluent builder for the WardenConfiguration.</returns>
        public static WardenConfiguration.Builder AddPortWatcher(
            this WardenConfiguration.Builder builder, string name,
            PortWatcherConfiguration configuration,
            Action<WatcherHooksConfiguration.Builder> hooks = null)
        {
            builder.AddWatcher(PortWatcher.Create(name, configuration), hooks);

            return builder;
        }
    }
}
