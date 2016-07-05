namespace Warden.Watchers.Port
{
    /// <summary>
    /// Custom check result type for PortWatcher.
    /// </summary>    
    public class PortWatcherCheckResult : WatcherCheckResult
    {
        /// <summary>
        /// Hostname or IP address that was resolved.
        /// </summary>
        public string Hostname { get; }

        /// <summary>
        /// Port number that was checked.
        /// </summary>
        public int Port { get; }

        protected PortWatcherCheckResult(PortWatcher watcher, bool isValid, string description,
            string hostname, int port)
            : base(watcher, isValid, description)
        {
            Hostname = hostname;
            Port = port;
        }

        /// <summary>
        /// Factory method for creating a new instance of PortWatcherCheckResult.
        /// </summary>
        /// <param name="watcher">Instance of PortWatcher.</param>
        /// <param name="isValid">Flag determining whether the performed check was valid.</param>
        /// <param name="hostname">Resolved hostname.</param>
        /// <param name="port">Checked port number.</param>
        /// <param name="description">Custom description of the performed check.</param>
        /// <returns>Instance of PortWatcherCheckResult.</returns>
        public static PortWatcherCheckResult Create(PortWatcher watcher, bool isValid,
            string hostname, int port, string description = "")
            => new PortWatcherCheckResult(watcher, isValid, description, hostname, port);
    }
}
