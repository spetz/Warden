using System;

namespace Sentry
{
    /// <summary>
    /// Contains the specific IWatcherCheckResult type and also the dates and times at which the watcher check has been executed. 
    /// This interface is used by most of the hooks, such as: OnSuccess(), OnSuccessAsync(), OnFirstSuccess(), OnFirstSuccessAsync(), 
    /// OnFailure(), OnFailureAsync(), OnFirstFailure(), OnFirstFailureAsync(), OnCompleted() and OnCompletedAsync().
    /// </summary>
    public interface ISentryCheckResult : IValidatable, ITimestampable
    {
        /// <summary>
        /// Holds the result of the specific watcher check. 
        /// Can be casted down to its custom result type in order to get the specialized data.
        /// </summary>
        IWatcherCheckResult WatcherCheckResult { get; }
    }

    /// <summary>
    /// Default implementation of ISentryCheckResult.
    /// </summary>
    public class SentryCheckResult : ISentryCheckResult
    {
        public IWatcherCheckResult WatcherCheckResult { get; }
        public DateTime StartedAt { get; }
        public DateTime CompletedAt { get; }
        public TimeSpan ExecutionTime => CompletedAt - StartedAt;
        public bool IsValid => WatcherCheckResult.IsValid;

        protected SentryCheckResult(IWatcherCheckResult watcherCheckResult, DateTime startedAt,
            DateTime completedAt)
        {
            if (watcherCheckResult == null)
                throw new ArgumentNullException(nameof(watcherCheckResult), "Watcher check result can not be null.");

            WatcherCheckResult = watcherCheckResult;
            StartedAt = startedAt;
            CompletedAt = completedAt;
        }

        /// <summary>
        /// Factory method for creating a new instance of ISentryCheckResult.
        /// </summary>
        /// <returns>Instance of ISentryCheckResult.</returns>
        public static ISentryCheckResult Create(IWatcherCheckResult watcherCheckResult, DateTime startedAt,
            DateTime completedAt)
            => new SentryCheckResult(watcherCheckResult, startedAt, completedAt);
    }
}