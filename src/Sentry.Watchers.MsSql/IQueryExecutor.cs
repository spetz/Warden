using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;

namespace Sentry.Watchers.MsSql
{
    public interface IQueryExecutor
    {
        Task<IEnumerable<dynamic>> QueryAsync(IDbConnection connection, string query,
            IDictionary<string, object> parameters, TimeSpan? timeout);
    }

    public class DapperQueryExecutor : IQueryExecutor
    {
        public async Task<IEnumerable<dynamic>> QueryAsync(IDbConnection connection, string query, 
            IDictionary<string, object> parameters, TimeSpan? timeout)
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