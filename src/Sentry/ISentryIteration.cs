using System;
using System.Collections.Generic;
using System.Linq;

namespace Sentry
{
    public interface ISentryIteration : ITimestampable
    {
        long Ordinal { get; }
        IEnumerable<ISentryCheckResult> Results { get; }
        bool IsValid { get; }
    }

    public class SentryIteration : ISentryIteration
    {
        public long Ordinal { get; }
        public IEnumerable<ISentryCheckResult> Results { get; }
        public DateTime StartedAtUtc { get; }
        public DateTime CompletedAtUtc { get; }
        public TimeSpan ExecutionTime => CompletedAtUtc - StartedAtUtc;
        public bool IsValid => Results.All(x => x.IsValid);

        protected SentryIteration(long ordinal, IEnumerable<ISentryCheckResult> results, DateTime startedAtUtc,
            DateTime completedAtUtc)
        {
            if (ordinal < 0)
            {
                throw new ArgumentException($"Sentry iteration ordinal can not be less than 0 ({ordinal}).",
                    nameof(ordinal));
            }
            Ordinal = ordinal;
            Results = results;
            StartedAtUtc = startedAtUtc;
            CompletedAtUtc = completedAtUtc;
        }

        public static SentryIteration Create(long ordinal, IEnumerable<ISentryCheckResult> results,
            DateTime startedAtUtc, DateTime completedAtUtc)
            => new SentryIteration(ordinal, results, startedAtUtc, completedAtUtc);
    }
}