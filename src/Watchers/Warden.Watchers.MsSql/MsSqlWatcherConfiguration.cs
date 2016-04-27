using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Warden.Core;

namespace Warden.Watchers.MsSql
{
    /// <summary>
    /// Configuration of the MsSqlWatcher.
    /// </summary>
    public class MsSqlWatcherConfiguration
    {
        /// <summary>
        /// Connection string of the MSSQL server.
        /// </summary>
        public string ConnectionString { get; protected set; }

        /// <summary>
        /// Read-only name of the database.
        /// </summary>
        public string Database { get; protected set; }

        /// <summary>
        /// SQL Query.
        /// </summary>
        public string Query { get; protected set; }

        /// <summary>
        /// Optional timeout of the SQL query.
        /// </summary>
        public TimeSpan? QueryTimeout { get; protected set; }

        /// <summary>
        /// Custom provider for the IDbConnection. Input parameter is connection string.
        /// </summary>
        public Func<string, IDbConnection> ConnectionProvider { get; protected set; }

        /// <summary>
        /// Custom provider for the IMsSql.
        /// </summary>
        public Func<IMsSql> MsSqlProvider { get; protected set; }

        /// <summary>
        /// Collection of SQL query parameters.
        /// </summary>
        public IDictionary<string, object> QueryParameters { get; protected set; }

        /// <summary>
        /// Predicate that has to be satisfied in order to return the valid result.
        /// </summary>
        public Func<IEnumerable<dynamic>, bool> EnsureThat { get; protected set; }

        /// <summary>
        /// Async predicate that has to be satisfied in order to return the valid result.
        /// </summary>
        public Func<IEnumerable<dynamic>, Task<bool>> EnsureThatAsync { get; protected set; }

        protected internal MsSqlWatcherConfiguration(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentException("MSSQL connection string can not be empty.", nameof(connectionString));

            try
            {
                var sqlConnectionStringBuilder = new SqlConnectionStringBuilder(connectionString);
                Database = sqlConnectionStringBuilder.InitialCatalog;
            }
            catch (Exception ex)
            {
                throw new ArgumentException("MSSQL connection string is invalid.", nameof(connectionString));
            }

            ConnectionString = connectionString;
            ConnectionProvider = sqlConnectionString => new SqlConnection(sqlConnectionString);
            MsSqlProvider = () => new DapperMsSql();
        }

        /// <summary>
        /// Factory method for creating a new instance of fluent builder for the MsSqlWatcherConfiguration.
        /// </summary>
        /// <param name="connectionString">Connection string of the MSSQL server.</param>
        /// <returns>Instance of fluent builder for the MsSqlWatcherConfiguration.</returns>
        public static Builder Create(string connectionString) => new Builder(connectionString);

