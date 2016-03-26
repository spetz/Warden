using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;

namespace Sentry.Watchers.MsSql
{
    public interface IMsSql
    {
        Task<IEnumerable<dynamic>> QueryAsync(IDbConnection connection, string query,
            IDictionary<string, object> parameters, TimeSpan? timeout = null);
    }

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

            return await connection.QueryAsync<dynamic>(query, queryParameters, commandTimeout: (int?)timeout?.TotalSeconds);
        }
    }
}