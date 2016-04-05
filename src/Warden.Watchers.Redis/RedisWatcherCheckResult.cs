using System.Collections.Generic;
using System.Linq;

namespace Warden.Watchers.Redis
{
    /// <summary>
    /// Custom check result type for RedisWatcher.
    /// </summary>
    public class RedisWatcherCheckResult : WatcherCheckResult
    {
        /// <summary>
        /// Connection string of the Redis server.
        /// </summary>
        public string ConnectionString { get; protected set; }

        /// <summary>
        /// Id of the Redis database. 
        /// </summary>
        public int Database { get; }

        /// <summary>
        /// Redis query.
        /// </summary>
        public string Query { get; }

        /// <summary>
        /// Collection of dynamic results of the Redis query.
        /// </summary>
        public IEnumerable<dynamic> QueryResult { get; }

        protected RedisWatcherCheckResult(RedisWatcher watcher, bool isValid, string description,
            int database, string connectionString, string query, IEnumerable<dynamic> queryResult)
            : base(watcher, isValid, description)
        {
            Database = database;
            ConnectionString = connectionString;
            Query = query;
            QueryResult = queryResult;
        }

        /// <summary>
        /// Factory method for creating a new instance of RedisWatcherCheckResult.
        /// </summary>
        /// <param name="watcher">Instance of RedisWatcher.</param>
        /// <param name="isValid">Flag determining whether the performed check was valid.</param>
        /// <param name="database">Id of the Redis database.</param>
        /// <param name="connectionString">Connection string of the Redis server.</param>
        /// <param name="description">Custom description of the performed check.</param>
        /// <returns>Instance of RedisWatcherCheckResult.</returns>
        public static RedisWatcherCheckResult Create(RedisWatcher watcher, bool isValid,
            int database, string connectionString, string description = "")
            => new RedisWatcherCheckResult(watcher, isValid, description, database,
                connectionString, string.Empty, Enumerable.Empty<dynamic>());

        /// <summary>
        /// Factory method for creating a new instance of RedisWatcherCheckResult.
        /// </summary>
        /// <param name="watcher">Instance of RedisWatcher.</param>
        /// <param name="isValid">Flag determining whether the performed check was valid.</param>
        /// <param name="connectionString">Connection string of the Redis server.</param>
        /// <param name="database">Id of the Redis database.</param>
        /// <param name="query">Redis query.</param>
        /// <param name="queryResult">Collection of dynamic results of the Redis query.</param>
        /// <param name="description">Custom description of the performed check.</param>
        /// <returns>Instance of RedisWatcherCheckResult.</returns>
        public static RedisWatcherCheckResult Create(RedisWatcher watcher, bool isValid,
            int database, string connectionString, string query, IEnumerable<dynamic> queryResult,
            string description = "")
            => new RedisWatcherCheckResult(watcher, isValid, description, database, connectionString, query,
                queryResult);
    }
}