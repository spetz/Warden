using System;
using System.Linq;
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
            var processes = System.Diagnostics.Process.GetProcessesByName(_configuration.Name);
            var process = processes.FirstOrDefault();
            var exists = process != null;
            var isValid = process?.Responding ?? false;
            var state = ProcessState.Unknown;
            if (exists)
                state = isValid ? ProcessState.Running : ProcessState.Stopped;

            var processId = process?.Id ?? 0;
            var description = $"Process '{_configuration.Name}' is {(exists ? string.Empty : "not ")}running.";
            var processInfo = ProcessInfo.Create(processId, _configuration.Name, exists, state);
            var result = ProcessWatcherCheckResult.Create(this, isValid, processInfo, description);

            return await Task.FromResult(result);
        }
    }
}