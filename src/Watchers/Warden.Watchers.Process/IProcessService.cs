using System.Threading.Tasks;
using System.Linq;

namespace Warden.Watchers.Process
{
    /// <summary>
    /// Custom service for checking process status.
    /// </summary>
    public interface IProcessService
    {
        /// <summary>
        /// Checks if the process exists and return its details.
        /// </summary>
        /// <param name="name">Name of the process.</param>
        /// <returns></returns>
        Task<ProcessInfo> GetProcessInfoAsync(string name);
    }

    /// <summary>
    /// Default implementation of the IProcessService based on System.Diagnostics.Process.
    /// </summary>
    public class ProcessService : IProcessService
    {
        public async Task<ProcessInfo> GetProcessInfoAsync(string name)
        {
            var processes = System.Diagnostics.Process.GetProcessesByName(name);
            var process = processes.FirstOrDefault();
            var processId = process?.Id ?? 0;
            var exists = process != null;
            var isResponding = true;

            //TODO: Fix this when .NET Core finally gets this property.
            //var isResponding = process?.Responding ?? false;

            return await Task.FromResult(ProcessInfo.Create(processId, name, exists, isResponding));
        }
    }
}