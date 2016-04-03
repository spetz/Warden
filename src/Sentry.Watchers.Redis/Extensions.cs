using System;
using Sentry.Core;

namespace Sentry.Watchers.Redis
{
    /// <summary>
    /// Custom extension methods for the Redis watcher.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Extension method for adding the Redis watcher to the the SentryConfiguration with the default name of Redis Watcher.
        /// </summary>
        /// <param name="builder">Instance of the Sentry configuration builder.</param>
        /// <param name="connectionString">Connection string of the Redis database.</param>m
        /// <param name="database">Name of the Redis database.</param>
        /// <param name="timeout">Optional timeout of the Redis query (5 seconds by default).</param>
        /// <param name="hooks">Optional lambda expression for configuring the watcher hooks.</param>
        /// <returns>Instance of fluent builder for the SentryConfiguration.</returns>
        public static SentryConfiguration.Builder AddRedisWatcher(
            this SentryConfiguration.Builder builder, string connectionString, int database, 
            TimeSpan? timeout = null, Action<WatcherHooksConfiguration.Builder> hooks = null)
        {
            builder.AddWatcher(RedisWatcher.Create(connectionString, database, timeout), hooks);

            return builder;
        }

        /// <summary>
        /// Extension method for adding the Redis watcher to the the SentryConfiguration.
        /// </summary>
        /// <param name="builder">Instance of the Sentry configuration builder.</param>
        /// <param name="name">Name of the RedisWatcher.</param>
        /// <param name="connectionString">Connection string of the Redis database.</param>
        /// <param name="database">Name of the Redis database.</param>
        /// <param name="timeout">Optional timeout of the Redis query (5 seconds by default).</param>
        /// <param name="hooks">Optional lambda expression for configuring the watcher hooks.</param>
        /// <returns>Instance of fluent builder for the SentryConfiguration.</returns>
        public static SentryConfiguration.Builder AddRedisWatcher(
            this SentryConfiguration.Builder builder, string name, 
            string connectionString, int database, TimeSpan? timeout = null,
            Action<WatcherHooksConfiguration.Builder> hooks = null)
        {
            builder.AddWatcher(RedisWatcher.Create(name, connectionString, database, timeout), hooks);

            return builder;
        }

        /// <summary>
        /// Extension method for adding the Redis watcher to the the SentryConfiguration with the default name of Redis Watcher.
        /// </summary>
        /// <param name="builder">Instance of the Sentry configuration builder.</param>
        /// <param name="connectionString">Connection string of the Redis database.</param>
        /// <param name="database">Name of the Redis database.</param>
        /// <param name="configurator">Lambda expression for configuring the RedisWatcher.</param>
        /// <param name="hooks">Optional lambda expression for configuring the watcher hooks.</param>
        /// <param name="timeout">Optional timeout of the Redis query (5 seconds by default).</param>
        public static SentryConfiguration.Builder AddRedisWatcher(
            this SentryConfiguration.Builder builder,
            string connectionString, int database, 
            Action<RedisWatcherConfiguration.Default> configurator,
            Action<WatcherHooksConfiguration.Builder> hooks = null, TimeSpan? timeout = null)
        {
            builder.AddWatcher(RedisWatcher.Create(connectionString, database, timeout, configurator), hooks);

            return builder;
        }

        /// <summary>
        /// Extension method for adding the Redis watcher to the the SentryConfiguration.
        /// </summary>
        /// <param name="builder">Instance of the Sentry configuration builder.</param>
        /// <param name="name">Name of the RedisWatcher.</param>
        /// <param name="connectionString">Connection string of the Redis database.</param>
        /// <param name="database">Name of the Redis database.</param>
        /// <param name="configurator">Lambda expression for configuring the RedisWatcher.</param>
        /// <param name="hooks">Optional lambda expression for configuring the watcher hooks.</param>
        /// <param name="timeout">Optional timeout of the Redis query (5 seconds by default).</param>
        /// <returns>Instance of fluent builder for the SentryConfiguration.</returns>
        public static SentryConfiguration.Builder AddRedisWatcher(
            this SentryConfiguration.Builder builder, string name,
            string connectionString, int database,
            Action<RedisWatcherConfiguration.Default> configurator,
            Action<WatcherHooksConfiguration.Builder> hooks = null, TimeSpan? timeout = null)
        {
            builder.AddWatcher(RedisWatcher.Create(name, connectionString, database, timeout, configurator), hooks);

            return builder;
        }

        /// <summary>
        /// Extension method for adding the Redis watcher to the the SentryConfiguration with the default name of Redis Watcher.
        /// </summary>
        /// <param name="builder">Instance of the Sentry configuration builder.</param>
        /// <param name="configuration">Configuration of RedisWatcher.</param>
        /// <param name="hooks">Optional lambda expression for configuring the watcher hooks.</param>
        /// <returns>Instance of fluent builder for the SentryConfiguration.</returns>
        public static SentryConfiguration.Builder AddRedisWatcher(
            this SentryConfiguration.Builder builder,
            RedisWatcherConfiguration configuration,
            Action<WatcherHooksConfiguration.Builder> hooks = null)
        {
            builder.AddWatcher(RedisWatcher.Create(configuration), hooks);

            return builder;
        }

        /// <summary>
        /// Extension method for adding the Redis watcher to the the SentryConfiguration.
        /// </summary>
        /// <param name="builder">Instance of the Sentry configuration builder.</param>
        /// <param name="name">Name of the RedisWatcher.</param>
        /// <param name="configuration">Configuration of RedisWatcher.</param>
        /// <param name="hooks">Optional lambda expression for configuring the watcher hooks.</param>
        /// <returns>Instance of fluent builder for the SentryConfiguration.</returns>
        public static SentryConfiguration.Builder AddRedisWatcher(
            this SentryConfiguration.Builder builder, string name,
            RedisWatcherConfiguration configuration,
            Action<WatcherHooksConfiguration.Builder> hooks = null)
        {
            builder.AddWatcher(RedisWatcher.Create(name, configuration), hooks);

            return builder;
        }
    }
}