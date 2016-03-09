using System;

namespace Sentry
{
    public interface ISentryCheckResult : ITimestampable
    {
        IWatcherCheckResult WatcherCheckResult { get; }
        bool IsValid { get; }
    }

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

        public static ISentryCheckResult Create(IWatcherCheckResult watcherCheckResult, DateTime startedAt,
            DateTime completedAt)
            => new SentryCheckResult(watcherCheckResult, startedAt, completedAt);
    }
}