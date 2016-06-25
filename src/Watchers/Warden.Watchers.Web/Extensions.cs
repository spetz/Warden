using System;
using Warden.Core;

namespace Warden.Watchers.Web
{
    /// <summary>
    /// Custom extension methods for the Web watcher.
    /// </summary>
    public static class Extensions
    {
        internal static string GetFullUrl(this IHttpRequest request, string baseUrl)
        {
            var endpoint = request.Endpoint;
            if (string.IsNullOrWhiteSpace(endpoint))
                return baseUrl;

            if (baseUrl.EndsWith("/"))
                return $"{baseUrl}{(endpoint.StartsWith("/") ? endpoint.Substring(1) : $"{endpoint}")}";

            return $"{baseUrl}{(endpoint.StartsWith("/") ? endpoint : $"/{endpoint}")}";
        }

        /// <summary>
        /// Extension method for adding the Web watcher to the the WardenConfiguration with the default name of Web Watcher.
        /// Uses the default HTTP GET request.
        /// </summary>
        /// <param name="builder">Instance of the Warden configuration builder.</param>
        /// <param name="url">Base URL of the request.</param>
        /// <param name="hooks">Optional lambda expression for configuring the watcher hooks.</param>
        /// <returns>Instance of fluent builder for the WardenConfiguration.</returns>
        public static WardenConfiguration.Builder AddWebWatcher(
            this WardenConfiguration.Builder builder, string url,
            Action<WatcherHooksConfiguration.Builder> hooks = null)
        {
            builder.AddWatcher(WebWatcher.Create(url), hooks);

            return builder;
        }

        /// <summary>
        /// Extension method for adding the Web watcher to the the WardenConfiguration with the default name of Web Watcher.
        /// </summary>
        /// <param name="builder">Instance of the Warden configuration builder.</param>
        /// <param name="url">Base URL of the request.</param>
        /// <param name="request">Instance of the IHttpRequest.</param>
        /// <param name="hooks">Optional lambda expression for configuring the watcher hooks.</param>
        /// <returns>Instance of fluent builder for the WardenConfiguration.</returns>
        public static WardenConfiguration.Builder AddWebWatcher(
            this WardenConfiguration.Builder builder, string url, IHttpRequest request,
            Action<WatcherHooksConfiguration.Builder> hooks = null)
        {
            builder.AddWatcher(WebWatcher.Create(url, request), hooks);

            return builder;
        }

        /// <summary>
        /// Extension method for adding the Web watcher to the the WardenConfiguration..
        /// </summary>
        /// <param name="builder">Instance of the Warden configuration builder.</param>
        /// <param name="name">Name of the WebWatcher.</param>
        /// <param name="url">Base URL of the request.</param>
        /// <param name="request">Instance of the IHttpRequest.</param>
        /// <param name="hooks">Optional lambda expression for configuring the watcher hooks.</param>
        /// <returns>Instance of fluent builder for the WardenConfiguration.</returns>
        public static WardenConfiguration.Builder AddWebWatcher(
            this WardenConfiguration.Builder builder,
            string name, string url, IHttpRequest request,
            Action<WatcherHooksConfiguration.Builder> hooks = null)
        {
            builder.AddWatcher(WebWatcher.Create(name, url, request), hooks);

            return builder;
        }

        /// <summary>
        /// Extension method for adding the Web watcher to the the WardenConfiguration with the default name of Web Watcher.
        /// Uses the default HTTP GET request.
        /// </summary>
        /// <param name="builder">Instance of the Warden configuration builder.</param>
        /// <param name="url">Base URL of the request.</param>
        /// <param name="configurator">Lambda expression for configuring the WebWatcher.</param>
        /// <param name="hooks">Optional lambda expression for configuring the watcher hooks.</param>
        public static WardenConfiguration.Builder AddWebWatcher(
            this WardenConfiguration.Builder builder, string url,
            Action<WebWatcherConfiguration.Default> configurator,
            Action<WatcherHooksConfiguration.Builder> hooks = null)
        {
            builder.AddWatcher(WebWatcher.Create(url, configurator), hooks);

            return builder;
        }

        /// <summary>
        /// Extension method for adding the Web watcher to the the WardenConfiguration with the default name of Web Watcher.
        /// </summary>
        /// <param name="builder">Instance of the Warden configuration builder.</param>
        /// <param name="url">Base URL of the request.</param>
        /// <param name="request">Instance of the IHttpRequest.</param>
        /// <param name="configurator">Lambda expression for configuring the WebWatcher.</param>
        /// <param name="hooks">Optional lambda expression for configuring the watcher hooks.</param>
        /// <returns>Instance of fluent builder for the WardenConfiguration.</returns>
        public static WardenConfiguration.Builder AddWebWatcher(
            this WardenConfiguration.Builder builder, string url, IHttpRequest request,
            Action<WebWatcherConfiguration.Default> configurator,
            Action<WatcherHooksConfiguration.Builder> hooks = null)
        {
            builder.AddWatcher(WebWatcher.Create(url, request, configurator), hooks);

            return builder;
        }

        /// <summary>
        /// Extension method for adding the Web watcher to the the WardenConfiguration.
        /// </summary>
        /// <param name="builder">Instance of the Warden configuration builder.</param>
        /// <param name="name">Name of the WebWatcher.</param>
        /// <param name="url">Base URL of the request.</param>
        /// <param name="request">Instance of the IHttpRequest.</param>
        /// <param name="configurator">Lambda expression for configuring the WebWatcher.</param>
        /// <param name="hooks">Optional lambda expression for configuring the watcher hooks.</param>
        /// <returns>Instance of fluent builder for the WardenConfiguration.</returns>
        public static WardenConfiguration.Builder AddWebWatcher(
            this WardenConfiguration.Builder builder,
            string name, string url, IHttpRequest request,
            Action<WebWatcherConfiguration.Default> configurator,
            Action<WatcherHooksConfiguration.Builder> hooks = null)
        {
            builder.AddWatcher(WebWatcher.Create(name, url, request, configurator), hooks);

            return builder;
        }

        /// <summary>
        /// Extension method for adding the Web watcher to the the WardenConfiguration with the default name of Web Watcher.
        /// </summary>
        /// <param name="builder">Instance of the Warden configuration builder.</param>
        /// <param name="configuration">Configuration of WebWatcher.</param>
        /// <param name="hooks">Optional lambda expression for configuring the watcher hooks.</param>
        /// <returns>Instance of fluent builder for the WardenConfiguration.</returns>
        public static WardenConfiguration.Builder AddWebWatcher(
            this WardenConfiguration.Builder builder,
            WebWatcherConfiguration configuration,
            Action<WatcherHooksConfiguration.Builder> hooks = null)
        {
            builder.AddWatcher(WebWatcher.Create(configuration), hooks);

            return builder;
        }

        /// <summary>
        /// Extension method for adding the Web watcher to the the WardenConfiguration.
        /// </summary>
        /// <param name="builder">Instance of the Warden configuration builder.</param>
        /// <param name="name">Name of the WebWatcher.</param>
        /// <param name="configuration">Configuration of WebWatcher.</param>
        /// <param name="hooks">Optional lambda expression for configuring the watcher hooks.</param>
        /// <returns>Instance of fluent builder for the WardenConfiguration.</returns>
        public static WardenConfiguration.Builder AddWebWatcher(
            this WardenConfiguration.Builder builder, string name,
            WebWatcherConfiguration configuration,
            Action<WatcherHooksConfiguration.Builder> hooks = null)
        {
            builder.AddWatcher(WebWatcher.Create(name, configuration), hooks);

            return builder;
        }
    }
}