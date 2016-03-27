using System.Collections.Generic;
using System.Linq;

namespace Sentry.Watchers.MongoDb
{
    public class MongoDbWatcherCheckResult : WatcherCheckResult
    {
        public string Database { get; }
        public string ConnectionString { get; }
        public string Query { get; }
        public IEnumerable<dynamic> QueryResult { get; }

        protected MongoDbWatcherCheckResult(IWatcher watcher, bool isValid, string description,
            string database, string connectionString, string query, IEnumerable<dynamic> queryResult)
            : base(watcher, isValid, description)
        {
            Database = database;
            ConnectionString = connectionString;
            Query = query;
            QueryResult = queryResult;
        }

        public static MongoDbWatcherCheckResult Create(IWatcher watcher, bool isValid,
            string database, string connectionString, string description = "")
            => new MongoDbWatcherCheckResult(watcher, isValid, description, database,
                connectionString, string.Empty, Enumerable.Empty<dynamic>());

        public static MongoDbWatcherCheckResult Create(IWatcher watcher, bool isValid, string database,
            string connectionString, string query, IEnumerable<dynamic> queryResult, string description = "")
            => new MongoDbWatcherCheckResult(watcher, isValid, description, database, connectionString, query,
                queryResult);
    }
}