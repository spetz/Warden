using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Warden.Watchers.Performance
{
    /// <summary>
    /// PerformanceWatcher designed for CPU & RAM monitoring.
    /// </summary>
    public class PerformanceWatcher : IWatcher
    {
        public string Name { get; }
        public const string DefaultName = "Performance Watcher";

        protected PerformanceWatcher(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Watcher name can not be empty.");

            Name = name;
        }

        public async Task<IWatcherCheckResult> ExecuteAsync()
        {
            var cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            var ramCounter = new PerformanceCounter("Memory", "Available MBytes");
            cpuCounter.NextValue();
            ramCounter.NextValue();
            await Task.Delay(100);
            var cpuUsage = cpuCounter.NextValue();
            var ramUsage = ramCounter.NextValue();

            return WatcherCheckResult.Create(this, true, $"CPU usage: {cpuUsage}%, RAM usage: {ramUsage} MB.");
        }

        /// <summary>
        /// Factory method for creating a new instance of PerformanceWatcher with default name of CPU Watcher.
        /// </summary>
        /// <returns>Instance of PerformanceWatcher.</returns>
        public static PerformanceWatcher Create()
            => Create(DefaultName);

        /// <summary>
        /// Factory method for creating a new instance of PerformanceWatcher.
        /// </summary>
        /// <param name="name">Name of the PerformanceWatcher.</param>
        /// <returns>Instance of PerformanceWatcher.</returns>
        public static PerformanceWatcher Create(string name)
            => new PerformanceWatcher(name);
    }
}