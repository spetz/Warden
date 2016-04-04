using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Warden.Watchers.Cpu
{
    /// <summary>
    /// CpuWatcher designed for CPU monitoring.
    /// </summary>
    public class CpuWatcher : IWatcher
    {
        public string Name { get; }
        public const string DefaultName = "CPU Watcher";

        protected CpuWatcher(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Watcher name can not be empty.");

            Name = name;
        }

        public async Task<IWatcherCheckResult> ExecuteAsync()
        {
            var cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            cpuCounter.NextValue();
            await Task.Delay(100);
            var cpuUsage = cpuCounter.NextValue();

            return WatcherCheckResult.Create(this, true, $"CPU usage: {cpuUsage}%");
        }

        /// <summary>
        /// Factory method for creating a new instance of CpuWatcher with default name of CPU Watcher.
        /// </summary>
        /// <returns>Instance of CpuWatcher.</returns>
        public static CpuWatcher Create()
            => Create(DefaultName);

        /// <summary>
        /// Factory method for creating a new instance of CpuWatcher.
        /// </summary>
        /// <param name="name">Name of the CpuWatcher.</param>
        /// <returns>Instance of CpuWatcher.</returns>
        public static CpuWatcher Create(string name)
            => new CpuWatcher(name);
    }
}