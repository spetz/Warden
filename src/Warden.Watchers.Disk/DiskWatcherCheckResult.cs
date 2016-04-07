namespace Warden.Watchers.Disk
{
    /// <summary>
    /// Custom check result type for DiskWatcher.
    /// </summary>
    public class DiskWatcherCheckResult : WatcherCheckResult
    {
        protected DiskWatcherCheckResult(DiskWatcher watcher, bool isValid, string description)
            : base(watcher, isValid, description)
        {
        }

        /// <summary>
        /// Factory method for creating a new instance of DiskWatcherCheckResult.
        /// </summary>
        /// <param name="watcher">Instance of DiskWatcher.</param>
        /// <param name="isValid">Flag determining whether the performed check was valid.</param>
        /// <param name="description">Custom description of the performed check.</param>
        /// <returns>Instance of DiskWatcherCheckResult.</returns>
        public static DiskWatcherCheckResult Create(DiskWatcher watcher, bool isValid,
            string description = "")
            => new DiskWatcherCheckResult(watcher, isValid, description);
    }
}