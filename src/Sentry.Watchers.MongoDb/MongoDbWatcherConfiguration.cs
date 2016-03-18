using System;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace Sentry.Watchers.MongoDb
{
    public class MongoDbWatcherConfiguration
    {
        public string Database { get; protected set; }
        public MongoClientSettings Settings { get; protected set; }
        public string ConnectionString { get; protected set; }
        public Func<IMongoDatabase, bool> EnsureThat { get; protected set; }
        public Func<IMongoDatabase, Task<bool>> EnsureThatAsync { get; protected set; }
        public TimeSpan ServerSelectionTimeout { get; protected set; }
        public TimeSpan ConnectTimeout { get; protected set; }

        protected internal MongoDbWatcherConfiguration(string database, string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("Connection string can not be empty.", nameof(connectionString));

            ValidateAndSetDatabase(database);
            ConnectionString = connectionString;
        }

        protected internal MongoDbWatcherConfiguration(string database, MongoClientSettings settings)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings), "Settings string can not be null.");

            ValidateAndSetDatabase(database);
            Settings = settings;
            ServerSelectionTimeout = settings.ServerSelectionTimeout;
            ConnectTimeout = settings.ConnectTimeout;
        }

        protected void ValidateAndSetDatabase(string database)
        {
            if (string.IsNullOrEmpty(database))
                throw new ArgumentException("Database name can not be empty.", nameof(database));

            Database = database;
        }

        public static Builder Create(string database, MongoClientSettings settings) => new Builder(database, settings);

        public static Builder Create(string database, string connectionString) => new Builder(database, connectionString);

        public class Builder
        {
            private readonly MongoDbWatcherConfiguration _configuration;

            public Builder(string database, MongoClientSettings settings)
            {
                _configuration = new MongoDbWatcherConfiguration(database, settings);
            }

            public Builder(string database, string connectionString)
            {
                _configuration = new MongoDbWatcherConfiguration(database, connectionString);
            }

            public Builder WithServerSelectionTimeout(TimeSpan timeout)
            {
                ValidateTimeout(timeout);
                _configuration.ServerSelectionTimeout = timeout;

                return this;
            }

            public Builder WithConnectTimeout(TimeSpan timeout)
            {
                ValidateTimeout(timeout);
                _configuration.ConnectTimeout = timeout;

                return this;
            }

            public Builder EnsureThat(Func<IMongoDatabase, bool> ensureThat)
            {
                if (ensureThat == null)
                    throw new ArgumentException("Ensure that predicate can not be null.", nameof(ensureThat));

                _configuration.EnsureThat = ensureThat;

                return this;
            }

            public Builder EnsureThatAsync(Func<IMongoDatabase, Task<bool>> ensureThat)
            {
                if (ensureThat == null)
                    throw new ArgumentException("Ensure that async predicate can not be null.", nameof(ensureThat));

                _configuration.EnsureThatAsync = ensureThat;

                return this;
            }

            protected void ValidateTimeout(TimeSpan timeout)
            {
                if (timeout == null)
                    throw new ArgumentNullException(nameof(timeout), "Timeout can not be null.");

                if (timeout == TimeSpan.Zero)
                    throw new ArgumentException("Timeout can not be equal to zero.", nameof(timeout));
            }

            public MongoDbWatcherConfiguration Build() => _configuration;
        }
    }
}