using System;
using Warden.Core;
using Warden.Watchers;

namespace Warden
{
    /// <summary>
    /// Contains the specific IWatcherCheckResult type and also the dates and times at which the watcher check has been executed. 
    /// This interface is used by most of the hooks, such as: OnSuccess(), OnSuccessAsync(), OnFirstSuccess(), OnFirstSuccessAsync(), 
    /// OnFailure(), OnFailureAsync(), OnFirstFailure(), OnFirstFailureAsync(), OnCompleted() and OnCompletedAsync().
    /// </summary>
    public interface IWardenCheckResult : IValidatable, ITimestampable
    {
        /// <summary>
        /// Holds the result of the specific watcher check. 
        /// Can be casted down to its custom result type in order to get the specialized data.
        /// </summary>
        IWatcherCheckResult WatcherCheckResult { get; }
    }

    /// <summary>
    /// Default implementation of IWardenCheckResult.
    /// </summary>
    public class WardenCheckResult : IWardenCheckResult
    {
        public IWatcherCheckResult WatcherCheckResult { get; }
        public DateTime StartedAt { get; }
        public DateTime CompletedAt { get; }
        public TimeSpan ExecutionTime => CompletedAt - StartedAt;
        public bool IsValid => WatcherCheckResult.IsValid;

        protected WardenCheckResult(IWatcherCheckResult watcherCheckResult, DateTime startedAt,
            DateTime completedAt)
        {
            if (watcherCheckResult == null)
                throw new ArgumentNullException(nameof(watcherCheckResult), "Watcher check result can not be null.");

            WatcherCheckResult = watcherCheckResult;
            StartedAt = startedAt;
            CompletedAt = completedAt;
        }

        /// <summary>
        /// Factory method for creating a new instance of IWardenCheckResult.
        /// </summary>
        /// <returns>Instance of IWardenCheckResult.</returns>
        public static IWardenCheckResult Create(IWatcherCheckResult watcherCheckResult, DateTime startedAt,
            DateTime completedAt)
            => new WardenCheckResult(watcherCheckResult, startedAt, completedAt);
    }
}