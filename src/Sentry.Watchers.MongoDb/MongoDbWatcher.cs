using System;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using Sentry.Core;

namespace Sentry.Watchers.MongoDb
{
    public class MongoDbWatcher : IWatcher
    {
        private readonly MongoDbWatcherConfiguration _configuration;
        private readonly IMongoDbConnection _connection;
        public string Name { get; }

        protected MongoDbWatcher(string name, MongoDbWatcherConfiguration configuration)
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
            _connection = configuration.ConnectionProvider(configuration.ConnectionString);
        }

        public async Task<IWatcherCheckResult> ExecuteAsync()
        {
            try
            {
                var database = await _connection.GetDatabaseAsync() ?? _configuration.DatabaseProvider();
                if (database == null)
                {
                    return WatcherCheckResult.Create(this, false,
                        $"Database: '{_configuration.Database}' has not been found.");
                }
                if (string.IsNullOrWhiteSpace(_configuration.Query))
                {
                    return WatcherCheckResult.Create(this, true);
                }

                var queryResult = await database.QueryAsync(_connection, _configuration.QueryCollectionName, _configuration.Query);
                var isValid = true;
                if (_configuration.EnsureThatAsync != null)
                    isValid = await _configuration.EnsureThatAsync?.Invoke(queryResult);

                isValid = isValid && (_configuration.EnsureThat?.Invoke(queryResult) ?? true);

                return WatcherCheckResult.Create(this, isValid);
            }
            catch (MongoException ex)
            {
                return WatcherCheckResult.Create(this, false, ex.Message);
            }
            catch (Exception ex)
            {
                throw new WatcherException("There was an error while trying to access the MongoDB.", ex);
            }
        }

        public static MongoDbWatcher Create(string name, string connectionString, string database,
            Action<MongoDbWatcherConfiguration.Default> configurator = null)
        {
            var config = new MongoDbWatcherConfiguration.Builder(connectionString, database);
            configurator?.Invoke((MongoDbWatcherConfiguration.Default)config);

            return Create(name, config.Build());
        }

        public static MongoDbWatcher Create(string name, MongoDbWatcherConfiguration configuration)
            => new MongoDbWatcher(name, configuration);

        //public static MongoDbWatcher Create(string name, string database, MongoClientSettings settings)
        //    => new MongoDbWatcher(name, MongoDbWatcherConfiguration.Create(database, settings).Build());
    }
}