using System;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace Warden.Watchers.Redis
{
    /// <summary>
    /// RedisWatcher designed for Redis monitoring.
    /// </summary>
    public class RedisWatcher : IWatcher
    {
        private readonly IRedisConnection _connection;
        private readonly RedisWatcherConfiguration _configuration;
        public string Name { get; }
        public string Group { get; }
        public const string DefaultName = "Redis Watcher";

        protected RedisWatcher(string name, RedisWatcherConfiguration configuration, string group)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Watcher name can not be empty.");

            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration),
                    "Redis Watcher configuration has not been provided.");
            }

            Name = name;
            _configuration = configuration;
            Group = group;
            _connection = _configuration.ConnectionProvider(_configuration.ConnectionString);
        }

        public async Task<IWatcherCheckResult> ExecuteAsync()
        {
            try
            {
                var redis = _configuration.RedisProvider?.Invoke() ?? await _connection.GetDatabaseAsync(_configuration.Database);
                if (redis == null)
                {
                    return RedisWatcherCheckResult.Create(this, false, _configuration.Database,
                        _configuration.ConnectionString, $"Database: '{_configuration.Database}' has not been found.");
                }
                if (string.IsNullOrWhiteSpace(_configuration.Query))
                {
                    return RedisWatcherCheckResult.Create(this, true, _configuration.Database,
                        _configuration.ConnectionString, 
                        $"Database: {_configuration.Database} has been sucessfully checked.");
                }

                return await ExecuteForQueryAsync(redis);
            }
            catch (RedisException exception)
            {
                return RedisWatcherCheckResult.Create(this, false, _configuration.Database,
                    _configuration.ConnectionString, exception.Message);
            }
            catch (Exception exception)
            {
                throw new WatcherException("There was an error while trying to access the Redis.", exception);
            }
        }

        private async Task<IWatcherCheckResult> ExecuteForQueryAsync(IRedis redis)
        {
            var queryResult = await redis.QueryAsync(_configuration.Query);
            var isValid = true;
            if (_configuration.EnsureThatAsync != null)
                isValid = await _configuration.EnsureThatAsync?.Invoke(queryResult);

            isValid = isValid && (_configuration.EnsureThat?.Invoke(queryResult) ?? true);
            var description = $"Redis check has returned {(isValid ? "valid" : "invalid")} result for " +
                              $"database: '{_configuration.Database}' and given query.";

            return RedisWatcherCheckResult.Create(this, isValid, _configuration.Database,
                _configuration.ConnectionString, _configuration.Query, queryResult, description);
        }

        /// <summary>
        /// Factory method for creating a new instance of RedisWatcher with default name of Redis Watcher.
        /// </summary>
        /// <param name="connectionString">Connection string of the Redis server.</param>
        /// <param name="database">Id of the Redis database.</param>
        /// <param name="timeout">Optional timeout of the Redis query (5 seconds by default).</param>
        /// <param name="configurator">Optional lambda expression for configuring the RedisWatcher.</param>
        /// <param name="group">Optional name of the group that param belongs to.</param>
        /// <returns>Instance of RedisWatcher.</returns>
        public static RedisWatcher Create(string connectionString, int database,
            TimeSpan? timeout = null, Action<RedisWatcherConfiguration.Default> configurator = null,
            string group = null)
            => Create(DefaultName, connectionString, database, timeout, configurator, group);

        /// <summary>
        /// Factory method for creating a new instance of RedisWatcher.
        /// </summary>
        /// <param name="name">Name of the RedisWatcher.</param>
        /// <param name="connectionString">Connection string of the Redis server.</param>
        /// <param name="database">Id of the Redis database.</param>
        /// <param name="timeout">Optional timeout of the Redis query (5 seconds by default).</param>
        /// <param name="configurator">Optional lambda expression for configuring the RedisWatcher.</param>
        /// <param name="group">Optional name of the group that param belongs to.</param>
        /// <returns>Instance of RedisWatcher.</returns>
        public static RedisWatcher Create(string name, string connectionString, int database,
            TimeSpan? timeout = null, Action<RedisWatcherConfiguration.Default> configurator = null,
            string group = null)
        {
            var config = new RedisWatcherConfiguration.Builder(connectionString, database, timeout);
            configurator?.Invoke((RedisWatcherConfiguration.Default)config);

            return Create(name, config.Build(), group);
        }

        /// <summary>
        /// Factory method for creating a new instance of RedisWatcher with default name of Redis Watcher.
        /// </summary>
        /// <param name="configuration">Configuration of RedisWatcher.</param>
        /// <param name="group">Optional name of the group that param belongs to.</param>
        /// <returns>Instance of RedisWatcher.</returns>
        public static RedisWatcher Create(RedisWatcherConfiguration configuration, string group = null)
            => Create(DefaultName, configuration, group);

        /// <summary>
        /// Factory method for creating a new instance of RedisWatcher.
        /// </summary>
        /// <param name="name">Name of the RedisWatcher.</param>
        /// <param name="configuration">Configuration of RedisWatcher.</param>
        /// <param name="group">Optional name of the group that param belongs to.</param>
        /// <returns>Instance of RedisWatcher.</returns>
        public static RedisWatcher Create(string name, RedisWatcherConfiguration configuration, string group = null)
            => new RedisWatcher(name, configuration, group);
    }
}