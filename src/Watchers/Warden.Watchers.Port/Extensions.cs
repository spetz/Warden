namespace Warden.Watchers.Port
{
    using System;
    using global::Warden.Core;

    public static class Extensions
    {
        /// <summary>
        /// Gets the host name (if possible) from url-formatted input string.
        /// </summary>
        /// <param name="input">The url formatted string.</param>
        /// <returns>The url string without protocol part. If input is of bad format, returns original input value.</returns>
        public static string GetHostname(this string input)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));

            try
            {
                var uri = new Uri(input);
                return uri.Host;
            }
            catch (UriFormatException)
            {
                return input;
            }
        }
        /// <summary>
        /// Extension method for adding the Port watcher to the the WardenConfiguration with the default name of Web Watcher.
        /// Uses the default HTTP GET request.
        /// </summary>
        /// <param name="builder">Instance of the Warden configuration builder.</param>
        /// <param name="host">The host name for server..</param>
        /// <param name="hooks">Optional lambda expression for configuring the watcher hooks.</param>
        /// <returns>Instance of fluent builder for the WardenConfiguration.</returns>
        public static WardenConfiguration.Builder AddPortWatcher(
            this WardenConfiguration.Builder builder, string host,
            Action<WatcherHooksConfiguration.Builder> hooks = null)
        {
            builder.AddWatcher(PortWatcher.Create(host), hooks);

            return builder;
        }


        /// <summary>
        /// Extension method for adding the Port watcher to the the WardenConfiguration..
        /// </summary>
        /// <param name="builder">Instance of the Warden configuration builder.</param>
        /// <param name="name">Name of the Port watcher.</param>
        /// <param name="host">A host name of server to check.</param>
        /// <param name="hooks">Optional lambda expression for configuring the watcher hooks.</param>
        /// <returns>Instance of fluent builder for the WardenConfiguration.</returns>
        public static WardenConfiguration.Builder AddPortWatcher(
            this WardenConfiguration.Builder builder,
            string name, string host,
            Action<WatcherHooksConfiguration.Builder> hooks = null)
        {
            builder.AddWatcher(PortWatcher.Create(name, host), hooks);

            return builder;
        }

        /// <summary>
        /// Extension method for adding the Port watcher to the the WardenConfiguration with the default name of Web Watcher.
        /// Uses the default HTTP GET request.
        /// </summary>
        /// <param name="builder">Instance of the Warden configuration builder.</param>
        /// <param name="host">A host name of server to check.</param>
        /// <param name="configurator">Lambda expression for configuring the PortWatcher.</param>
        /// <param name="hooks">Optional lambda expression for configuring the watcher hooks.</param>
        public static WardenConfiguration.Builder AddPortWatcher(
            this WardenConfiguration.Builder builder, string host,
            Action<PortConfiguration.Default> configurator,
            Action<WatcherHooksConfiguration.Builder> hooks = null)
        {
            builder.AddWatcher(PortWatcher.Create(host, configurator), hooks);

            return builder;
        }

        /// <summary>
        /// Extension method for adding the Port watcher to the the WardenConfiguration.
        /// </summary>
        /// <param name="builder">Instance of the Warden configuration builder.</param>
        /// <param name="name">Name of the PortWatcher.</param>
        /// <param name="host">A host name of server to check.</param>
        /// <param name="configurator">Lambda expression for configuring the PortWatcher.</param>
        /// <param name="hooks">Optional lambda expression for configuring the watcher hooks.</param>
        /// <returns>Instance of fluent builder for the WardenConfiguration.</returns>
        public static WardenConfiguration.Builder AddPortWatcher(
            this WardenConfiguration.Builder builder,
            string name, string host,
            Action<PortConfiguration.Default> configurator,
            Action<WatcherHooksConfiguration.Builder> hooks = null)
        {
            builder.AddWatcher(PortWatcher.Create(name, host, configurator), hooks);

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
            PortConfiguration configuration,
            Action<WatcherHooksConfiguration.Builder> hooks = null)
        {
            builder.AddWatcher(PortWatcher.Create(name, configuration), hooks);

            return builder;
        }


    }
}
