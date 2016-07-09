namespace Warden.Watchers.Server
{
    /// <summary>
    /// Custom check result type for ServerWatcher.
    /// </summary>    
    public class ServerWatcherCheckResult : WatcherCheckResult
    {
        public ConnectionInfo ConnectionInfo { get; }

        protected ServerWatcherCheckResult(ServerWatcher watcher, bool isValid, 
            string description, ConnectionInfo connectionInfo)
            : base(watcher, isValid, description)
        {
            ConnectionInfo = connectionInfo;
        }

        /// <summary>
        /// Factory method for creating a new instance of ServerWatcherCheckResult.
        /// </summary>
        /// <param name="watcher">Instance of ServerWatcher.</param>
        /// <param name="isValid">Flag determining whether the performed check was valid.</param>
        /// <param name="connectionInfo">Details of the resolved connection to the specified hostname and port.</param>
        /// <param name="description">Custom description of the performed check.</param>
        /// <returns>Instance of ServerWatcherCheckResult.</returns>
        public static ServerWatcherCheckResult Create(ServerWatcher watcher, bool isValid,
            ConnectionInfo connectionInfo, string description = "")
            => new ServerWatcherCheckResult(watcher, isValid, description, connectionInfo);
    }
}
