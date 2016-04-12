using System;

namespace Warden.Core
{
    /// <summary>
    /// Common interface containing the dates and times of the watcher check execution.
    /// </summary>
    public interface ITimestampable
    {
        /// <summary>
        /// Date and time at which the operation started.
        /// </summary>
        DateTime StartedAt { get; }

        /// <summary>
        /// Date and time at which the operation completed.
        /// </summary>
        DateTime CompletedAt { get; }

        /// <summary>
        /// Amount of time required to start and complete the operation.
        /// </summary>
        TimeSpan ExecutionTime { get; }
    }
}