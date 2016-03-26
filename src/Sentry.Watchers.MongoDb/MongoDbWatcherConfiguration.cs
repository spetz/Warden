using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using Sentry.Core;

namespace Sentry.Watchers.MongoDb
{
    public class MongoDbWatcherConfiguration
    {
        public string Database { get; protected set; }
        public string ConnectionString { get; protected set; }
        public string Query { get; protected set; }
        public string QueryCollectionName { get; protected set; }
        public Func<string, IMongoDbConnection> ConnectionProvider { get; protected set; }
        public Func<IMongoDb> DatabaseProvider { get; protected set; }
        public Func<IEnumerable<dynamic>, bool> EnsureThat { get; protected set; }
        public Func<IEnumerable<dynamic>, Task<bool>> EnsureThatAsync { get; protected set; }
        public TimeSpan Timeout { get; protected set; }

        protected internal MongoDbWatcherConfiguration(string database, string connectionString,
            TimeSpan? timeout = null)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("Connection string can not be empty.", nameof(connectionString));

            ValidateAndSetDatabase(database);
            ConnectionString = connectionString;
            if (timeout.HasValue)
            {
                ValidateTimeout(timeout.Value);
                Timeout = timeout.Value;
            }
            else
                Timeout = TimeSpan.FromSeconds(5);

           ConnectionProvider = cs => new MongoDbConnection(Database, connectionString, Timeout);
        }

        protected static void ValidateTimeout(TimeSpan timeout)
        {
            if (timeout == null)
                throw new ArgumentNullException(nameof(timeout), "Timeout can not be null.");

            if (timeout == TimeSpan.Zero)
                throw new ArgumentException("Timeout can not be equal to zero.", nameof(timeout));
        }

        protected virtual MongoServerAddress GetServerAddress()
        {
            //Remove the "mongodb://" substring
            var cleanedConnectionString = ConnectionString.Substring(10);
            var hostAndPort = cleanedConnectionString.Split(':');

            return new MongoServerAddress(hostAndPort[0], int.Parse(hostAndPort[1]));
        }

        protected void ValidateAndSetDatabase(string database)
        {
            if (string.IsNullOrEmpty(database))
                throw new ArgumentException("Database name can not be empty.", nameof(database));

            Database = database;
        }

        public static Builder Create(string database, string connectionString, TimeSpan? timeout = null)
            => new Builder(database, connectionString, timeout);

        public abstract class Configurator<T> : WatcherConfigurator<T, MongoDbWatcherConfiguration>
            where T : Configurator<T>
        {

            protected Configurator(string database, string connectionString, TimeSpan? timeout = null)
            {
                Configuration = new MongoDbWatcherConfiguration(database, connectionString, timeout);
            }

            protected Configurator(MongoDbWatcherConfiguration configuration) : base(configuration)
            {
            }

            public T WithConnectionProvider(Func<string, IMongoDbConnection> connectionProvider)
            {
                if (connectionProvider == null)
                {
                    throw new ArgumentNullException(nameof(connectionProvider),
                        "MongoDB connection provider can not be null.");
                }

                Configuration.ConnectionProvider = connectionProvider;

                return Configurator;
            }

            public T WithDatabaseProvider(Func<IMongoDb> databaseProvider)
            {
                if (databaseProvider == null)
                {
                    throw new ArgumentNullException(nameof(databaseProvider),
                        "MongoDB database provider can not be null.");
                }

                Configuration.DatabaseProvider = databaseProvider;

                return Configurator;
            }

            public T WithQuery(string collectionName, string query)
            {
                if (string.IsNullOrEmpty(collectionName))
                    throw new ArgumentException("MongoDB collection name can not be empty.", nameof(collectionName));

                if (string.IsNullOrEmpty(query))
                    throw new ArgumentException("MongoDB query can not be empty.", nameof(query));

                Configuration.QueryCollectionName = collectionName;
                Configuration.Query = query;

                return Configurator;
            }

            public T EnsureThat(Func<IEnumerable<dynamic>, bool> ensureThat)
            {
                if (ensureThat == null)
                    throw new ArgumentException("Ensure that predicate can not be null.", nameof(ensureThat));

                Configuration.EnsureThat = ensureThat;

                return Configurator;
            }

            public T EnsureThatAsync(Func<IEnumerable<dynamic>, Task<bool>> ensureThat)
            {
                if (ensureThat == null)
                    throw new ArgumentException("Ensure that async predicate can not be null.", nameof(ensureThat));

                Configuration.EnsureThatAsync = ensureThat;

                return Configurator;
            }
        }

        public class Default : Configurator<Default>
        {
            public Default(MongoDbWatcherConfiguration configuration) : base(configuration)
            {
                SetInstance(this);
            }
        }

        public class Builder : Configurator<Builder>
        {
            public Builder(string database, string connectionString, TimeSpan? timeout = null)
                : base(database, connectionString, timeout)
            {
                SetInstance(this);
            }

            public MongoDbWatcherConfiguration Build() => Configuration;

            public static explicit operator Default(Builder builder) => new Default(builder.Configuration);
        }
    }
}