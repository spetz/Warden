using System;
using System.Collections.Generic;
using System.Linq;
using Warden.Core;

namespace Warden
{
    /// <summary>
    /// IWardenIteration has the number (ordinal) of the executed iteration (e.g. 1,2,3 ... N) and the collection of the IWardenCheckResult. 
    /// For example, let's say you have added 3 watchers this list will then contain 3 elements.
    /// </summary>
    public interface IWardenIteration : IValidatable, ITimestampable
    {
        /// <summary>
        /// Name of the Warden that has executed the iteration.
        /// </summary>
        string WardenName { get; }

        /// <summary>
        /// Number of the iteration.
        /// </summary>
        long Ordinal { get; }

        /// <summary>
        /// Collection containing all of the watcher check results executed in this iteration.
        /// </summary>
        IEnumerable<IWardenCheckResult> Results { get; }
    }

    public class WardenIteration : IWardenIteration
    {
        public string WardenName { get; }
        public long Ordinal { get; }
        public IEnumerable<IWardenCheckResult> Results { get; }
        public DateTime StartedAt { get; }
        public DateTime CompletedAt { get; }
        public TimeSpan ExecutionTime => CompletedAt - StartedAt;
        public bool IsValid => Results.All(x => x.IsValid);

        protected WardenIteration(string wardenName, long ordinal, IEnumerable<IWardenCheckResult> results,
            DateTime startedAt, DateTime completedAt)
        {
            if (string.IsNullOrWhiteSpace(wardenName))
                throw new ArgumentException("Warden name can not be empty.", nameof(wardenName));
            if (ordinal < 0)
            {
                throw new ArgumentException($"Warden iteration ordinal can not be less than 0 ({ordinal}).",
                    nameof(ordinal));
            }

            WardenName = wardenName;
            Ordinal = ordinal;
            Results = results;
            StartedAt = startedAt;
            CompletedAt = completedAt;
        }

        /// <summary>
        /// Factory method for creating a new instance of the IWardenIteration.
        /// </summary>
        /// <param name="wardenName">Name of the Warden that has executed the iteration.</param>
        /// <param name="ordinal">Number of executed iteration.</param>
        /// <param name="results">Collection of IWardenCheckResult that were created during the iteration.</param>
        /// <param name="startedAt">Date and time at which the iteration started.</param>
        /// <param name="completedAt">Date and time at which the iteration completedAt.</param>
        /// <returns>Instance of IWardenIteration.</returns>
        public static IWardenIteration Create(string wardenName, long ordinal, IEnumerable<IWardenCheckResult> results,
            DateTime startedAt, DateTime completedAt)
            => new WardenIteration(wardenName, ordinal, results, startedAt, completedAt);
    }
}