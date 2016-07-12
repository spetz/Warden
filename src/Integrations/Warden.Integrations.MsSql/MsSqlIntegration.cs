using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;

namespace Warden.Integrations.MsSql
{
    /// <summary>
    /// Integration with the MS SQL.
    /// </summary>
    public class MsSqlIntegration : IIntegration
    {
        private readonly IMsSqlService _msSqlService;
        private readonly MsSqlIntegrationConfiguration _configuration;

        public MsSqlIntegration(MsSqlIntegrationConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration),
                    "MS SQL Integration configuration has not been provided.");
            }

            _configuration = configuration;
            _msSqlService = _configuration.MsSqlServiceProvider();
        }


        /// <summary>
        /// Executes the SQL query and returns a collection of the dynamic results.
        /// </summary>
        /// <param name="query">SQL query.</param>
        /// <param name="parameters">SQL query parameters.</param>
        /// <returns>Collection of the dynamic results.</returns>
        public async Task<IEnumerable<dynamic>> QueryAsync(string query, IDictionary<string, object> parameters = null)
        {
            try
            {
                using (var connection = _configuration.ConnectionProvider(_configuration.ConnectionString))
                {
                    connection.Open();
                    var queryToExecute = string.IsNullOrWhiteSpace(query) ? _configuration.Query : query;
                    var queryParameters = parameters ?? _configuration.QueryParameters;

                    return await _msSqlService.QueryAsync(connection, queryToExecute,
                        queryParameters, _configuration.QueryTimeout);
                }
            }
            catch (SqlException exception)
            {
                throw new IntegrationException("There was a SQL error while trying to execute the query.", exception);
            }
            catch (Exception exception)
            {
                throw new IntegrationException("There was an error while trying to access the MS SQL database.",
                    exception);
            }
        }

        /// <summary>
        /// Executes the SQL command and returns a scalar representing the number of affected rows.
        /// </summary>
        /// <param name="command">SQL command.</param>
        /// <param name="parameters">SQL command parameters.</param>
        /// <returns>Scalar representing the number of affected rows.</returns>
        public async Task<int> ExecuteAsync(string command, IDictionary<string, object> parameters = null)
        {
            try
            {
                using (var connection = _configuration.ConnectionProvider(_configuration.ConnectionString))
                {
                    connection.Open();
                    var commandToExecute = string.IsNullOrWhiteSpace(command) ? _configuration.Command : command;
                    var commandParameters = parameters ?? _configuration.CommandParameters;

                    return await _msSqlService.ExecuteAsync(connection, commandToExecute,
                        commandParameters, _configuration.CommandTimeout);
                }
            }
            catch (SqlException exception)
            {
                throw new IntegrationException("There was a SQL error while trying to execute the command.", exception);
            }
            catch (Exception exception)
            {
                throw new IntegrationException("There was an error while trying to access the MS SQL database.",
                    exception);
            }
        }

        /// <summary>
        /// Factory method for creating a new instance of MsSqlIntegration.
        /// </summary>
        /// <param name="connectionString">Connection string of the MS SQL server.</param>
        /// <param name="configurator">Lambda expression for configuring MS SQL integration.</param>
        /// <returns>Instance of MsSqlIntegration.</returns>
        public static MsSqlIntegration Create(string connectionString,
            Action<MsSqlIntegrationConfiguration.Builder> configurator)
        {
            var config = new MsSqlIntegrationConfiguration.Builder(connectionString);
            configurator?.Invoke(config);

            return Create(config.Build());
        }

        /// <summary>
        /// Factory method for creating a new instance of MsSqlIntegration.
        /// </summary>
        /// <param name="configuration">Configuration of MS SQL integration.</param>
        /// <returns>Instance of MsSqlIntegration.</returns>
        public static MsSqlIntegration Create(MsSqlIntegrationConfiguration configuration)
            => new MsSqlIntegration(configuration);
    }
}