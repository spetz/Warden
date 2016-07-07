namespace Warden.Watchers.Port
{
    using System;
    using System.Net;
    using System.Threading.Tasks;

    using System.Collections.Generic;
    using System.Net.NetworkInformation;

    /// <summary>
    /// PingWatcher designed for monitoring specified host with ICMP echo request..
    /// </summary>
    public class PingWatcher : IWatcher
    {
        private readonly PingWatcherConfiguration _configuration;

        private readonly Dictionary<IPStatus, string> _ipStatusMessages = new Dictionary<IPStatus, string>
        {
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
            {IPStatus.SourceQuench, "ICMP echo request failed because host '{0}' discared the packet." },
            {IPStatus.NoResources, "ICMP echo request to host '{0}' failed because of insufficient sources." },
            {IPStatus.Unknown, "ICMP echo request to host '{0}' failed because of uknown error." },
            {IPStatus.PacketTooBig, "ICMP echo request to host '{0}' failed because the packet is larger than MTU of node (router of gateway)" },
            {IPStatus.ParameterProblem, "ICMP echo request to host '{0}' failed because the node (router or gateway) failed while processing packet header." },
        };

        /// <summary>
        /// Default name of the PingWatcher.
        /// </summary>
        public const string DefaultName = "Ping Watcher";

        /// <summary>
        /// Name of the PingWatcher.
        /// </summary>
        public string Name { get; }

        protected PingWatcher(string name, PingWatcherConfiguration configuration)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Watcher name can not be empty.");

            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration),
                    "Ping Watcher configuration has not been provided.");
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
            catch (TaskCanceledException)
            {
                return PingWatcherCheckResult.Create(this,
                    false, _configuration.Hostname,
                    "A connection timeout has occurred while trying to access the hostname: " +
                    $"'{_configuration.Hostname}'.");
            }
            catch (Exception exception)
            {
                throw new WatcherException("There was an error while trying to access the hostname: " +
                                           $"'{_configuration.Hostname}'.", exception);
            }
        }

        private async Task<IWatcherCheckResult> ExecuteCheckAsync()
        {
            using (var servicesProvider = GetServicesProvider())
            {
                var connectionStatusAndIpAddress = await TryConnectAsync(servicesProvider);
                if (connectionStatusAndIpAddress.Item1 == null)
                {
                    return PingWatcherCheckResult.Create(this, false, _configuration.Hostname,
                            $"Could not resolve hostname '{_configuration.Hostname}'.");
                }

                if (connectionStatusAndIpAddress.Item1.Value != IPStatus.Success)
                {
                    var ipStatus = connectionStatusAndIpAddress.Item1.Value;
                    if (!_ipStatusMessages.ContainsKey(connectionStatusAndIpAddress.Item1.Value))
                    {
                        ipStatus = IPStatus.Unknown;
                    }

                    return PingWatcherCheckResult.Create(this, false, _configuration.Hostname,
                        string.Format(_ipStatusMessages[ipStatus], this._configuration.Hostname));
                }

                return await EnsureAsync(connectionStatusAndIpAddress.Item2);
            }
        }

        private async Task<IWatcherCheckResult> EnsureAsync(IPAddress ipAddress)
        {
            var isValid = true;
            if (_configuration.EnsureThatAsync != null)
                isValid = await _configuration.EnsureThatAsync?.Invoke(ipAddress);

            isValid = isValid && (_configuration.EnsureThat?.Invoke(ipAddress) ?? true);

            return PingWatcherCheckResult.Create(this, isValid, _configuration.Hostname,
                $"Successfully connected to the hostname '{_configuration.Hostname}'");
        }

        /// <summary>
        /// Checks if Ping is available on the remote host.
        /// </summary>
        /// <param name="servicesProvider"></param>
        /// <returns>True, if remote host has accepted connection, otherwise false. Null if host could not be resolved.</returns>
        private async Task<Tuple<IPStatus?, IPAddress>> TryConnectAsync(PingServicesProvider servicesProvider)
        {
            var ipAddress = servicesProvider.DnsResolver.GetIpAddress(_configuration.Hostname);
            if (ipAddress == null)
                return new Tuple<IPStatus?, IPAddress>(null, null);

            var result = await servicesProvider.Ping.PingAsync(ipAddress, _configuration.Timeout);

            return new Tuple<IPStatus?, IPAddress>(result, ipAddress);
        }

        /// <summary>
        /// Creates PingServicesProvider instance.
        /// </summary>
        /// <returns>PingServicesProvider instance.</returns>
        private PingServicesProvider GetServicesProvider()
        {
            var ping = _configuration.PingProvider();
            var dnsResolver = _configuration.DnsResolverProvider();

            return new PingServicesProvider(ping, dnsResolver);
        }

        /// <summary>
        /// Class that holds useful providers to check the connection.
        /// </summary>
        private class PingServicesProvider : IDisposable
        {
            /// <summary>
            /// ITcpClinet implementation instance.
            /// </summary>
            public IPingProvider Ping { get; }

            /// <summary>
            /// IDnsResolver implementation instance.
            /// </summary>
            public IDnsResolver DnsResolver { get; }

            public PingServicesProvider(IPingProvider ping, IDnsResolver dnsResolver)
            {
                Ping = ping;
                DnsResolver = dnsResolver;
            }

            public void Dispose()
            {
                Ping.Dispose();
                DnsResolver.Dispose();
            }
        }

        /// <summary>
        /// Factory method for creating a new instance of PingWatcher.
        /// </summary>
        /// <param name="hostname">Hostname to connect to.</param>
        /// <param name="configurator">A configuration bulider that should be used by watcher.</param>
        /// <returns>A PingWatcher instance.</returns>
        public static PingWatcher Create(string hostname, Action<PingWatcherConfiguration.Default> configurator = null)
        {
            var config = new PingWatcherConfiguration.Builder(hostname);
            configurator?.Invoke((PingWatcherConfiguration.Default)config);

            return Create(DefaultName, config.Build());
        }

        /// <summary>
        /// Factory method for creating a new instance of PingWatcher.
        /// </summary>
        /// <param name="name">Name of the PingWatcher.</param>
        /// <param name="hostname">Hostname to connect to.</param>
        /// <param name="configurator">A configuration that should be used by watcher.</param>
        /// <returns>A PingWatcher instance.</returns>
        public static PingWatcher Create(string name, string hostname, Action<PingWatcherConfiguration.Default> configurator = null)
        {
            var config = new PingWatcherConfiguration.Builder(hostname);
            configurator?.Invoke((PingWatcherConfiguration.Default)config);

            return Create(name, config.Build());
        }

        /// <summary>
        /// Factory method for creating a new instance of PingWatcher.
        /// </summary>
        /// <param name="name">Name of the PingWatcher.</param>
        /// <param name="configuration">Configuration of PingWatcher.</param>
        /// <returns>A PingWatcher instance.</returns>
        public static PingWatcher Create(string name, PingWatcherConfiguration configuration)
            => new PingWatcher(name, configuration);

        /// <summary>
        /// Factory method for creating a new instance of PingWatcher with default name of Ping Watcher.
        /// </summary>
        /// <param name="configuration">Configuration of PingWatcher.</param>
        /// <returns>A PingWatcher instance.</returns>
        public static PingWatcher Create(PingWatcherConfiguration configuration)
            => new PingWatcher(DefaultName, configuration);
    }
}
