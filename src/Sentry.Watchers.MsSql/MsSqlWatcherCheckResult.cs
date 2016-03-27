using System.Collections.Generic;
using System.Linq;

namespace Sentry.Watchers.MsSql
{
    public class MsSqlWatcherCheckResult : WatcherCheckResult
    {
        public string ConnectionString { get; }
        public string Query { get; }
        public IEnumerable<dynamic> QueryResult { get; }

        protected MsSqlWatcherCheckResult(IWatcher watcher, bool isValid, string description,
            string connectionString, string query, IEnumerable<dynamic> queryResult)
            : base(watcher, isValid, description)
        {
            ConnectionString = connectionString;
            Query = query;
            QueryResult = queryResult;
        }

        public static MsSqlWatcherCheckResult Create(IWatcher watcher, bool isValid,
            string connectionString, string description = "")
            => new MsSqlWatcherCheckResult(watcher, isValid, description, connectionString, string.Empty,
                Enumerable.Empty<dynamic>());

        public static MsSqlWatcherCheckResult Create(IWatcher watcher, bool isValid,
            string connectionString, string query, IEnumerable<dynamic> queryResult, string description = "")
            => new MsSqlWatcherCheckResult(watcher, isValid, description, connectionString, query, queryResult);
    }
}