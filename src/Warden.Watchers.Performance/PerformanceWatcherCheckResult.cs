using System;

namespace Warden.Watchers.Performance
{
    /// <summary>
    /// Custom check result type for PerformanceWatcher.
    /// </summary>
    public class PerformanceWatcherCheckResult : WatcherCheckResult
    {
        /// <summary>
        /// Delay between resource usage calculation while using the default performance counter (100 ms by default).
        /// </summary>
        public TimeSpan Delay { get; protected set; }

        /// <summary>
        /// Usage of the resources such as CPU and RAM.
        /// </summary>
        public ResourceUsage ResourceUsage { get; }

        protected PerformanceWatcherCheckResult(PerformanceWatcher watcher, bool isValid, string description,
            TimeSpan delay, ResourceUsage resourceUsage)
            : base(watcher, isValid, description)
        {
            Delay = delay;
            ResourceUsage = resourceUsage;
        }

        /// <summary>
        /// Factory method for creating a new instance of PerformanceWatcherCheckResult.
        /// </summary>
        /// <param name="watcher">Instance of PerformanceWatcher.</param>
        /// <param name="isValid">Flag determining whether the performed check was valid.</param>
        /// <param name="delay">Delay between resource usage calculation while using the default performance counter (100 ms by default).</param>
        /// <param name="resourceUsage">Usage of the resources such as CPU and RAM.</param>
        /// <param name="description">Custom description of the performed check.</param>
        /// <returns>Instance of PerformanceWatcherCheckResult.</returns>
        public static PerformanceWatcherCheckResult Create(PerformanceWatcher watcher, bool isValid,
            TimeSpan delay, ResourceUsage resourceUsage, string description = "")
            => new PerformanceWatcherCheckResult(watcher, isValid, description, delay, resourceUsage);
    }
}