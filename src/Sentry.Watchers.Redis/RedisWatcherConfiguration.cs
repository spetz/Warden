using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sentry.Watchers.Redis
{
    /// <summary>
    /// Configuration of the RedisWatcher.
    /// </summary>
    public class RedisWatcherConfiguration
    {
        /// <summary>
        /// Connection string of the Redis server.
        /// </summary>
        public string ConnectionString { get; protected set; }

        /// <summary>
        /// Custom provider for the IRedis.
        /// </summary>
        public Func<IRedis> RedisProvider { get; protected set; }

        /// <summary>
        /// Predicate that has to be satisfied in order to return the valid result.
        /// </summary>
        public Func<IEnumerable<dynamic>, bool> EnsureThat { get; protected set; }

        /// <summary>
        /// Async predicate that has to be satisfied in order to return the valid result.
        /// </summary>
        public Func<IEnumerable<dynamic>, Task<bool>> EnsureThatAsync { get; protected set; }
    }
}