using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;

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

        public async Task<IEnumerable<dynamic>>  QueryAsync(string query, IDictionary<string, object> parameters = null)
        {
            try
            {
                using (var connection = _configuration.ConnectionProvider(_configuration.ConnectionString))
                {
                    connection.Open();

                    return await _msSqlService.QueryAsync(connection, _configuration.Query,
                        _configuration.QueryParameters, _configuration.QueryTimeout);
                }
            }
            catch (SqlException exception)
            {
                throw new IntegrationException("There was a SQL error while trying to execute the query.", exception);
            }
            catch (Exception exception)
            {
                throw new IntegrationException("There was an error while trying to access MSSQL database.", exception);
            }
        }

        public async Task<IEnumerable<dynamic>> ExecuteCommandAsync(string command, IDictionary<string, object> parameters = null)
        {
            try
            {
                using (var connection = _configuration.ConnectionProvider(_configuration.ConnectionString))
                {
                    connection.Open();

                    return await _msSqlService.QueryAsync(connection, _configuration.Query,
                        _configuration.QueryParameters, _configuration.QueryTimeout);
                }
            }
            catch (SqlException exception)
            {
                throw new IntegrationException("There was a SQL error while trying to execute the query.", exception);
            }
            catch (Exception exception)
            {
                throw new IntegrationException("There was an error while trying to access MSSQL database.", exception);
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