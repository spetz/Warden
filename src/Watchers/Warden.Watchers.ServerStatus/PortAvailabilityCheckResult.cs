namespace Warden.Watchers.ServerStatus
{
    /// <summary>
    /// Custom check result type for PortAvailbilityWatcher.
    /// </summary>    
    public class PortAvailabilityCheckResult : WatcherCheckResult
    {
        /// <summary>
        /// Host name that was checked.
        /// </summary>
        public string Host { get; private set; }

        /// <summary>
        /// Port that was checked.
        /// </summary>
        public int Port { get; private set; }

        protected PortAvailabilityCheckResult(PortAvailabilityWatcher watcher, bool isValid, string description,
            string host, int port)
            : base(watcher, isValid, description)
        {
            this.Host = host;
            this.Port = port;
        }

        /// <summary>
        /// Factory method for creating a new instance of PortAvailbilityCheckResult.
        /// </summary>
        /// <param name="watcher">Instance of PortAvailabilityWatcher.</param>
        /// <param name="isValid">Flag determining whether the performed check was valid.</param>
        /// <param name="host">Host name that was checked by watcher.</param>
        /// <param name="port">Port that was checked by watcher.</param>
        /// <param name="description">Custom description of the performed check.</param>
        /// <returns>Instance of WebWatcherCheckResult.</returns>
        public static PortAvailabilityCheckResult Create(PortAvailabilityWatcher watcher, bool isValid,
            string host, int port, string description = "")
            => new PortAvailabilityCheckResult(watcher, isValid, description, host, port);
    }
}
