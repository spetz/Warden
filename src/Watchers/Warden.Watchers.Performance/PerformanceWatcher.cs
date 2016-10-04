using System;
using System.Threading.Tasks;

namespace Warden.Watchers.Performance
{
    /// <summary>
    /// PerformanceWatcher designed for CPU & RAM monitoring.
    /// </summary>
    public class PerformanceWatcher : IWatcher
    {
        private readonly PerformanceWatcherConfiguration _configuration;
        public string Name { get; }
        public string Group { get; }
        public const string DefaultName = "Performance Watcher";

        protected PerformanceWatcher(string name, PerformanceWatcherConfiguration configuration, string group)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Watcher name can not be empty.");

            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration),
                    "Performance Watcher configuration has not been provided.");
            }

            Name = name;
            _configuration = configuration;
            Group = group;
        }

        public async Task<IWatcherCheckResult> ExecuteAsync()
        {
            try
            {
                var resourceUsage = await _configuration.PerformanceProvider().GetResourceUsageAsync();
                var isValid = true;
                if (_configuration.EnsureThatAsync != null)
                    isValid = await _configuration.EnsureThatAsync?.Invoke(resourceUsage);

                isValid = isValid && (_configuration.EnsureThat?.Invoke(resourceUsage) ?? true);
                var description = $"Performance check has returned {(isValid ? "valid" : "invalid")} result for " +
                                  $"CPU usage: {resourceUsage.Cpu:F}%, RAM usage: {resourceUsage.Ram} MB.";

                return PerformanceWatcherCheckResult.Create(this, isValid, _configuration.Delay,
                    _configuration.MachineName, resourceUsage, description);
            }
            catch (Exception exception)
            {
                throw new WatcherException("There was an error while trying to calculate performance.", exception);
            }
        }

        /// <summary>
        /// Factory method for creating a new instance of PerformanceWatcher with default name of Performance Watcher.
        /// </summary>
        /// <param name="delay">Delay between resource usage calculation while using the default performance counter (100 ms by default).</param>
        /// <param name="machineName">Optional name of the remote machine.</param>
        /// <param name="configurator">Optional lambda expression for configuring the PerformanceWatcher.</param>
        /// <param name="group">Optional name of the group that PerformanceWatcher belongs to.</param>
        /// <returns>Instance of PerformanceWatcher.</returns>
        public static PerformanceWatcher Create(TimeSpan? delay = null,
            string machineName = null,
            Action<PerformanceWatcherConfiguration.Default> configurator = null,
            string group = null)
            => Create(DefaultName, delay, machineName, configurator, group);

        /// <summary>
        /// Factory method for creating a new instance of PerformanceWatcher with default name of Performance Watcher.
        /// </summary>
        /// <param name="name">Name of the PerformanceWatcher.</param>
        /// <param name="delay">Delay between resource usage calculation while using the default performance counter (100 ms by default).</param>
        /// <param name="machineName">Optional name of the remote machine.</param>
        /// <param name="configurator">Optional lambda expression for configuring the PerformanceWatcher.</param>
        /// <param name="group">Optional name of the group that PerformanceWatcher belongs to.</param>
        /// <returns>Instance of PerformanceWatcher.</returns>
        public static PerformanceWatcher Create(string name, TimeSpan? delay = null,
            string machineName = null,
            Action<PerformanceWatcherConfiguration.Default> configurator = null,
            string group = null)
        {
            var config = new PerformanceWatcherConfiguration.Builder(delay, machineName);
            configurator?.Invoke((PerformanceWatcherConfiguration.Default) config);

            return Create(name, config.Build(), group);
        }

        /// <summary>
        /// Factory method for creating a new instance of PerformanceWatcher with default name of Performance Watcher.
        /// </summary>
        /// <param name="configuration">Configuration of PerformanceWatcher.</param>
        /// <param name="group">Optional name of the group that PerformanceWatcher belongs to.</param>
        /// <returns>Instance of PerformanceWatcher.</returns>
        public static PerformanceWatcher Create(PerformanceWatcherConfiguration configuration, 
            string group = null)
            => Create(DefaultName, configuration, group);

        /// <summary>
        /// Factory method for creating a new instance of PerformanceWatcher.
        /// </summary>
        /// <param name="name">Name of the PerformanceWatcher.</param>
        /// <param name="configuration">Configuration of PerformanceWatcher.</param>
        /// <param name="group">Optional name of the group that PerformanceWatcher belongs to.</param> 
        /// <returns>Instance of PerformanceWatcher.</returns>
        public static PerformanceWatcher Create(string name, PerformanceWatcherConfiguration configuration,
            string group = null)
            => new PerformanceWatcher(name, configuration, group);
    }
}