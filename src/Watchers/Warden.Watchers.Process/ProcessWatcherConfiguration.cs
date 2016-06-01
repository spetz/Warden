using System;
using System.Threading.Tasks;

namespace Warden.Watchers.Process
{
    /// <summary>
    /// Configuration of the ProcessWatcher.
    /// </summary>
    public class ProcessWatcherConfiguration
    {
        /// <summary>
        /// Name of the process.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Predicate that has to be satisfied in order to return the valid result.
        /// </summary>
        public Func<ProcessInfo, bool> EnsureThat { get; protected set; }

        /// <summary>
        /// Async predicate that has to be satisfied in order to return the valid result.
        /// </summary>
        public Func<ProcessInfo, Task<bool>> EnsureThatAsync { get; protected set; }
    }
}