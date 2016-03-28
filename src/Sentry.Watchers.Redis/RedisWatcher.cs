using System;
using System.Threading.Tasks;

namespace Sentry.Watchers.Redis
{
    /// <summary>
    /// RedisWatcher designed for Redis monitoring.
    /// </summary>
    public class RedisWatcher : IWatcher
    {
        private readonly IRedis _redis;
        private readonly RedisWatcherConfiguration _configuration;
        public string Name { get; }

        protected RedisWatcher(string name, RedisWatcherConfiguration configuration)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Watcher name can not be empty.");

            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration),
                    "Redis Watcher configuration has not been provided.");
            }

            Name = name;
            _configuration = configuration;
            _redis = _configuration.RedisProvider();
        }

        public async Task<IWatcherCheckResult> ExecuteAsync()
        {
            throw new System.NotImplementedException();
        }
    }
}