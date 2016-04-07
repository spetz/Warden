namespace Warden.Watchers.Disk
{
    /// <summary>
    /// Custom check result type for DiskWatcher.
    /// </summary>
    public class DiskWatcherCheckResult : WatcherCheckResult
    {
        public DiskCheck DiskCheck { get; }

        protected DiskWatcherCheckResult(DiskWatcher watcher, bool isValid, string description,
            DiskCheck diskCheck)
            : base(watcher, isValid, description)
        {
            DiskCheck = diskCheck;
        }

        /// <summary>
        /// Factory method for creating a new instance of DiskWatcherCheckResult.
        /// </summary>
        /// <param name="watcher">Instance of DiskWatcher.</param>
        /// <param name="isValid">Flag determining whether the performed check was valid.</param>
        /// <param name="diskCheck">Instance of DiskCheck.</param>
        /// <param name="description">Custom description of the performed check.</param>
        /// <returns>Instance of DiskWatcherCheckResult.</returns>
        public static DiskWatcherCheckResult Create(DiskWatcher watcher, bool isValid,
            DiskCheck diskCheck, string description = "")
            => new DiskWatcherCheckResult(watcher, isValid, description, diskCheck);
    }
}