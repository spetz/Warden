using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Warden.Core;

namespace Warden.Watchers.Redis
{
    /// <summary>
    /// Configuration of the RedisWatcher.
    /// </summary>
    public class RedisWatcherConfiguration
    {
        /// <summary>
        /// Connection string of the Redis server.
        /// </summary>
        public string ConnectionString { get; protected set; }

        /// <summary>
        /// Id of the Redis database. 
        /// </summary>
        public int Database { get; protected set; }

        /// <summary>
        /// Redis query.
        /// </summary>
        public string Query { get; protected set; }

        /// <summary>
        /// Optional timeout of the Redis (5 seconds by default).
        /// </summary>
        public TimeSpan Timeout { get; protected set; }

        /// <summary>
        /// Custom provider for the IRedisConnection. Input parameter is connection string.
        /// </summary>
        public Func<string, IRedisConnection> ConnectionProvider { get; protected set; }

        /// <summary>
        /// Custom provider for the IRedis.
        /// </summary>
        public Func<IRedis> RedisProvider { get; protected set; }

        /// <summary>
        /// Predicate that has to be satisfied in order to return the valid result.
        /// </summary>
        public Func<IEnumerable<dynamic>, bool> EnsureThat { get; protected set; }

        /// <summary>
        /// Async predicate that has to be satisfied in order to return the valid result.
        /// </summary>
        public Func<IEnumerable<dynamic>, Task<bool>> EnsureThatAsync { get; protected set; }

        protected internal RedisWatcherConfiguration(string connectionString, int database, TimeSpan? timeout = null)
        {
            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentException("Redis connection string can not be empty.", nameof(connectionString));
            if (database < 0)
                throw new ArgumentException("Database id can not be less than 0.", nameof(database));

            if (timeout.HasValue)
            {
                ValidateTimeout(timeout.Value);
                Timeout = timeout.Value;
            }
            else
                Timeout = TimeSpan.FromSeconds(5);

            Database = database;
            ConnectionString = connectionString;
            ConnectionProvider = redisConnectionString => new RedisConnection(redisConnectionString, Timeout);
        }

        protected static void ValidateTimeout(TimeSpan timeout)
        {
            if (timeout == null)
                throw new ArgumentNullException(nameof(timeout), "Timeout can not be null.");

            if (timeout == TimeSpan.Zero)
                throw new ArgumentException("Timeout can not be equal to zero.", nameof(timeout));
        }

        /// <summary>
        /// Factory method for creating a new instance of fluent builder for the RedisWatcherConfiguration.
        /// </summary>
        /// <param name="connectionString">Connection string of the Redis server.</param>
        /// <param name="database">Id of the Redis database.</param>
        /// <param name="timeout">Optional timeout of the Redis (5 seconds by default).</param>
        /// <returns>Instance of fluent builder for the RedisWatcherConfiguration.</returns>
        public static Builder Create(string connectionString, int database, TimeSpan? timeout = null)
            => new Builder(connectionString, database, timeout);

        /// <summary>
        /// Fluent builder for the RedisWatcherConfiguration.
        /// </summary>
        public abstract class Configurator<T> : WatcherConfigurator<T, RedisWatcherConfiguration>
            where T : Configurator<T>
        {
            protected Configurator(string connectionString, int database, TimeSpan? timeout = null)
            {
                Configuration = new RedisWatcherConfiguration(connectionString, database, timeout);
            }

            protected Configurator(RedisWatcherConfiguration configuration) : base(configuration)
            {
            }

            /// <summary>
            /// Sets the Redis query.
            /// </summary>
            /// <param name="query">Redis query.</param>
            /// <returns>Instance of fluent builder for the RedisWatcherConfiguration.</returns>
            public T WithQuery(string query)
            {
                if (string.IsNullOrEmpty(query))
                    throw new ArgumentException("Redis query can not be empty.", nameof(query));

                Configuration.Query = query;

                return Configurator;
            }


            /// <summary>
            /// Sets the predicate that has to be satisfied in order to return the valid result.
            /// </summary>
            /// <param name="ensureThat">Lambda expression predicate.</param>
            /// <returns>Instance of fluent builder for the RedisWatcherConfiguration.</returns>
            public T EnsureThat(Func<IEnumerable<dynamic>, bool> ensureThat)
            {
                if (ensureThat == null)
                    throw new ArgumentException("Ensure that predicate can not be null.", nameof(ensureThat));

                Configuration.EnsureThat = ensureThat;

                return Configurator;
            }

            /// <summary>
            /// Sets the async predicate that has to be satisfied in order to return the valid result.
            /// </summary>
            /// <param name="ensureThat">Lambda expression predicate.</param>
            /// <returns>Instance of fluent builder for the RedisWatcherConfiguration.</returns>
            public T EnsureThatAsync(Func<IEnumerable<dynamic>, Task<bool>> ensureThat)
            {
                if (ensureThat == null)
                    throw new ArgumentException("Ensure that async predicate can not be null.", nameof(ensureThat));

                Configuration.EnsureThatAsync = ensureThat;

                return Configurator;
            }

            /// <summary>
            /// Sets the custom provider for the IRedisConnection.
            /// </summary>
            /// <param name="connectionProvider">Custom provider for the IRedisConnection.</param>
            /// <returns>Lambda expression taking as an input connection string 
            /// and returning an instance of the IRedisConnection.</returns>
            /// <returns>Instance of fluent builder for the RedisWatcherConfiguration.</returns>
            public T WithConnectionProvider(Func<string, IRedisConnection> connectionProvider)
            {
                if (connectionProvider == null)
                {
                    throw new ArgumentNullException(nameof(connectionProvider),
                        "Redis connection provider can not be null.");
                }

                Configuration.ConnectionProvider = connectionProvider;

                return Configurator;
            }

            /// <summary>
            /// Sets the custom provider for the IRedis.
            /// </summary>
            /// <param name="redisProvider">Custom provider for the IRedis.</param>
            /// <returns>Lambda expression returning an instance of the IRedis.</returns>
            /// <returns>Instance of fluent builder for the RedisWatcherConfiguration.</returns>
            public T WithRedisProvider(Func<IRedis> redisProvider)
            {
                if (redisProvider == null)
                {
                    throw new ArgumentNullException(nameof(redisProvider), "Redis provider can not be null.");
                }

                Configuration.RedisProvider = redisProvider;

                return Configurator;
            }
        }

        /// <summary>
        /// Default RedisWatcherConfiguration fluent builder used while configuring watcher via lambda expression.
        /// </summary>
        public class Default : Configurator<Default>
        {
            public Default(RedisWatcherConfiguration configuration) : base(configuration)
            {
                SetInstance(this);
            }
        }

        /// <summary>
        /// Extended RedisWatcherConfiguration fluent builder used while configuring watcher directly.
        /// </summary>
        public class Builder : Configurator<Builder>
        {
            public Builder(string connectionString, int database, TimeSpan? timeout = null)
                : base(connectionString, database, timeout)
            {
                SetInstance(this);
            }

            /// <summary>
            /// Builds the RedisWatcherConfiguration and return its instance.
            /// </summary>
            /// <returns>Instance of RedisWatcherConfiguration.</returns>
            public RedisWatcherConfiguration Build() => Configuration;

            /// <summary>
            /// Operator overload to provide casting the Builder configurator into Default configurator.
            /// </summary>
            /// <param name="builder">Instance of extended Builder configurator.</param>
            /// <returns>Instance of Default builder configurator.</returns>
            public static explicit operator Default(Builder builder) => new Default(builder.Configuration);
        }
    }
}