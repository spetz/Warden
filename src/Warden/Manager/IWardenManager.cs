using System.Threading.Tasks;

namespace Warden.Manager
{
    /// <summary>
    /// Warden Manager is responsible for handling the commands and events that control the running instance of the Warden.
    /// </summary>
    public interface IWardenManager
    {
        /// <summary>
        /// Start the Warden Manager using the underlying IWarden instance.
        /// </summary>
        /// <returns></returns>
        Task StartAsync();

        /// <summary>
        /// Stop the Warden Manager including the underlying IWarden instance.
        /// </summary>
        /// <returns></returns>
        Task StopAsync();
    }
}