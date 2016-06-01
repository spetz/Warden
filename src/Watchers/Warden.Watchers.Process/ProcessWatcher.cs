using System;
using System.Threading.Tasks;

namespace Warden.Watchers.Process
{
    /// <summary>
    /// ProcessWatcher designed for process monitoring.
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
            var processService = _configuration.ProcessServiceProvider();
            var processInfo  = await processService.GetProcessInfoAsync(_configuration.Name);
            var isRunning = processInfo.State == ProcessState.Running;
            var isValid = _configuration.SkipStateValidation
                ? processInfo.Exists
                : processInfo.State == ProcessState.Running;
            var description = $"Process '{_configuration.Name}' is {(isRunning ? string.Empty : "not ")}running.";
            var result = ProcessWatcherCheckResult.Create(this, isValid, processInfo, description);

            return await Task.FromResult(result);
        }
    }
}