        /// <summary>
        /// Fluent builder for the MsSqlWatcherConfiguration.
        /// </summary>
        public abstract class Configurator<T> : WatcherConfigurator<T, MsSqlWatcherConfiguration>
            where T : Configurator<T>
        {
            protected Configurator(string connectionString)
            {
                Configuration = new MsSqlWatcherConfiguration(connectionString);
            }

            protected Configurator(MsSqlWatcherConfiguration configuration) : base(configuration)
            {
            }

            /// <summary>
            /// Sets the SQL query and its parameters (optional).
            /// </summary>
            /// <param name="query">SQL query.</param>
            /// <param name="parameters">Optional SQL query parameters.</param>
            /// <returns>Instance of fluent builder for the MsSqlWatcherConfiguration.</returns>
            public T WithQuery(string query, IDictionary<string, object> parameters = null)
            {
                if (string.IsNullOrEmpty(query))
                    throw new ArgumentException("SQL query can not be empty.", nameof(query));

                Configuration.Query = query;
                Configuration.QueryParameters = parameters;

                return Configurator;
            }

            /// <summary>
            /// Sets the timeout for the SQL query execution.
            /// </summary>
            /// <param name="timeout">Timeout of SQL query.</param>
            /// <returns>Instance of fluent builder for the MsSqlWatcherConfiguration.</returns>
            public T WithQueryTimeout(TimeSpan timeout)
            {
                if (timeout == null)
                    throw new ArgumentNullException(nameof(timeout), "Timeout can not be null.");

                if (timeout == TimeSpan.Zero)
                    throw new ArgumentException("Timeout can not be equal to zero.", nameof(timeout));

                Configuration.QueryTimeout = timeout;

                return Configurator;
            }

            /// <summary>
            /// Sets the predicate that has to be satisfied in order to return the valid result.
            /// </summary>
            /// <param name="ensureThat">Lambda expression predicate.</param>
            /// <returns>Instance of fluent builder for the MsSqlWatcherConfiguration.</returns>
            public T EnsureThat(Func<IEnumerable<dynamic>, bool> ensureThat)
            {
                if (ensureThat == null)
                    throw new ArgumentException("Ensure that predicate can not be null.", nameof(ensureThat));

                Configuration.EnsureThat = results => ensureThat(results);

                return Configurator;
            }

            /// <summary>
            /// Sets the async predicate that has to be satisfied in order to return the valid result.
            /// </summary>
            /// <param name="ensureThat">Lambda expression predicate.</param>
            /// <returns>Instance of fluent builder for the MsSqlWatcherConfiguration.</returns>
            public T EnsureThatAsync(Func<IEnumerable<dynamic>, Task<bool>> ensureThat)
            {
                if (ensureThat == null)
                    throw new ArgumentException("Ensure that async predicate can not be null.", nameof(ensureThat));

                Configuration.EnsureThatAsync = ensureThat;

                return Configurator;
            }

            /// <summary>
            /// Sets the custom provider for the IDbConnection.
            /// </summary>
            /// <param name="connectionProvider">Custom provider for the IDbConnection.</param>
            /// <returns>Lambda expression taking as an input connection string 
            /// and returning an instance of the IDbConnection.</returns>
            /// <returns>Instance of fluent builder for the MsSqlWatcherConfiguration.</returns>
            public T WithConnectionProvider(Func<string, IDbConnection> connectionProvider)
            {
                if (connectionProvider == null)
                {
                    throw new ArgumentNullException(nameof(connectionProvider),
                        "SQL connection provider can not be null.");
                }

                Configuration.ConnectionProvider = connectionProvider;

                return Configurator;
            }

            /// <summary>
            /// Sets the custom provider for the IMsSql.
            /// </summary>
            /// <param name="msSqlProvider">Custom provider for the IMsSql.</param>
            /// <returns>Lambda expression returning an instance of the IMsSql.</returns>
            /// <returns>Instance of fluent builder for the MsSqlWatcherConfiguration.</returns>
            public T WithMsSqlProvider(Func<IMsSql> msSqlProvider)
            {
                if (msSqlProvider == null)
                    throw new ArgumentNullException(nameof(msSqlProvider), "MSSQL provider can not be null.");

                Configuration.MsSqlProvider = msSqlProvider;

                return Configurator;
            }
        }

        /// <summary>
        /// Default MsSqlWatcherConfiguration fluent builder used while configuring watcher via lambda expression.
        /// </summary>
        public class Default : Configurator<Default>
        {
            public Default(MsSqlWatcherConfiguration configuration) : base(configuration)
            {
                SetInstance(this);
            }
        }

        /// <summary>
        /// Extended MsSqlWatcherConfiguration fluent builder used while configuring watcher directly.
        /// </summary>
        public class Builder : Configurator<Builder>
        {
            public Builder(string connectionString) : base(connectionString)
            {
                SetInstance(this);
            }

            /// <summary>
            /// Builds the MsSqlWatcherConfiguration and return its instance.
            /// </summary>
            /// <returns>Instance of MsSqlWatcherConfiguration.</returns>
            public MsSqlWatcherConfiguration Build() => Configuration;

            /// <summary>
            /// Operator overload to provide casting the Builder configurator into Default configurator.
            /// </summary>
            /// <param name="builder">Instance of extended Builder configurator.</param>
            /// <returns>Instance of Default builder configurator.</returns>
            public static explicit operator Default(Builder builder) => new Default(builder.Configuration);
        }
    }
}