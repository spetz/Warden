using System;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace Sentry.Watchers.MongoDb
{
    public class MongoDbWatcherConfiguration
    {
        public string ConnectionString { get; protected set; }
        public string Database { get; protected set; }
        public Func<IMongoDatabase, bool> EnsureThat { get; protected set; }
        public Func<IMongoDatabase, Task<bool>> EnsureThatAsync { get; protected set; }
        public TimeSpan Timeout { get; protected set; }

        protected internal MongoDbWatcherConfiguration(string connectionString, string database)
        {
            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentException("Connection string can not be empty.", nameof(connectionString));
            if (string.IsNullOrEmpty(database))
                throw new ArgumentException("Database name can not be empty.", nameof(connectionString));

            ConnectionString = connectionString;
            Database = database;
        }

        public static Builder Create(string connectionString, string database) => new Builder(connectionString, database);

        public class Builder
        {
            private readonly MongoDbWatcherConfiguration _configuration;

            public Builder(string connectionString, string database)
            {
                _configuration = new MongoDbWatcherConfiguration(connectionString, database);
            }

            public Builder WithTimeout(TimeSpan timeout)
            {
                if (timeout == null)
                    throw new ArgumentNullException(nameof(timeout), "Timeout can not be null.");

                if (timeout == TimeSpan.Zero)
                    throw new ArgumentException("Timeout can not be equal to zero.", nameof(timeout));

                _configuration.Timeout = timeout;

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

            public MongoDbWatcherConfiguration Build() => _configuration;
        }
    }
}