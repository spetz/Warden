using Warden.Core;

namespace Warden.Watchers
{
    /// <summary>
    /// Holds the information about the watcher check result. 
    /// This interface is implemented by the specific watchers which allows to pass the custom data. 
    /// It's being used by most of the watcher hooks.
    /// </summary>
    public interface IWatcherCheckResult : IValidatable, IWatcherCheck
    {
        /// <summary>
        /// Custom description of the performed watcher check.
        /// </summary>
        string Description { get; }
    }

    public class WatcherCheckResult : WatcherCheck, IWatcherCheckResult
    {
        public string Description { get; }
        public bool IsValid { get; }

        protected WatcherCheckResult(IWatcher watcher, bool isValid, string description) : base(watcher)
        {
            Description = description;
            IsValid = isValid;
        }

        /// <summary>
        /// Factory method for creating a new instance of the IWatcherCheckResult.
        /// </summary>
        /// <param name="watcher">Instance of IWatcher.</param>
        /// <param name="isValid">Flag determining whether the performed check was valid.</param>
        /// <param name="description">Custom description of the performed check.</param>
        /// <returns>Instance of IWatcherCheckResult.</returns>
        public static IWatcherCheckResult Create(IWatcher watcher, bool isValid, string description = "")
            => new WatcherCheckResult(watcher, isValid, description);
    }
}