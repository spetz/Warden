namespace Warden.Watchers.Process
{
    /// <summary>
    /// Custom check result type for ProcessWatcher.
    /// </summary>
    public class ProcessWatcherCheckResult : WatcherCheckResult
    {
        /// <summary>
        /// Name of the process.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Flag determining whether the process exists.
        /// </summary>
        public bool Exists { get; }

        /// <summary>
        /// State of the process.
        /// </summary>
        public ProcessState State { get; }

        protected ProcessWatcherCheckResult(ProcessWatcher watcher, bool isValid, string description, 
            string name, bool exists, ProcessState state)
            : base(watcher, isValid, description)
        {
            Name = name;
            Exists = exists;
            State = state;
        }

        /// <summary>
        /// Factory method for creating a new instance of ProcessWatcherCheckResult.
        /// </summary>
        /// <param name="watcher">Instance of ProcessWatcher.</param>
        /// <param name="isValid">Flag determining whether the performed check was valid.</param>
        /// <param name="name">Name of the process.</param>
        /// <param name="exists">Flag determining whether the process exists.</param>
        /// <param name="state"> State of the process.</param>
        /// <returns>Instance of ProcessWatcherCheckResult.</returns>
        public static ProcessWatcherCheckResult Create(ProcessWatcher watcher, bool isValid,
            string name, bool exists, ProcessState state, string description = "")
            => new ProcessWatcherCheckResult(watcher, isValid, description, name, exists, state);
    }
}