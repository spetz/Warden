namespace Warden.Watchers.ServerStatus
{
    using System.Linq;
    using System.Net;

    /// <summary>
    /// A service to handle dns requests.
    /// </summary>
    public interface IDnsResolver
    {
        /// <summary>
        /// Gets the IP address of provider hostname or provider.
        /// </summary>
        /// <param name="hostnameOrIp">A host name or IPv4 address.</param>
        /// <returns>An IP address or null if cannot be resolved.</returns>
        IPAddress GetIp(string hostnameOrIp);
    }

    internal class DefaultDnsResolver : IDnsResolver
    {
        /// <summary>
        /// Gets the IP address of provider hostname or provider.
        /// </summary>
        /// <param name="hostnameOrIp">A host name or IPv4 address.</param>
        /// <returns>An IP address or null if cannot be resolved.</returns>
        public IPAddress GetIp(string hostnameOrIp)
        {
            return Dns.GetHostAddresses(hostnameOrIp).FirstOrDefault();
        }
    }
}
