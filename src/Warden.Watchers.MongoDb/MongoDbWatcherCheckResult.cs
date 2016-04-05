using System.Collections.Generic;
using System.Linq;

namespace Warden.Watchers.MongoDb
{
    /// <summary>
    /// Custom check result type for MongoDBWatcher.
    /// </summary>
    public class MongoDbWatcherCheckResult : WatcherCheckResult
    {
        /// <summary>
        /// Name of the MongoDB database. 
        /// </summary>
        public string Database { get; }

        /// <summary>
        /// Connection string of the MongoDB server.
        /// </summary>
        public string ConnectionString { get; }

        /// <summary>
        /// MongoDB Query.
        /// </summary>
        public string Query { get; }

        /// <summary>
        /// Collection of dynamic results of the MongoDB query.
        /// </summary>
        public IEnumerable<dynamic> QueryResult { get; }

        protected MongoDbWatcherCheckResult(MongoDbWatcher watcher, bool isValid, string description,
            string database, string connectionString, string query, IEnumerable<dynamic> queryResult)
            : base(watcher, isValid, description)
        {
            Database = database;
            ConnectionString = connectionString;
            Query = query;
            QueryResult = queryResult;
        }

        /// <summary>
        /// Factory method for creating a new instance of MongoDbWatcherCheckResult.
        /// </summary>
        /// <param name="watcher">Instance of MongoDbWatcher.</param>
        /// <param name="isValid">Flag determining whether the performed check was valid.</param>
        /// <param name="database">Name of the MongoDB database.</param>
        /// <param name="connectionString">Connection string of the MongoDB server.</param>
        /// <param name="description">Custom description of the performed check.</param>
        /// <returns>Instance of MongoDbWatcherCheckResult.</returns>
        public static MongoDbWatcherCheckResult Create(MongoDbWatcher watcher, bool isValid,
            string database, string connectionString, string description = "")
            => new MongoDbWatcherCheckResult(watcher, isValid, description, database,
                connectionString, string.Empty, Enumerable.Empty<dynamic>());

        /// <summary>
        /// Factory method for creating a new instance of MongoDbWatcherCheckResult.
        /// </summary>
        /// <param name="watcher">Instance of MongoDbWatcher.</param>
        /// <param name="isValid">Flag determining whether the performed check was valid.</param>
        /// <param name="database">Name of the MongoDB database.</param>
        /// <param name="connectionString">Connection string of the MongoDB server.</param>
        /// <param name="query">MongoDB query.</param>
        /// <param name="queryResult">Collection of dynamic results of the MongoDB query.</param>
        /// <param name="description">Custom description of the performed check.</param>
        /// <returns>Instance of MongoDbWatcherCheckResult.</returns>
        public static MongoDbWatcherCheckResult Create(MongoDbWatcher watcher, bool isValid, string database,
            string connectionString, string query, IEnumerable<dynamic> queryResult, string description = "")
            => new MongoDbWatcherCheckResult(watcher, isValid, description, database, connectionString, query,
                queryResult);
    }
}