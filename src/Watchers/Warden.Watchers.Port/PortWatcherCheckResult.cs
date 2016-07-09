namespace Warden.Watchers.Port
{
    /// <summary>
    /// Custom check result type for PortWatcher.
    /// </summary>    
    public class PortWatcherCheckResult : WatcherCheckResult
    {
        public ConnectionInfo ConnectionInfo { get; }

        protected PortWatcherCheckResult(PortWatcher watcher, bool isValid, 
            string description, ConnectionInfo connectionInfo)
            : base(watcher, isValid, description)
        {
            ConnectionInfo = connectionInfo;
        }

        /// <summary>
        /// Factory method for creating a new instance of PortWatcherCheckResult.
        /// </summary>
        /// <param name="watcher">Instance of PortWatcher.</param>
        /// <param name="isValid">Flag determining whether the performed check was valid.</param>
        /// <param name="connectionInfo">Details of the resolved connection to the specified hostname and port.</param>
        /// <param name="description">Custom description of the performed check.</param>
        /// <returns>Instance of PortWatcherCheckResult.</returns>
        public static PortWatcherCheckResult Create(PortWatcher watcher, bool isValid,
            ConnectionInfo connectionInfo, string description = "")
            => new PortWatcherCheckResult(watcher, isValid, description, connectionInfo);
    }
}
