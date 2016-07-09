using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace Warden.Watchers.Port
{
    /// <summary>
    /// PortWatcher designed for monitoring hostname availability including available ports (if specified).
    /// </summary>
    public class PortWatcher : IWatcher
    {
        private readonly PortWatcherConfiguration _configuration;

        private readonly Dictionary<IPStatus, string> _pingStatusMessages = new Dictionary<IPStatus, string>
        {
            {IPStatus.Unknown, "ICMP echo request to host '{0}' failed because of uknown error." },
            {IPStatus.Success, "Success."},
            {IPStatus.BadDestination, "ICMP echo request failed. Host '{0}' cannot receive ICMP echo requests."},
            {IPStatus.BadHeader, "ICMP echo request to host '{0}' failed because of invalid header." },
            {IPStatus.BadOption, "ICMP echo request to host '{0}' failed contains invalid option." },
            {IPStatus.BadRoute, "ICMP echo request failed because there is no valid route between source and host '{0}'." },
            {IPStatus.DestinationHostUnreachable, "ICMP echo request failed because host '{0}' is not reachable." },
            {IPStatus.DestinationNetworkUnreachable, "ICMP echo request failed because network that contains host '{0}' is not reachable." },
            {IPStatus.DestinationPortUnreachable, "ICMP echo request failed because the Ping on host '{0}' is not reachable." },
            {IPStatus.DestinationProtocolUnreachable, "ICMP echo request failed because host '{0}' doesn't supPing the packet's protocol." },
            {IPStatus.HardwareError, "ICMP echo request to host '{0}' failed because of hardware error." },
            {IPStatus.IcmpError, "ICMP echo request to host '{0}' failed because of ICMP protocol error." },
            {IPStatus.TtlExpired, "ICMP echo request to host '{0}' failed because its Time To Live (TTL) reached 0, so forwarding node (router or gateway) discared the request." },
            {IPStatus.TimedOut, "ICMP echo request failed because the reply from host '{0}' was not received in specified time." },
            {IPStatus.SourceQuench, "ICMP echo request failed because host '{0}' discarded the packet." },
            {IPStatus.NoResources, "ICMP echo request to host '{0}' failed because of insufficient sources." },
            {IPStatus.PacketTooBig, "ICMP echo request to host '{0}' failed because the packet is larger than MTU of node (router of gateway)." },
            {IPStatus.ParameterProblem, "ICMP echo request to host '{0}' failed because the node (router or gateway) failed while processing packet header." },
        };

        /// <summary>
        /// Default name of the PortWatcher.
        /// </summary>
        public const string DefaultName = "Port Watcher";

        /// <summary>
        /// Name of the PortWatcher.
        /// </summary>
        public string Name { get; }

        protected PortWatcher(string name, PortWatcherConfiguration configuration)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Watcher name can not be empty.");

            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration),
                    "Port Watcher configuration has not been provided.");
            }

            Name = name;
            _configuration = configuration;
        }

        public async Task<IWatcherCheckResult> ExecuteAsync()
        {
            try
            {
                return await ExecuteCheckAsync();
            }
            catch (TaskCanceledException exception)
            {
                var connectionInfo = ConnectionInfo.Create(_configuration.Hostname,
                    IPAddress.None, _configuration.Port, IPStatus.Unknown, string.Empty);

                return PortWatcherCheckResult.Create(this, false, connectionInfo,
                    "A connection timeout has occurred while trying to access the hostname: " +
                    $"'{_configuration.Hostname}' using port: {_configuration.Port}.");
            }
            catch (Exception exception)
            {
                throw new WatcherException("There was an error while trying to access the hostname: " +
                                           $"'{_configuration.Hostname}' using port: {_configuration.Port}.",
                    exception);
            }
        }

        private async Task<IWatcherCheckResult> ExecuteCheckAsync()
        {
            using (var servicesProvider = GetServicesProviders())
            {
                var ipStatusAndAddress = await TryConnectAsync(servicesProvider);
                var ipStatus = ipStatusAndAddress.Item1;
                var ipAddress = ipStatusAndAddress.Item2;
                var ipStatusMessage = _pingStatusMessages[ipStatus];
                var portSpecified = _configuration.Port > 0;
                var connectionInfo = ConnectionInfo.Create(_configuration.Hostname,
                    ipAddress, _configuration.Port, ipStatus, ipStatusMessage);


                if (ipStatus == IPStatus.Unknown)
                {
                    return PortWatcherCheckResult.Create(this, false, connectionInfo,
                        $"Could not resolve the hostname '{_configuration.Hostname}' " +
                        $"{(portSpecified ? $"using port: {_configuration.Port}" : string.Empty)}. " +
                        $"Ping status: '{ipStatusMessage}'");
                }
                if (ipStatus != IPStatus.Success)
                {
                    return PortWatcherCheckResult.Create(this, false, connectionInfo,
                        $"Unable to connect to the hostname '{_configuration.Hostname}' " +
                        $"{(portSpecified ? $"using port: {_configuration.Port}" : string.Empty)}. " +
                        $"Ping status: '{ipStatusMessage}'");
                }

                return await EnsureAsync(connectionInfo);
            }
        }

        private async Task<IWatcherCheckResult> EnsureAsync(ConnectionInfo connectionInfo)
        {
            var isValid = true;
            var portSpecified = _configuration.Port > 0;
            if (_configuration.EnsureThatAsync != null)
                isValid = await _configuration.EnsureThatAsync?.Invoke(connectionInfo);

            isValid = isValid && (_configuration.EnsureThat?.Invoke(connectionInfo) ?? true);

            return PortWatcherCheckResult.Create(this, isValid, connectionInfo,
                $"Successfully connected to the hostname '{_configuration.Hostname}' " +
                $"{(portSpecified ? $"using port: {_configuration.Port}" : string.Empty)}.");
        }

        /// <summary>
        /// Checks if port is available on the remote host.
        /// </summary>
        /// <param name="servicesProvider"></param>
        /// <returns>IPStatus, if remote host has accepted connection, otherwise unknown. None IP address if host could not be resolved.</returns>
        private async Task<Tuple<IPStatus, IPAddress>> TryConnectAsync(PortServicesProvider servicesProvider)
        {
            var ipAddress = servicesProvider.DnsResolver.GetIpAddress(_configuration.Hostname);
            if (Equals(ipAddress, IPAddress.None))
                return new Tuple<IPStatus, IPAddress>(IPStatus.Unknown, IPAddress.None);

            var ipStatus = await servicesProvider.Pinger.PingAsync(ipAddress, _configuration.Timeout);
            if (_configuration.Port > 0)
                await servicesProvider.TcpClient.ConnectAsync(ipAddress, _configuration.Port, _configuration.Timeout);

            return new Tuple<IPStatus, IPAddress>(ipStatus, ipAddress);
        }

        /// <summary>
        /// Creates PortServicesProvider instance.
        /// </summary>
        /// <returns>PortServicesProvider instance.</returns>
        private PortServicesProvider GetServicesProviders()
        {
            var client = _configuration.TcpClientProvider();
            var dnsResolver = _configuration.DnsResolverProvider();
            var pinger = _configuration.PingerProvider();

            return new PortServicesProvider(client, dnsResolver, pinger);
        }

        /// <summary>
        /// Class that holds useful providers to check the connection.
        /// </summary>
        private class PortServicesProvider : IDisposable
        {
            /// <summary>
            /// ITcpClient implementation instance.
            /// </summary>
            public ITcpClient TcpClient { get; }

            /// <summary>
            /// IPinger implementation instance.
            /// </summary>
            public IPinger Pinger { get; }

            /// <summary>
            /// IDnsResolver implementation instance.
            /// </summary>
            public IDnsResolver DnsResolver { get; }

            public PortServicesProvider(ITcpClient tcpClient, IDnsResolver dnsResolver, IPinger pinger)
            {
                TcpClient = tcpClient;
                DnsResolver = dnsResolver;
                Pinger = pinger;
            }

            public void Dispose()
            {
                TcpClient.Dispose();
                DnsResolver.Dispose();
                Pinger.Dispose();
            }
        }

        /// <summary>
        /// Factory method for creating a new instance of PortWatcher.
        /// </summary>
        /// <param name="hostname">Hostname to connect to.</param>
        /// <param name="port">Optional port number of the hostname (0 means not specified).</param>
        /// <param name="configurator">A configuration bulider that should be used by watcher.</param>
        /// <returns>A PortWatcher instance.</returns>
        public static PortWatcher Create(string hostname, int port = 0, Action<PortWatcherConfiguration.Default> configurator = null)
        {
            var config = new PortWatcherConfiguration.Builder(hostname, port);
            configurator?.Invoke((PortWatcherConfiguration.Default)config);

            return Create(DefaultName, config.Build());
        }

        /// <summary>
        /// Factory method for creating a new instance of PortWatcher.
        /// </summary>
        /// <param name="name">Name of the PortWatcher.</param>
        /// <param name="hostname">Hostname to connect to.</param>
        /// <param name="port">Optional port number of the hostname (0 means not specified).</param>
        /// <param name="configurator">A configuration that should be used by watcher.</param>
        /// <returns>A PortWatcher instance.</returns>
        public static PortWatcher Create(string name, string hostname, int port = 0, Action<PortWatcherConfiguration.Default> configurator = null)
        {
            var config = new PortWatcherConfiguration.Builder(hostname, port);
            configurator?.Invoke((PortWatcherConfiguration.Default)config);

            return Create(name, config.Build());
        }

        /// <summary>
        /// Factory method for creating a new instance of PortWatcher.
        /// </summary>
        /// <param name="name">Name of the PortWatcher.</param>
        /// <param name="configuration">Configuration of PortWatcher.</param>
        /// <returns>A PortWatcher instance.</returns>
        public static PortWatcher Create(string name, PortWatcherConfiguration configuration)
            => new PortWatcher(name, configuration);

        /// <summary>
        /// Factory method for creating a new instance of PortWatcher with default name of Port Watcher.
        /// </summary>
        /// <param name="configuration">Configuration of PortWatcher.</param>
        /// <returns>A PortWatcher instance.</returns>
        public static PortWatcher Create(PortWatcherConfiguration configuration)
            => new PortWatcher(DefaultName, configuration);
    }
}
