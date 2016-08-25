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
        public string Group { get; }
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
            var isValid = _configuration.DoesNotHaveToBeResponding
                ? processInfo.Exists
                : processInfo.Exists && processInfo.Responding;

            var description = $"Process '{_configuration.Name}' does {(processInfo.Exists ? string.Empty : "not ")}exist.";
            var result = ProcessWatcherCheckResult.Create(this, isValid, processInfo, description);

            return await Task.FromResult(result);
        }

        /// <summary>
        /// Factory method for creating a new instance of ProcessWatcher with default name of Process Watcher.
        /// </summary>
        /// <param name="processName">Name of the process.</param>
        /// <param name="configurator">Optional lambda expression for configuring the ProcessWatcher.</param>
        /// <returns>Instance of ProcessWatcher.</returns>
        public static ProcessWatcher Create(string processName,
            Action<ProcessWatcherConfiguration.Default> configurator = null)
            => Create(DefaultName, processName, configurator);

        /// <summary>
        /// Factory method for creating a new instance of ProcessWatcher with default name of Process Watcher.
        /// </summary>
        /// <param name="name">Name of the ProcessWatcher.</param>
        /// <param name="processName">Name of the process.</param>
        /// <param name="configurator">Optional lambda expression for configuring the ProcessWatcher.</param>
        /// <returns>Instance of ProcessWatcher.</returns>
        public static ProcessWatcher Create(string name, string processName,
            Action<ProcessWatcherConfiguration.Default> configurator = null)
        {
            var config = new ProcessWatcherConfiguration.Builder(processName);
            configurator?.Invoke((ProcessWatcherConfiguration.Default)config);

            return Create(name, config.Build());
        }

        /// <summary>
        /// Factory method for creating a new instance of ProcessWatcher with default name of Process Watcher.
        /// </summary>
        /// <param name="configuration">Configuration of ProcessWatcher.</param>
        /// <returns>Instance of ProcessWatcher.</returns>
        public static ProcessWatcher Create(ProcessWatcherConfiguration configuration)
            => Create(DefaultName, configuration);

        /// <summary>
        /// Factory method for creating a new instance of ProcessWatcher.
        /// </summary>
        /// <param name="name">Name of the ProcessWatcher.</param>
        /// <param name="configuration">Configuration of ProcessWatcher.</param>
        /// <returns>Instance of ProcessWatcher.</returns>
        public static ProcessWatcher Create(string name, ProcessWatcherConfiguration configuration)
            => new ProcessWatcher(name, configuration);
    }
}