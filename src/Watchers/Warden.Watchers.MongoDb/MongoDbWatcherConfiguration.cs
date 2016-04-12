using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using Warden.Core;

namespace Warden.Watchers.MongoDb
{
    /// <summary>
    /// Configuration of the MongoDbWatcher.
    /// </summary>
    public class MongoDbWatcherConfiguration
    {
        /// <summary>
        /// Name of the MongoDB database. 
        /// </summary>
        public string Database { get; protected set; }

        /// <summary>
        /// Connection string of the MongoDB server.
        /// </summary>
        public string ConnectionString { get; protected set; }

        /// <summary>
        /// MongoDB query.
        /// </summary>
        public string Query { get; protected set; }

        /// <summary>
        /// Name of the MongoDB collection.
        /// </summary>
        public string CollectionName { get; protected set; }

        /// <summary>
        /// Optional timeout of the MongoDB query (5 seconds by default).
        /// </summary>
        public TimeSpan Timeout { get; protected set; }

        /// <summary>
        /// Custom provider for the IMongoDbConnection. Input parameter is connection string.
        /// </summary>
        public Func<string, IMongoDbConnection> ConnectionProvider { get; protected set; }

        /// <summary>
        /// Custom provider for the IMongoDb. 
        /// </summary>
        public Func<IMongoDb> MongoDbProvider { get; protected set; }

        /// <summary>
        /// Predicate that has to be satisfied in order to return the valid result.
        /// </summary>
        public Func<IEnumerable<dynamic>, bool> EnsureThat { get; protected set; }

        /// <summary>
        /// Async predicate that has to be satisfied in order to return the valid result.
        /// </summary>
        public Func<IEnumerable<dynamic>, Task<bool>> EnsureThatAsync { get; protected set; }

        protected internal MongoDbWatcherConfiguration(string connectionString,
            string database, TimeSpan? timeout = null)
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

        /// <summary>
        /// Factory method for creating a new instance of fluent builder for the MongoDbWatcherConfiguration.
        /// </summary>
        /// <param name="connectionString">Connection string of the MongoDB server.</param>
        /// <param name="database">Name of the MongoDB database.</param>
        /// <param name="timeout">Optional timeout of the MongoDB query (5 seconds by default).</param>
        /// <returns>Instance of fluent builder for the MongoDbWatcherConfiguration.</returns>
        public static Builder Create(string connectionString, string database, TimeSpan? timeout = null)
            => new Builder(connectionString, database, timeout);

        /// <summary>
        /// Fluent builder for the MongoDbWatcherConfiguration.
        /// </summary>
        public abstract class Configurator<T> : WatcherConfigurator<T, MongoDbWatcherConfiguration>
            where T : Configurator<T>
        {
            protected Configurator(string connectionString, string database, TimeSpan? timeout = null)
            {
                Configuration = new MongoDbWatcherConfiguration(connectionString, database, timeout);
            }

            protected Configurator(MongoDbWatcherConfiguration configuration) : base(configuration)
            {
            }

            /// <summary>
            /// Sets the collection name and a MongoDB query.
            /// </summary>
            /// <param name="collectionName">Name of the MongoDB collection.</param>
            /// <param name="query">MongoDB query.</param>
            /// <returns>Instance of fluent builder for the MongoDbWatcherConfiguration.</returns>
            public T WithQuery(string collectionName, string query)
            {
                if (string.IsNullOrEmpty(collectionName))
                    throw new ArgumentException("MongoDB collection name can not be empty.", nameof(collectionName));

                if (string.IsNullOrEmpty(query))
                    throw new ArgumentException("MongoDB query can not be empty.", nameof(query));

                Configuration.CollectionName = collectionName;
                Configuration.Query = query;

                return Configurator;
            }


            /// <summary>
            /// Sets the predicate that has to be satisfied in order to return the valid result.
            /// </summary>
            /// <param name="ensureThat">Lambda expression predicate.</param>
            /// <returns>Instance of fluent builder for the MongoDbWatcherConfiguration.</returns>
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
            /// <returns>Instance of fluent builder for the MongoDbWatcherConfiguration.</returns>
            public T EnsureThatAsync(Func<IEnumerable<dynamic>, Task<bool>> ensureThat)
            {
                if (ensureThat == null)
                    throw new ArgumentException("Ensure that async predicate can not be null.", nameof(ensureThat));

                Configuration.EnsureThatAsync = ensureThat;

                return Configurator;
            }

            /// <summary>
            /// Sets the custom provider for the IMongoDbConnection.
            /// </summary>
            /// <param name="connectionProvider">Custom provider for the IMongoDbConnection.</param>
            /// <returns>Lambda expression taking as an input connection string 
            /// and returning an instance of the IMongoDbConnection.</returns>
            /// <returns>Instance of fluent builder for the MongoDbWatcherConfiguration.</returns>
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

            /// <summary>
            /// Sets the custom provider for the IMongoDb.
            /// </summary>
            /// <param name="mongoDbProvider">Custom provider for the IMongoDb.</param>
            /// <returns>Lambda expression returning an instance of the IMongoDb.</returns>
            /// <returns>Instance of fluent builder for the MongoDbWatcherConfiguration.</returns>
            public T WithMongoDbProvider(Func<IMongoDb> mongoDbProvider)
            {
                if (mongoDbProvider == null)
                {
                    throw new ArgumentNullException(nameof(mongoDbProvider), "MongoDB provider can not be null.");
                }

                Configuration.MongoDbProvider = mongoDbProvider;

                return Configurator;
            }
        }


        /// <summary>
        /// Default MongoDbWatcherConfiguration fluent builder used while configuring watcher via lambda expression.
        /// </summary>
        public class Default : Configurator<Default>
        {
            public Default(MongoDbWatcherConfiguration configuration) : base(configuration)
            {
                SetInstance(this);
            }
        }

        /// <summary>
        /// Extended MongoDbWatcherConfiguration fluent builder used while configuring watcher directly.
        /// </summary>
        public class Builder : Configurator<Builder>
        {
            public Builder(string database, string connectionString, TimeSpan? timeout = null)
                : base(database, connectionString, timeout)
            {
                SetInstance(this);
            }

            /// <summary>
            /// Builds the MongoDbWatcherConfiguration and return its instance.
            /// </summary>
            /// <returns>Instance of MongoDbWatcherConfiguration.</returns>
            public MongoDbWatcherConfiguration Build() => Configuration;

            /// <summary>
            /// Operator overload to provide casting the Builder configurator into Default configurator.
            /// </summary>
            /// <param name="builder">Instance of extended Builder configurator.</param>
            /// <returns>Instance of Default builder configurator.</returns>
            public static explicit operator Default(Builder builder) => new Default(builder.Configuration);
        }
    }
}