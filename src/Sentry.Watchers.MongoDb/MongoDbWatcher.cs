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
        private readonly MongoClient _client;
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
            _client = configuration.ClientProvider();
        }

        public async Task<IWatcherCheckResult> ExecuteAsync()
        {
            try
            {
                var database = _configuration.DatabaseProvider(_client, _configuration.Database);
                var databases = await _client.ListDatabasesAsync();
                var isValid = true;
                while (await databases.MoveNextAsync())
                {
                    isValid = databases.Current.Any(x => x["name"] == _configuration.Database);
                }

                if (!isValid)
                {
                    return WatcherCheckResult.Create(this, false,
                        $"Database: '{_configuration.Database}' has not been found.");
                }

                if (_configuration.EnsureThatAsync != null)
                    isValid = await _configuration.EnsureThatAsync?.Invoke(database);

                isValid = isValid && (_configuration.EnsureThat?.Invoke(database) ?? true);

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

        public static MongoDbWatcher Create(string name, string database, MongoClientSettings settings)
            => new MongoDbWatcher(name, MongoDbWatcherConfiguration.Create(database, settings).Build());
    }
}