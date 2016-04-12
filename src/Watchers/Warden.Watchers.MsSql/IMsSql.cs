using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;

namespace Warden.Watchers.MsSql
{
    /// <summary>
    /// Custom MSSQL database connector for executing the SQL queries.
    /// </summary>
    public interface IMsSql
    {
        /// <summary>
        /// Executes the SQL query and returns a collection of the dynamic results.
        /// </summary>
        /// <param name="connection">Instance of IDbConnection.</param>
        /// <param name="query">SQL query.</param>
        /// <param name="parameters">SQL query parameters.</param>
        /// <param name="timeout">Optional timeout.</param>
        /// <returns></returns>
        Task<IEnumerable<dynamic>> QueryAsync(IDbConnection connection, string query,
            IDictionary<string, object> parameters, TimeSpan? timeout = null);
    }

    /// <summary>
    /// Default implementation of the IMsSql based on Dapper.
    /// </summary>
    public class DapperMsSql : IMsSql
    {
        public async Task<IEnumerable<dynamic>> QueryAsync(IDbConnection connection, string query,
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

            return await connection.QueryAsync<dynamic>(query, queryParameters,
                commandTimeout: (int?) timeout?.TotalSeconds);
        }
    }
}