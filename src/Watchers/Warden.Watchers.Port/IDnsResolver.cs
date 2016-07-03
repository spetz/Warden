namespace Warden.Watchers.Port
{
    using System;
    using System.Linq;
    using System.Net;

    /// <summary>
    /// A service that handle dns requests.
    /// </summary>
    public interface IDnsResolver : IDisposable
    {
        /// <summary>
        /// Gets the IP address or hostname.
        /// </summary>
        /// <param name="hostnameOrIp">A host name or IPv4 address.</param>
        /// <returns>An IP address or null if cannot be resolved.</returns>
        IPAddress GetIp(string hostnameOrIp);
    }

    public class DnsResolver : IDnsResolver
    {
        /// <summary>
        /// Gets the IP address of provider hostname or provider.
        /// </summary>
        /// <param name="hostnameOrIp">A host name or IPv4 address.</param>
        /// <returns>An IP address or null if cannot be resolved.</returns>
        public IPAddress GetIp(string hostnameOrIp) => Dns.GetHostAddresses(hostnameOrIp).FirstOrDefault();

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
        }
    }
}
