using System;
using System.Collections.Generic;
using System.Linq;

namespace Sentry
{
    public interface ISentryIteration : IValidatable, ITimestampable
    {
        long Ordinal { get; }
        IEnumerable<ISentryCheckResult> Results { get; }
    }

    public class SentryIteration : ISentryIteration
    {
        public long Ordinal { get; }
        public IEnumerable<ISentryCheckResult> Results { get; }
        public DateTime StartedAt { get; }
        public DateTime CompletedAt { get; }
        public TimeSpan ExecutionTime => CompletedAt - StartedAt;
        public bool IsValid => Results.All(x => x.IsValid);

        protected SentryIteration(long ordinal, IEnumerable<ISentryCheckResult> results, DateTime startedAt,
            DateTime completedAt)
        {
            if (ordinal < 0)
            {
                throw new ArgumentException($"Sentry iteration ordinal can not be less than 0 ({ordinal}).",
                    nameof(ordinal));
            }
            Ordinal = ordinal;
            Results = results;
            StartedAt = startedAt;
            CompletedAt = completedAt;
        }

        public static SentryIteration Create(long ordinal, IEnumerable<ISentryCheckResult> results,
            DateTime startedAt, DateTime completedAt)
            => new SentryIteration(ordinal, results, startedAt, completedAt);
    }
}