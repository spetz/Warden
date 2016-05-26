using System;
using System.Threading.Tasks;

namespace Warden.Watchers.Process
{
    /// <summary>
    /// PerformanceWatcher designed for application process monitoring.
    /// </summary>
    public class ProcessWatcher : IWatcher
    {
        private readonly ProcessWatcherConfiguration _configuration;
        public string Name { get; }
        public const string DefaultName = "Process Watcher";

        protected ProcessWatcher(string name, ProcessWatcherConfiguration configuration)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Watcher name can not be empty.");

            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration),
                    "Process Watcher configuration has not been provided.");
            }

            Name = name;
            _configuration = configuration;
        }

        public async Task<IWatcherCheckResult> ExecuteAsync()
        {
            throw new System.NotImplementedException();
        }
    }
}