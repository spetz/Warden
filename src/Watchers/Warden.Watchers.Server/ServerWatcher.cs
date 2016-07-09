using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace Warden.Watchers.Server
{
    /// <summary>
    /// ServerWatcher designed for monitoring server availability including opened ports (if specified).
    /// </summary>
    public class ServerWatcher : IWatcher
    {
        private readonly ServerWatcherConfiguration _configuration;

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
        /// Default name of the ServerWatcher.
        /// </summary>
        public const string DefaultName = "Server Watcher";

        /// <summary>
        /// Name of the ServerWatcher.
        /// </summary>
        public string Name { get; }

        protected ServerWatcher(string name, ServerWatcherConfiguration configuration)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Watcher name can not be empty.");

            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration),
                    "Server watcher configuration has not been provided.");
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
                    IPAddress.None, _configuration.Port, false, IPStatus.Unknown, string.Empty);

                return ServerWatcherCheckResult.Create(this, false, connectionInfo,
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
                var ipStatusAndAddressAndOpenedPort = await TryConnectAsync(servicesProvider);
                var ipStatus = ipStatusAndAddressAndOpenedPort.Item1;
                var ipAddress = ipStatusAndAddressAndOpenedPort.Item2;
                var portOpened = ipStatusAndAddressAndOpenedPort.Item3;
                var ipStatusMessage = _pingStatusMessages[ipStatus];
                var portSpecified = _configuration.Port > 0;
                var connectionInfo = ConnectionInfo.Create(_configuration.Hostname,
                    ipAddress, _configuration.Port, portOpened, ipStatus, ipStatusMessage);

                if (ipStatus == IPStatus.Unknown)
                {
                    return ServerWatcherCheckResult.Create(this, false, connectionInfo,
                        $"Could not resolve the hostname '{_configuration.Hostname}' " +
                        $"{(portSpecified ? $"using port: {_configuration.Port}" : string.Empty)}. " +
                        $"Ping status: '{ipStatusMessage}'");
                }
                if (ipStatus != IPStatus.Success)
                {
                    return ServerWatcherCheckResult.Create(this, false, connectionInfo,
                        $"Unable to connect to the hostname '{_configuration.Hostname}' " +
                        $"{(portSpecified ? $"using port: {_configuration.Port}" : string.Empty)}. " +
                        $"Ping status: '{ipStatusMessage}'");
                }
                if (portSpecified && !portOpened)
                {
                    return ServerWatcherCheckResult.Create(this, false, connectionInfo,
                        $"Unable to connect to the hostname '{_configuration.Hostname}' " +
                        $"using port: {_configuration.Port}. " +
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

            return ServerWatcherCheckResult.Create(this, isValid, connectionInfo,
                $"Successfully connected to the hostname '{_configuration.Hostname}' " +
                $"{(portSpecified ? $"using port: {_configuration.Port}" : string.Empty)}.");
        }

        /// <summary>
        /// Resolves the hostname and checks whether the opened port (if specified) is being opened on the remote host.
        /// </summary>
        /// <param name="servicesProvider"></param>
        /// <returns>IPStatus, if remote host has accepted connection, otherwise unknown. None IP address if host could not be resolved. True if port is being oepened.</returns>
        private async Task<Tuple<IPStatus, IPAddress, bool>> TryConnectAsync(ServerServicesProvider servicesProvider)
        {
            var ipAddress = servicesProvider.DnsResolver.GetIpAddress(_configuration.Hostname);
            if (Equals(ipAddress, IPAddress.None))
                return new Tuple<IPStatus, IPAddress, bool>(IPStatus.Unknown, IPAddress.None, false);

            var portOpened = false;
            var ipStatus = await servicesProvider.Pinger.PingAsync(ipAddress, _configuration.Timeout);
            if (_configuration.Port > 0)
            {
                await servicesProvider.TcpClient.ConnectAsync(ipAddress, _configuration.Port, _configuration.Timeout);
                portOpened = servicesProvider.TcpClient.IsConnected;
            }

            return new Tuple<IPStatus, IPAddress, bool>(ipStatus, ipAddress, portOpened);
        }

        /// <summary>
        /// Creates PortServicesProvider instance.
        /// </summary>
        /// <returns>PortServicesProvider instance.</returns>
        private ServerServicesProvider GetServicesProviders()
        {
            var client = _configuration.TcpClientProvider();
            var dnsResolver = _configuration.DnsResolverProvider();
            var pinger = _configuration.PingerProvider();

            return new ServerServicesProvider(client, dnsResolver, pinger);
        }

        /// <summary>
        /// Class that holds useful providers to check the connection.
        /// </summary>
        private class ServerServicesProvider : IDisposable
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

            public ServerServicesProvider(ITcpClient tcpClient, IDnsResolver dnsResolver, IPinger pinger)
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
        /// Factory method for creating a new instance of ServerWatcher.
        /// </summary>
        /// <param name="hostname">Hostname to connect to.</param>
        /// <param name="port">Optional port number of the hostname (0 means not specified).</param>
        /// <param name="configurator">A configuration bulider that should be used by watcher.</param>
        /// <returns>A ServerWatcher instance.</returns>
        public static ServerWatcher Create(string hostname, int port = 0, Action<ServerWatcherConfiguration.Default> configurator = null)
        {
            var config = new ServerWatcherConfiguration.Builder(hostname, port);
            configurator?.Invoke((ServerWatcherConfiguration.Default)config);

            return Create(DefaultName, config.Build());
        }

        /// <summary>
        /// Factory method for creating a new instance of ServerWatcher.
        /// </summary>
        /// <param name="name">Name of the ServerWatcher.</param>
        /// <param name="hostname">Hostname to connect to.</param>
        /// <param name="port">Optional port number of the hostname (0 means not specified).</param>
        /// <param name="configurator">A configuration that should be used by watcher.</param>
        /// <returns>A ServerWatcher instance.</returns>
        public static ServerWatcher Create(string name, string hostname, int port = 0, Action<ServerWatcherConfiguration.Default> configurator = null)
        {
            var config = new ServerWatcherConfiguration.Builder(hostname, port);
            configurator?.Invoke((ServerWatcherConfiguration.Default)config);

            return Create(name, config.Build());
        }

        /// <summary>
        /// Factory method for creating a new instance of ServerWatcher.
        /// </summary>
        /// <param name="name">Name of the ServerWatcher.</param>
        /// <param name="configuration">Configuration of ServerWatcher.</param>
        /// <returns>A ServerWatcher instance.</returns>
        public static ServerWatcher Create(string name, ServerWatcherConfiguration configuration)
            => new ServerWatcher(name, configuration);

        /// <summary>
        /// Factory method for creating a new instance of ServerWatcher with default name of Server watcher.
        /// </summary>
        /// <param name="configuration">Configuration of ServerWatcher.</param>
        /// <returns>A ServerWatcher instance.</returns>
        public static ServerWatcher Create(ServerWatcherConfiguration configuration)
            => new ServerWatcher(DefaultName, configuration);
    }
}
