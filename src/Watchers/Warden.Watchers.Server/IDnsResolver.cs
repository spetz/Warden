using System;
using System.Net;
using System.Linq;

namespace Warden.Watchers.Server
{
    /// <summary>
    /// Custom DNS resolver service.
    /// </summary>
    public interface IDnsResolver : IDisposable
    {
        /// <summary>
        /// Gets the IP address or hostname.
        /// </summary>
        /// <param name="hostnameOrIp">A hostname or IPv4 address.</param>
        /// <returns>IP address of the resolved hostname (if exists).</returns>
        IPAddress GetIpAddress(string hostnameOrIp);
    }

    /// <summary>
    /// Default implementation of the IDnsResolver.
    /// </summary>
    public class DnsResolver : IDnsResolver
    {
        /// <summary>
        /// Gets the IP address of provided hostname or provider.
        /// </summary>
        /// <param name="hostnameOrIp">A hostname or IPv4 address.</param>
        /// <returns>An IP address or null if cannot be resolved.</returns>
        public IPAddress GetIpAddress(string hostnameOrIp) => Dns.GetHostAddresses(hostnameOrIp).FirstOrDefault();

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
        }
    }
}
