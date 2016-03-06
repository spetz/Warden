using System;

namespace Sentry
{
    public interface ISentryOutcome
    {
        IWatcherOutcome WatcherOutcome { get; }
        DateTime StartedAtUtc { get; }
        DateTime CompletedAtUtc { get; }
        TimeSpan ExecutionTime { get; }
        Exception Exception { get; }
    }

    public class SentryOutcome : ISentryOutcome
    {
        public IWatcherOutcome WatcherOutcome { get; }
        public DateTime StartedAtUtc { get; }
        public DateTime CompletedAtUtc { get; }
        public Exception Exception { get; }
        public TimeSpan ExecutionTime => CompletedAtUtc - StartedAtUtc;

        protected SentryOutcome(IWatcherOutcome watcherOutcome, DateTime startedAtUtc, DateTime completedAtUtc,
            Exception exception = null)
        {
            if (watcherOutcome == null)
                throw new ArgumentNullException(nameof(watcherOutcome), "Watcher outcome can not be null.");

            WatcherOutcome = watcherOutcome;
            StartedAtUtc = startedAtUtc;
            CompletedAtUtc = completedAtUtc;
            Exception = exception;
        }

        public static SentryOutcome Valid(IWatcherOutcome watcherOutcome, DateTime startedAt,
            DateTime completedAt) => Create(watcherOutcome, startedAt, completedAt);

        public static SentryOutcome Invalid(IWatcherOutcome watcherOutcome, DateTime startedAt,
            DateTime completedAt, Exception exception)
            => Create(watcherOutcome, startedAt, completedAt, exception);

        private static SentryOutcome Create(IWatcherOutcome watcherOutcome, DateTime startedAt,
            DateTime completedAt, Exception exception = null)
            => new SentryOutcome(watcherOutcome, startedAt, completedAt, exception);
    }
}