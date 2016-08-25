using System;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace Warden.Watchers.MongoDb
{
    /// <summary>
    /// MongoDbWatcher designed for MongoDB monitoring.
    /// </summary>
    public class MongoDbWatcher : IWatcher
    {
        private readonly MongoDbWatcherConfiguration _configuration;
        private readonly IMongoDbConnection _connection;
        public string Name { get; }
        public string Group { get; }
        public const string DefaultName = "MongoDB Watcher";

        protected MongoDbWatcher(string name, MongoDbWatcherConfiguration configuration, string group)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Watcher name can not be empty.");

            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration),
                    "MongoDB Watcher configuration has not been provided.");
            }

            Name = name;
            _configuration = configuration;
            Group = group;
            _connection = configuration.ConnectionProvider(configuration.ConnectionString);
        }

        public async Task<IWatcherCheckResult> ExecuteAsync()
        {
            try
            {
                var mongoDb = _configuration.MongoDbProvider?.Invoke() ?? await _connection.GetDatabaseAsync();
                if (mongoDb == null)
                {
                    return MongoDbWatcherCheckResult.Create(this, false, _configuration.Database,
                        _configuration.ConnectionString, $"Database: '{_configuration.Database}' has not been found.");
                }
                if (string.IsNullOrWhiteSpace(_configuration.Query))
                {
                    return MongoDbWatcherCheckResult.Create(this, true, _configuration.Database,
                        _configuration.ConnectionString,
                        $"Database: {_configuration.Database} has been sucessfully checked.");
                }

                return await ExecuteForQueryAsync(mongoDb);
            }
            catch (MongoException exception)
            {
                return MongoDbWatcherCheckResult.Create(this, false, _configuration.Database,
                    _configuration.ConnectionString, exception.Message);
            }
            catch (Exception exception)
            {
                throw new WatcherException("There was an error while trying to access the MongoDB.", exception);
            }
        }

        private async Task<IWatcherCheckResult> ExecuteForQueryAsync(IMongoDb mongoDb)
        {
            var queryResult = await mongoDb.QueryAsync(_configuration.CollectionName, _configuration.Query);
            var isValid = true;
            if (_configuration.EnsureThatAsync != null)
                isValid = await _configuration.EnsureThatAsync?.Invoke(queryResult);

            isValid = isValid && (_configuration.EnsureThat?.Invoke(queryResult) ?? true);
            var description = $"MongoDB check has returned {(isValid ? "valid" : "invalid")} result for " +
                              $"database: '{_configuration.Database}' and given query.";

            return MongoDbWatcherCheckResult.Create(this, isValid, _configuration.Database,
                _configuration.ConnectionString, _configuration.Query, queryResult, description);
        }

        /// <summary>
        /// Factory method for creating a new instance of MongoDbWatcher with default name of MongoDB Watcher.
        /// </summary>
        /// <param name="connectionString">Connection string of the MongoDB server.</param>
        /// <param name="database">Name of the MongoDB database.</param>
        /// <param name="timeout">Optional timeout of the MongoDB query (5 seconds by default).</param>
        /// <param name="configurator">Optional lambda expression for configuring the MongoDbWatcher.</param>
        /// <param name="group">Optional name of the group that MongoDbWatcher belongs to.</param>
        /// <returns>Instance of MongoDbWatcher.</returns>
        public static MongoDbWatcher Create(string connectionString, string database,
            TimeSpan? timeout = null, Action<MongoDbWatcherConfiguration.Default> configurator = null,
            string group = null)
            => Create(DefaultName, connectionString, database, timeout, configurator, group);

        /// <summary>
        /// Factory method for creating a new instance of MongoDbWatcher.
        /// </summary>
        /// <param name="name">Name of the MongoDbWatcher.</param>
        /// <param name="connectionString">Connection string of the MongoDB server.</param>
        /// <param name="database">Name of the MongoDB database.</param>
        /// <param name="timeout">Optional timeout of the MongoDB query (5 seconds by default).</param>
        /// <param name="configurator">Optional lambda expression for configuring the MongoDbWatcher.</param>
        /// <param name="group">Optional name of the group that MongoDbWatcher belongs to.</param>
        /// <returns>Instance of MongoDbWatcher.</returns>
        public static MongoDbWatcher Create(string name, string connectionString, string database,
            TimeSpan? timeout = null, Action<MongoDbWatcherConfiguration.Default> configurator = null,
            string group = null)
        {
            var config = new MongoDbWatcherConfiguration.Builder(connectionString, database, timeout);
            configurator?.Invoke((MongoDbWatcherConfiguration.Default) config);

            return Create(name, config.Build(), group);
        }

        /// <summary>
        /// Factory method for creating a new instance of MongoDbWatcher with default name of MongoDB Watcher.
        /// </summary>
        /// <param name="configuration">Configuration of MongoDbWatcher.</param>
        /// <param name="group">Optional name of the group that MongoDbWatcher belongs to.</param>
        /// <returns>Instance of MongoDbWatcher.</returns>
        public static MongoDbWatcher Create(MongoDbWatcherConfiguration configuration, 
            string group = null)
            => Create(DefaultName, configuration, group);

        /// <summary>
        /// Factory method for creating a new instance of MongoDbWatcher.
        /// </summary>
        /// <param name="name">Name of the MongoDbWatcher.</param>
        /// <param name="configuration">Configuration of MongoDbWatcher.</param>
        /// <param name="group">Optional name of the group that MongoDbWatcher belongs to.</param>
        /// <returns>Instance of MongoDbWatcher.</returns>
        public static MongoDbWatcher Create(string name, MongoDbWatcherConfiguration configuration,
            string group = null)
            => new MongoDbWatcher(name, configuration, group);
    }
}