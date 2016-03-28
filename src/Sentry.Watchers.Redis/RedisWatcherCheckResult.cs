using System.Collections.Generic;
namespace Sentry.Watchers.Redis
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

        protected RedisWatcherCheckResult(IWatcher watcher, bool isValid, string description,
            string connectionString) : base(watcher, isValid, description)
        {
            ConnectionString = connectionString;
        }

        /// <summary>
        /// Factory method for creating a new instance of RedisWatcherCheckResult.
        /// </summary>
        /// <param name="watcher">Instance of IWatcher.</param>
        /// <param name="isValid">Flag determining whether the performed check was valid.</param>
        /// <param name="connectionString">Connection string of the Redis server.</param>
        /// <param name="description">Custom description of the performed check.</param>
        /// <returns>Instance of RedisWatcherCheckResult.</returns>
        public static RedisWatcherCheckResult Create(IWatcher watcher, bool isValid,
            string connectionString, string description = "")
            => new RedisWatcherCheckResult(watcher, isValid, description, connectionString);
    }
}