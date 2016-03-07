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
        public DateTime StartedAtUtc { get; }
        public DateTime CompletedAtUtc { get; }
        public TimeSpan ExecutionTime => CompletedAtUtc - StartedAtUtc;
        public bool IsValid => WatcherCheckResult.IsValid;

        protected SentryCheckResult(IWatcherCheckResult watcherCheckResult, DateTime startedAtUtc,
            DateTime completedAtUtc)
        {
            if (watcherCheckResult == null)
                throw new ArgumentNullException(nameof(watcherCheckResult), "Watcher check result can not be null.");

            WatcherCheckResult = watcherCheckResult;
            StartedAtUtc = startedAtUtc;
            CompletedAtUtc = completedAtUtc;
        }

        public static SentryCheckResult Create(IWatcherCheckResult watcherCheckResult, DateTime startedAt,
            DateTime completedAt)
            => new SentryCheckResult(watcherCheckResult, startedAt, completedAt);
    }
}