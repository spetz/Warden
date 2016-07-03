namespace Warden.Watchers.Port
{
    /// <summary>
    /// Custom check result type for PortAvailbilityWatcher.
    /// </summary>    
    public class PortCheckResult : WatcherCheckResult
    {
        /// <summary>
        /// Hostname or IP address that was checked.
        /// </summary>
        public string Host { get; private set; }

        /// <summary>
        /// Port that was checked.
        /// </summary>
        public int Port { get; private set; }

        protected PortCheckResult(PortWatcher watcher, bool isValid, string description,
            string host, int port)
            : base(watcher, isValid, description)
        {
            Host = host;
            Port = port;
        }

        /// <summary>
        /// Factory method for creating a new instance of PortCheckResult.
        /// </summary>
        /// <param name="watcher">Instance of PortWatcher.</param>
        /// <param name="isValid">Flag determining whether the performed check was valid.</param>
        /// <param name="host">Hostname that was checked by watcher.</param>
        /// <param name="port">Port that was checked by watcher.</param>
        /// <param name="description">Custom description of the performed check.</param>
        /// <returns>Instance of PortWatcherCheckResult.</returns>
        public static PortCheckResult Create(PortWatcher watcher, bool isValid,
            string host, int port, string description = "")
            => new PortCheckResult(watcher, isValid, description, host, port);
    }
}
