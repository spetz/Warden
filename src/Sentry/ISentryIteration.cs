using System;
using System.Collections.Generic;
using System.Linq;

namespace Sentry
{
    /// <summary>
    /// ISentryIteration has the number (ordinal) of the executed iteration (e.g. 1,2,3 ... N) and the collection of the ISentryCheckResult. 
    /// For example, let's say you have added 3 watchers this list will then contain 3 elements.
    /// </summary>
    public interface ISentryIteration : IValidatable, ITimestampable
    {
        /// <summary>
        /// Number of the iteration.
        /// </summary>
        long Ordinal { get; }

        /// <summary>
        /// Collection containing all of the watcher check results executed in this iteration.
        /// </summary>
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

        /// <summary>
        /// Factory method for creating a new instance of the ISentryIteration.
        /// </summary>
        /// <param name="ordinal">Number of executed iteration.</param>
        /// <param name="results">Collection of ISentryCheckResult that were created during the iteration.</param>
        /// <param name="startedAt">Date and time at which the iteration started.</param>
        /// <param name="completedAt">Date and time at which the iteration completedAt.</param>
        /// <returns>Instance of ISentryIteration.</returns>
        public static ISentryIteration Create(long ordinal, IEnumerable<ISentryCheckResult> results,
            DateTime startedAt, DateTime completedAt)
            => new SentryIteration(ordinal, results, startedAt, completedAt);
    }
}