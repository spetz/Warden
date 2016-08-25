using System;

using Warden.Core;

namespace Warden.Watchers.Redis
{
    /// <summary>
    /// Custom extension methods for the Redis watcher.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Extension method for adding the Redis watcher to the the WardenConfiguration with the default name of Redis Watcher.
        /// </summary>
        /// <param name="builder">Instance of the Warden configuration builder.</param>
        /// <param name="connectionString">Connection string of the Redis database.</param>m
        /// <param name="database">Name of the Redis database.</param>
        /// <param name="timeout">Optional timeout of the Redis query (5 seconds by default).</param>
        /// <param name="hooks">Optional lambda expression for configuring the watcher hooks.</param>
        /// <param name="interval">Optional interval (5 seconds by default) after which the next check will be invoked.</param>
        /// <param name="group">Optional name of the group that param belongs to.</param>
        /// <returns>Instance of fluent builder for the WardenConfiguration.</returns>
        public static WardenConfiguration.Builder AddRedisWatcher(
            this WardenConfiguration.Builder builder,
            string connectionString,
            int database,
            TimeSpan? timeout = null,
            Action<WatcherHooksConfiguration.Builder> hooks = null,
            TimeSpan? interval = null,
            string group = null)
        {
            builder.AddWatcher(RedisWatcher.Create(connectionString, database, timeout, group: group),
                hooks, interval);

            return builder;
        }

        /// <summary>
        /// Extension method for adding the Redis watcher to the the WardenConfiguration.
        /// </summary>
        /// <param name="builder">Instance of the Warden configuration builder.</param>
        /// <param name="name">Name of the RedisWatcher.</param>
        /// <param name="connectionString">Connection string of the Redis database.</param>
        /// <param name="database">Name of the Redis database.</param>
        /// <param name="timeout">Optional timeout of the Redis query (5 seconds by default).</param>
        /// <param name="hooks">Optional lambda expression for configuring the watcher hooks.</param>
        /// <param name="interval">Optional interval (5 seconds by default) after which the next check will be invoked.</param>
        /// <param name="group">Optional name of the group that param belongs to.</param>
        /// <returns>Instance of fluent builder for the WardenConfiguration.</returns>
        public static WardenConfiguration.Builder AddRedisWatcher(
            this WardenConfiguration.Builder builder,
            string name,
            string connectionString,
            int database,
            TimeSpan? timeout = null,
            Action<WatcherHooksConfiguration.Builder> hooks = null,
            TimeSpan? interval = null,
            string group = null)
        {
            builder.AddWatcher(RedisWatcher.Create(name, connectionString, database, timeout, group: group),
                hooks, interval);

            return builder;
        }

        /// <summary>
        /// Extension method for adding the Redis watcher to the the WardenConfiguration with the default name of Redis Watcher.
        /// </summary>
        /// <param name="builder">Instance of the Warden configuration builder.</param>
        /// <param name="connectionString">Connection string of the Redis database.</param>
        /// <param name="database">Name of the Redis database.</param>
        /// <param name="configurator">Lambda expression for configuring the RedisWatcher.</param>
        /// <param name="hooks">Optional lambda expression for configuring the watcher hooks.</param>
        /// <param name="timeout">Optional timeout of the Redis query (5 seconds by default).</param>
        /// <param name="interval">Optional interval (5 seconds by default) after which the next check will be invoked.</param>
        /// <param name="group">Optional name of the group that param belongs to.</param>
        /// <returns>Instance of fluent builder for the WardenConfiguration.</returns>
        public static WardenConfiguration.Builder AddRedisWatcher(
            this WardenConfiguration.Builder builder,
            string connectionString,
            int database,
            Action<RedisWatcherConfiguration.Default> configurator,
            Action<WatcherHooksConfiguration.Builder> hooks = null,
            TimeSpan? timeout = null,
            TimeSpan? interval = null,
            string group = null)
        {
            builder.AddWatcher(RedisWatcher.Create(connectionString, database, timeout, configurator, group),
                hooks, interval);

            return builder;
        }

        /// <summary>
        /// Extension method for adding the Redis watcher to the the WardenConfiguration.
        /// </summary>
        /// <param name="builder">Instance of the Warden configuration builder.</param>
        /// <param name="name">Name of the RedisWatcher.</param>
        /// <param name="connectionString">Connection string of the Redis database.</param>
        /// <param name="database">Name of the Redis database.</param>
        /// <param name="configurator">Lambda expression for configuring the RedisWatcher.</param>
        /// <param name="hooks">Optional lambda expression for configuring the watcher hooks.</param>
        /// <param name="timeout">Optional timeout of the Redis query (5 seconds by default).</param>
        /// <param name="interval">Optional interval (5 seconds by default) after which the next check will be invoked.</param>
        /// <param name="group">Optional name of the group that param belongs to.</param>
        /// <returns>Instance of fluent builder for the WardenConfiguration.</returns>
        public static WardenConfiguration.Builder AddRedisWatcher(
            this WardenConfiguration.Builder builder,
            string name,
            string connectionString,
            int database,
            Action<RedisWatcherConfiguration.Default> configurator,
            Action<WatcherHooksConfiguration.Builder> hooks = null,
            TimeSpan? timeout = null,
            TimeSpan? interval = null,
            string group = null)
        {
            builder.AddWatcher(RedisWatcher.Create(name, connectionString, database, timeout, configurator, group),
                hooks, interval);

            return builder;
        }

        /// <summary>
        /// Extension method for adding the Redis watcher to the the WardenConfiguration with the default name of Redis Watcher.
        /// </summary>
        /// <param name="builder">Instance of the Warden configuration builder.</param>
        /// <param name="configuration">Configuration of RedisWatcher.</param>
        /// <param name="hooks">Optional lambda expression for configuring the watcher hooks.</param>
        /// <param name="interval">Optional interval (5 seconds by default) after which the next check will be invoked.</param>
        /// <param name="group">Optional name of the group that param belongs to.</param>
        /// <returns>Instance of fluent builder for the WardenConfiguration.</returns>
        public static WardenConfiguration.Builder AddRedisWatcher(
            this WardenConfiguration.Builder builder,
            RedisWatcherConfiguration configuration,
            Action<WatcherHooksConfiguration.Builder> hooks = null,
            TimeSpan? interval = null,
            string group = null)
        {
            builder.AddWatcher(RedisWatcher.Create(configuration, group), hooks, interval);

            return builder;
        }

        /// <summary>
        /// Extension method for adding the Redis watcher to the the WardenConfiguration.
        /// </summary>
        /// <param name="builder">Instance of the Warden configuration builder.</param>
        /// <param name="name">Name of the RedisWatcher.</param>
        /// <param name="configuration">Configuration of RedisWatcher.</param>
        /// <param name="hooks">Optional lambda expression for configuring the watcher hooks.</param>
        /// <param name="interval">Optional interval (5 seconds by default) after which the next check will be invoked.</param>
        /// <param name="group">Optional name of the group that param belongs to.</param>
        /// <returns>Instance of fluent builder for the WardenConfiguration.</returns>
        public static WardenConfiguration.Builder AddRedisWatcher(
            this WardenConfiguration.Builder builder,
            string name,
            RedisWatcherConfiguration configuration,
            Action<WatcherHooksConfiguration.Builder> hooks = null,
            TimeSpan? interval = null,
            string group = null)
        {
            builder.AddWatcher(RedisWatcher.Create(name, configuration, group), hooks, interval);

            return builder;
        }
    }
}