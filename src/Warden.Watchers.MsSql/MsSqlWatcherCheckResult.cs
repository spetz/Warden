using System.Collections.Generic;
using System.Linq;

namespace Warden.Watchers.MsSql
{
    /// <summary>
    /// Custom check result type for MsSqlWatcher.
    /// </summary>
    public class MsSqlWatcherCheckResult : WatcherCheckResult
    {
        /// <summary>
        /// Connection string of the MSSQL server.
        /// </summary>
        public string ConnectionString { get; }

        /// <summary>
        /// SQL Query.
        /// </summary>
        public string Query { get; }

        /// <summary>
        /// Collection of dynamic results of the SQL query.
        /// </summary>
        public IEnumerable<dynamic> QueryResult { get; }

        protected MsSqlWatcherCheckResult(MsSqlWatcher watcher, bool isValid, string description,
            string connectionString, string query, IEnumerable<dynamic> queryResult)
            : base(watcher, isValid, description)
        {
            ConnectionString = connectionString;
            Query = query;
            QueryResult = queryResult;
        }

        /// <summary>
        /// Factory method for creating a new instance of MsSqlWatcherCheckResult.
        /// </summary>
        /// <param name="watcher">Instance of MsSqlWatcher.</param>
        /// <param name="isValid">Flag determining whether the performed check was valid.</param>
        /// <param name="connectionString">Connection string of the MSSQL server.</param>
        /// <param name="description">Custom description of the performed check.</param>
        /// <returns>Instance of MsSqlWatcherCheckResult.</returns>
        public static MsSqlWatcherCheckResult Create(MsSqlWatcher watcher, bool isValid,
            string connectionString, string description = "")
            => new MsSqlWatcherCheckResult(watcher, isValid, description, connectionString, string.Empty,
                Enumerable.Empty<dynamic>());

        /// <summary>
        /// Factory method for creating a new instance of MsSqlWatcherCheckResult.
        /// </summary>
        /// <param name="watcher">Instance of MsSqlWatcher.</param>
        /// <param name="isValid">Flag determining whether the performed check was valid.</param>
        /// <param name="connectionString">Connection string of the MSSQL server.</param>
        /// <param name="query">SQL query.</param>
        /// <param name="queryResult">Collection of dynamic results of the SQL query.</param>
        /// <param name="description">Custom description of the performed check.</param>
        /// <returns>Instance of MsSqlWatcherCheckResult.</returns>
        public static MsSqlWatcherCheckResult Create(MsSqlWatcher watcher, bool isValid,
            string connectionString, string query, IEnumerable<dynamic> queryResult, string description = "")
            => new MsSqlWatcherCheckResult(watcher, isValid, description, connectionString, query, queryResult);
    }
}