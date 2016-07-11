using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace Warden.Integrations.MsSql
{
    /// <summary>
    /// Configuration of the MsSqlIntegration.
    /// </summary>
    public class MsSqlIntegrationConfiguration
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
        /// Custom provider for the IMsSqlService.
        /// </summary>
        public Func<IMsSqlService> MsSqlServiceProvider { get; protected set; }

        /// <summary>
        /// Collection of SQL query parameters.
        /// </summary>
        public IDictionary<string, object> QueryParameters { get; protected set; }

        protected internal MsSqlIntegrationConfiguration(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentException("MS SQL connection string can not be empty.", nameof(connectionString));

            try
            {
                var sqlConnectionStringBuilder = new SqlConnectionStringBuilder(connectionString);
                Database = sqlConnectionStringBuilder.InitialCatalog;
            }
            catch (Exception ex)
            {
                throw new ArgumentException("MS SQL connection string is invalid.", nameof(connectionString));
            }

            ConnectionString = connectionString;
            ConnectionProvider = sqlConnectionString => new SqlConnection(sqlConnectionString);
            MsSqlServiceProvider = () => new DapperMsSqlService();
        }

        /// <summary>
        /// Factory method for creating a new instance of fluent builder for the MsSqlIntegrationConfiguration.
        /// </summary>
        /// <param name="connectionString">Connection string of the MS SQL server.</param>
        /// <returns>Instance of fluent builder for the MsSqlIntegrationConfiguration.</returns>
        public static Builder Create(string connectionString) => new Builder(connectionString);

        /// <summary>
        /// Fluent builder for the MsSqlIntegrationConfiguration.
        /// </summary>
        public class Builder
        {
            protected readonly MsSqlIntegrationConfiguration Configuration;

            /// <summary>
            /// Constructor of fluent builder for the MsSqlIntegrationConfiguration.
            /// </summary>
            /// <param name="connectionString">Connection string of the MS SQL server.</param>
            /// <returns>Instance of fluent builder for the MsSqlIntegrationConfiguration.</returns>
            public Builder(string connectionString)
            {
                Configuration = new MsSqlIntegrationConfiguration(connectionString);
            }

            /// <summary>
            /// Sets the SQL query and its parameters (optional).
            /// </summary>
            /// <param name="query">SQL query.</param>
            /// <param name="parameters">Optional SQL query parameters.</param>
            /// <returns>Instance of fluent builder for the MsSqlIntegrationConfiguration.</returns>
            public Builder WithQuery(string query, IDictionary<string, object> parameters = null)
            {
                if (string.IsNullOrEmpty(query))
                    throw new ArgumentException("SQL query can not be empty.", nameof(query));

                Configuration.Query = query;
                Configuration.QueryParameters = parameters;

                return this;
            }

            /// <summary>
            /// Sets the timeout for the SQL query execution.
            /// </summary>
            /// <param name="timeout">Timeout of SQL query.</param>
            /// <returns>Instance of fluent builder for the MsSqlIntegrationConfiguration.</returns>
            public Builder WithQueryTimeout(TimeSpan timeout)
            {
                if (timeout == null)
                    throw new ArgumentNullException(nameof(timeout), "Timeout can not be null.");

                if (timeout == TimeSpan.Zero)
                    throw new ArgumentException("Timeout can not be equal to zero.", nameof(timeout));

                Configuration.QueryTimeout = timeout;

                return this;
            }

            /// <summary>
            /// Sets the custom provider for the IDbConnection.
            /// </summary>
            /// <param name="connectionProvider">Custom provider for the IDbConnection.</param>
            /// <returns>Lambda expression taking as an input connection string 
            /// and returning an instance of the IDbConnection.</returns>
            /// <returns>Instance of fluent builder for the MsSqlIntegrationConfiguration.</returns>
            public Builder WithConnectionProvider(Func<string, IDbConnection> connectionProvider)
            {
                if (connectionProvider == null)
                {
                    throw new ArgumentNullException(nameof(connectionProvider),
                        "SQL connection provider can not be null.");
                }

                Configuration.ConnectionProvider = connectionProvider;

                return this;
            }

            /// <summary>
            /// Sets the custom provider for the IMsSql.
            /// </summary>
            /// <param name="msSqlServiceProvider">Custom provider for the IMsSqlService.</param>
            /// <returns>Lambda expression returning an instance of the IMsSqlService.</returns>
            /// <returns>Instance of fluent builder for the MsSqlIntegrationConfiguration.</returns>
            public Builder WithMsSqlServiceProvider(Func<IMsSqlService> msSqlServiceProvider)
            {
                if (msSqlServiceProvider == null)
                {
                    throw new ArgumentNullException(nameof(msSqlServiceProvider),
                        "MS SQL service provider can not be null.");
                }

                Configuration.MsSqlServiceProvider = msSqlServiceProvider;

                return this;
            }

            /// <summary>
            /// Builds the MsSqlIntegrationConfiguration and return its instance.
            /// </summary>
            /// <returns>Instance of MsSqlIntegrationConfiguration.</returns>
            public MsSqlIntegrationConfiguration Build() => Configuration;
        }
    }
}