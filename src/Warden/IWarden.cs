using System.Threading;
using System.Threading.Tasks;

namespace Warden
{
    /// <summary>
    /// Core interface responsible for executing the watchers, hooks and integrations.
    /// </summary>
    public interface IWarden
    {
        /// <summary>
        /// Customizable name of the Warden.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Start the Warden. It will be running iterations in a loop (infinite by default but can be changed) and executing all of the configured hooks.
        /// </summary>
        /// <returns></returns>
        Task StartAsync();

        /// <summary>
        /// Pause the Warden. It will not reset the current iteration number (ordinal) back to 1. Can be resumed by invoking StartAsync().
        /// </summary>
        /// <returns></returns>
        Task PauseAsync();

        /// <summary>
        /// Stop the Warden. It will reset the current iteration number (ordinal) back to 1. Can be resumed by invoking StartAsync().
        /// </summary>
        /// <returns></returns>
        Task StopAsync();
    }
}