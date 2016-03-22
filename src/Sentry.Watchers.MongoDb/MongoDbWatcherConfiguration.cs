using System;
using System.Threading.Tasks;
using MongoDB.Driver;
using Sentry.Core;

namespace Sentry.Watchers.MongoDb
{
    public class MongoDbWatcherConfiguration
    {
        public string Database { get; protected set; }
        public MongoClientSettings Settings { get; protected set; }
        public Func<MongoClient> ClientProvider { get; protected set; }
        public string ConnectionString { get; protected set; }
        public Func<MongoClient, string, IMongoDatabase> DatabaseProvider { get; protected set; }
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
            ServerSelectionTimeout = TimeSpan.FromSeconds(10);
            ConnectTimeout = TimeSpan.FromSeconds(10);
            ClientProvider = () => new MongoClient(connectionString);
            DatabaseProvider = (client, name) => ClientProvider().GetDatabase(name);
        }

        protected internal MongoDbWatcherConfiguration(string database, MongoClientSettings settings)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings), "Settings string can not be null.");

            ValidateAndSetDatabase(database);
            Settings = settings;
            ServerSelectionTimeout = settings.ServerSelectionTimeout;
            ConnectTimeout = settings.ConnectTimeout;
            ClientProvider = () => new MongoClient(InitializeSettings());
            DatabaseProvider = (client, databaseName) => ClientProvider().GetDatabase(databaseName);
        }

        protected virtual MongoClientSettings InitializeSettings()
        {
            var settings = Settings ?? new MongoClientSettings
            {
                Server = GetServerAddress()
            };
            settings.ConnectTimeout = ConnectTimeout;
            settings.ServerSelectionTimeout = ServerSelectionTimeout;

            return settings;
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

        public static Builder Create(string database, MongoClientSettings settings) => new Builder(database, settings);

        public static Builder Create(string database, string connectionString) => new Builder(database, connectionString);

        public abstract class Configurator<T> : WatcherConfigurator<T,MongoDbWatcherConfiguration> where T : Configurator<T>
        {
            protected Configurator(string database, MongoClientSettings settings)
            {
                Configuration = new MongoDbWatcherConfiguration(database, settings);
            }

            protected Configurator(string database, string connectionString)
            {
                Configuration = new MongoDbWatcherConfiguration(database, connectionString);
            }

            protected Configurator(MongoDbWatcherConfiguration configuration) : base(configuration)
            {
            }

            public T WithSettings(MongoClientSettings settings)
            {
                Configuration.Settings = settings;

                return Configurator;
            }

            public T WithServerSelectionTimeout(TimeSpan timeout)
            {
                ValidateTimeout(timeout);
                Configuration.ServerSelectionTimeout = timeout;

                return Configurator;
            }

            public T WithConnectTimeout(TimeSpan timeout)
            {
                ValidateTimeout(timeout);
                Configuration.ConnectTimeout = timeout;

                return Configurator;
            }

            public T WithClientProvider(Func<MongoClient> clientProvider)
            {
                if (clientProvider == null)
                    throw new ArgumentNullException(nameof(clientProvider), "Mongo client provider can not be null.");

                Configuration.ClientProvider = clientProvider;

                return Configurator;
            }

            public T WithDatabaseProvider(Func<MongoClient, string, IMongoDatabase> databaseProvider)
            {
                if (databaseProvider == null)
                {
                    throw new ArgumentNullException(nameof(databaseProvider),
                        "MongoDB database provider can not be null.");
                }

                Configuration.DatabaseProvider = databaseProvider;

                return Configurator;
            }

            public T EnsureThat(Func<IMongoDatabase, bool> ensureThat)
            {
                if (ensureThat == null)
                    throw new ArgumentException("Ensure that predicate can not be null.", nameof(ensureThat));

                Configuration.EnsureThat = ensureThat;

                return Configurator;
            }

            public T EnsureThatAsync(Func<IMongoDatabase, Task<bool>> ensureThat)
            {
                if (ensureThat == null)
                    throw new ArgumentException("Ensure that async predicate can not be null.", nameof(ensureThat));

                Configuration.EnsureThatAsync = ensureThat;

                return Configurator;
            }

            protected void ValidateTimeout(TimeSpan timeout)
            {
                if (timeout == null)
                    throw new ArgumentNullException(nameof(timeout), "Timeout can not be null.");

                if (timeout == TimeSpan.Zero)
                    throw new ArgumentException("Timeout can not be equal to zero.", nameof(timeout));
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
            public Builder(string database, string connectionString) : base(database, connectionString)
            {
                SetInstance(this);
            }

            public Builder(string database, MongoClientSettings settings) : base(database, settings)
            {
                SetInstance(this);
            }

            public MongoDbWatcherConfiguration Build() => Configuration;

            public static explicit operator Default(Builder builder) => new Default(builder.Configuration);
        }
    }
}