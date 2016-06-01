namespace Warden.Watchers.Process
{
    /// <summary>
    /// Custom check result type for ProcessWatcher.
    /// </summary>
    public class ProcessWatcherCheckResult : WatcherCheckResult
    {
        /// <summary>
        /// Details of the process.
        /// </summary>
        public ProcessInfo ProcessInfo { get; }

        protected ProcessWatcherCheckResult(ProcessWatcher watcher, bool isValid, string description,
            ProcessInfo processInfo)
            : base(watcher, isValid, description)
        {
            ProcessInfo = processInfo;
        }

        /// <summary>
        /// Factory method for creating a new instance of ProcessWatcherCheckResult.
        /// </summary>
        /// <param name="watcher">Instance of ProcessWatcher.</param>
        /// <param name="isValid">Flag determining whether the performed check was valid.</param>
        /// <param name="processInfo">Details of the process.</param>
        /// <param name="description">Custom description of the performed check.</param>
        /// <returns>Instance of ProcessWatcherCheckResult.</returns>
        public static ProcessWatcherCheckResult Create(ProcessWatcher watcher, bool isValid,
             ProcessInfo processInfo, string description = "")
            => new ProcessWatcherCheckResult(watcher, isValid, description, processInfo);
    }
}