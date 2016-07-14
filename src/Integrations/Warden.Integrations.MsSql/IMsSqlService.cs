using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;

namespace Warden.Integrations.MsSql
{
    /// <summary>
    /// Custom MS SQL database connector for executing the SQL queries.
    /// </summary>
    public interface IMsSqlService
    {
        /// <summary>
        /// Executes the SQL query and returns a collection of the strongly typed results.
        /// </summary>
        /// <param name="connection">Instance of IDbConnection.</param>
        /// <param name="query">SQL query.</param>
        /// <param name="parameters">SQL query parameters.</param>
        /// <param name="timeout">Optional timeout.</param>
        /// <returns>Collection of the strongly typed results.</returns>
        Task<IEnumerable<T>> QueryAsync<T>(IDbConnection connection, string query,
            IDictionary<string, object> parameters, TimeSpan? timeout = null);

        /// <summary>
        /// Executes the SQL command and returns a scalar representing number of affected rows.
        /// </summary>
        /// <param name="connection">Instance of IDbConnection.</param>
        /// <param name="command">SQL command.</param>
        /// <param name="parameters">SQL command parameters.</param>
        /// <param name="timeout">Optional timeout.</param>
        /// <returns>Scalar representing the number of affected rows.</returns>
        Task<int> ExecuteAsync(IDbConnection connection, string command,
            IDictionary<string, object> parameters, TimeSpan? timeout = null);
    }

    /// <summary>
    /// Default implementation of the IMsSqlService based on Dapper.
    /// </summary>
    public class DapperMsSqlService : IMsSqlService
    {
        public async Task<IEnumerable<T>> QueryAsync<T>(IDbConnection connection, string query,
            IDictionary<string, object> parameters, TimeSpan? timeout = null)
        {
            var queryParameters = new DynamicParameters();
            if (parameters != null)
            {
                foreach (var parameter in parameters)
                {
                    queryParameters.Add(parameter.Key, parameter.Value);
                }
            }

            return await connection.QueryAsync<T>(query, queryParameters,
                commandTimeout: (int?)timeout?.TotalSeconds);
        }

        public async Task<int> ExecuteAsync(IDbConnection connection, string command,
            IDictionary<string, object> parameters, TimeSpan? timeout = null)
        {
            var commandParameters = new DynamicParameters();
            if (parameters != null)
            {
                foreach (var parameter in parameters)
                {
                    commandParameters.Add(parameter.Key, parameter.Value);
                }
            }

            return await connection.ExecuteAsync(command, parameters,
                commandTimeout: (int?) timeout?.TotalSeconds);
        }
    }
}