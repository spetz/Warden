namespace Warden.Watchers.Port
{
    using System;
    using System.Net;
    using System.Net.NetworkInformation;
    using System.Threading.Tasks;

    /// <summary>
    /// Provides a functionality to ping specified address.
    /// </summary>
    public interface IPingProvider : IDisposable
    {
        /// <summary>
        /// Sends a ICMP echo request to the specified address ip.
        /// </summary>
        /// <param name="addressIp">A destination address ip.</param>
        /// <param name="timeout">Timeout for connection</param>
        /// <returns>ICMP echo request status.</returns>
        Task<IPStatus> PingAsync(IPAddress addressIp, TimeSpan? timeout);
    }

    public class PingProvider : IPingProvider
    {

        /// <summary>
        /// Sends a ICMP echo request to the specified address ip.
        /// </summary>
        /// <param name="addressIp">A destination address ip.</param>
        /// <param name="timeout">Timeout for connection</param>
        /// <returns>ICMP echo request status.</returns>
        public Task<IPStatus> PingAsync(IPAddress addressIp, TimeSpan? timeout)
        {
            var ping = new Ping();

            if (timeout.HasValue)
            {
                return
                    ping.SendPingAsync(addressIp, timeout.Value.Milliseconds)
                        .ContinueWith(pingTask => pingTask.Result.Status);
            }

            return
                ping.SendPingAsync(addressIp)
                    .ContinueWith(pingTask => pingTask.Result.Status);
        }

        public void Dispose()
        {
        }
    }
}
