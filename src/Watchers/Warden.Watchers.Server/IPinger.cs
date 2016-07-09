using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace Warden.Watchers.Server
{
    /// <summary>
    /// Provides a functionality to ping specified address.
    /// </summary>
    public interface IPinger : IDisposable
    {
        /// <summary>
        /// Sends the ICMP echo request to the specified IP address.
        /// </summary>
        /// <param name="addressIp">A destination IP address.</param>
        /// <param name="timeout">Optional timeout for connection.</param>
        /// <returns>ICMP echo request status.</returns>
        Task<IPStatus> PingAsync(IPAddress addressIp, TimeSpan? timeout = null);
    }

    public class Pinger : IPinger
    {
        /// <summary>
        /// Sends the ICMP echo request to the specified IP address.
        /// </summary>
        /// <param name="addressIp">A destination IP address.</param>
        /// <param name="timeout">Optional timeout for connection.</param>
        /// <returns>ICMP echo request status.</returns>
        public async Task<IPStatus> PingAsync(IPAddress addressIp, TimeSpan? timeout = null)
        {
            var ping = new Ping();
            if (timeout.HasValue)
            {
                return await ping.SendPingAsync(addressIp, timeout.Value.Milliseconds)
                    .ContinueWith(pingTask => pingTask.Result.Status);
            }

            return await ping.SendPingAsync(addressIp)
                .ContinueWith(pingTask => pingTask.Result.Status);
        }

        public void Dispose()
        {
        }
    }
}
