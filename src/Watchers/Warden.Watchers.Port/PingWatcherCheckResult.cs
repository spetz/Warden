namespace Warden.Watchers.Port
{
    /// <summary>
    /// Custom check result type for PortWatcher.
    /// </summary>    
    public class PingWatcherCheckResult : WatcherCheckResult
    {
        /// <summary>
        /// Hostname or IP address that was resolved.
        /// </summary>
        public string Hostname { get; }

        protected PingWatcherCheckResult(PingWatcher watcher, bool isValid, string description,
            string hostname)
            : base(watcher, isValid, description)
        {
            Hostname = hostname;
        }

        /// <summary>
        /// Factory method for creating a new instance of PortWatcherCheckResult.
        /// </summary>
        /// <param name="watcher">Instance of PingWatcher.</param>
        /// <param name="isValid">Flag determining whether the performed check was valid.</param>
        /// <param name="hostname">Resolved hostname.</param>
        /// <param name="description">Custom description of the performed check.</param>
        /// <returns>Instance of PortWatcherCheckResult.</returns>
        public static PingWatcherCheckResult Create(PingWatcher watcher, bool isValid,
            string hostname, string description = "")
            => new PingWatcherCheckResult(watcher, isValid, description, hostname);
    }
}
