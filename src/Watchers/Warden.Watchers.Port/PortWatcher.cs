using System;
using System.Net;
using System.Threading.Tasks;

namespace Warden.Watchers.Port
{
    /// <summary>
    /// PortWatcher designed for monitoring available ports for given hostname.
    /// </summary>
    public class PortWatcher : IWatcher
    {
        private readonly PortWatcherConfiguration _configuration;

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
                return PortWatcherCheckResult.Create(this,
                    false, _configuration.Hostname, _configuration.Port,
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
            using (var servicesProvider = GetServicesProvider())
            {
                var connectionStatusAndIpAddress = await TryConnectAsync(servicesProvider);
                if (connectionStatusAndIpAddress.Item1 == null)
                {
                    return PortWatcherCheckResult.Create(this, false, _configuration.Hostname,
                        _configuration.Port, $"Could not resolve hostname '{_configuration.Hostname}' " +
                                             $"using port: {_configuration.Port}.");
                }
                if (connectionStatusAndIpAddress.Item1 == false)
                {
                    return PortWatcherCheckResult.Create(this, false, _configuration.Hostname,
                        _configuration.Port, $"Unable to connect to the hostname '{_configuration.Hostname}' " +
                                             $"using port: {_configuration.Port}.");
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

            return PortWatcherCheckResult.Create(this, isValid, _configuration.Hostname,
                _configuration.Port, $"Successfully connected to the hostname '{_configuration.Hostname}' " +
                                     $"using port: {_configuration.Port}.");
        }

        /// <summary>
        /// Checks if port is available on the remote host.
        /// </summary>
        /// <param name="servicesProvider"></param>
        /// <returns>True, if remote host has accepted connection, otherwise false. Null if host could not be resolved.</returns>
        private async Task<Tuple<bool?, IPAddress>> TryConnectAsync(PortServicesProvider servicesProvider)
        {
            var ipAddress = servicesProvider.DnsResolver.GetIpAddress(_configuration.Hostname);
            if (ipAddress == null)
                return new Tuple<bool?, IPAddress>(null, null);

            await servicesProvider.TcpClient.ConnectAsync(ipAddress, _configuration.Port, _configuration.Timeout);

            return new Tuple<bool?, IPAddress>(servicesProvider.TcpClient.IsConnected, ipAddress);
        }

        /// <summary>
        /// Creates PortServicesProvider instance.
        /// </summary>
        /// <returns>PortServicesProvider instance.</returns>
        private PortServicesProvider GetServicesProvider()
        {
            var client = _configuration.TcpClientProvider();
            var dnsResolver = _configuration.DnsResolverProvider();

            return new PortServicesProvider(client, dnsResolver);
        }

        /// <summary>
        /// Class that holds useful providers to check the connection.
        /// </summary>
        private class PortServicesProvider : IDisposable
        {
            /// <summary>
            /// ITcpClinet implementation instance.
            /// </summary>
            public ITcpClient TcpClient { get; }

            /// <summary>
            /// IDnsResolver implementation instance.
            /// </summary>
            public IDnsResolver DnsResolver { get; }

            public PortServicesProvider(ITcpClient tcpClient, IDnsResolver dnsResolver)
            {
                TcpClient = tcpClient;
                DnsResolver = dnsResolver;
            }

            public void Dispose()
            {
                TcpClient.Dispose();
                DnsResolver.Dispose();
            }
        }

        /// <summary>
        /// Factory method for creating a new instance of PortWatcher.
        /// </summary>
        /// <param name="hostname">Hostname to connect to.</param>
        /// <param name="port">Port number of the hostname to connect to.</param>
        /// <param name="configurator">A configuration bulider that should be used by watcher.</param>
        /// <returns>A PortWatcher instance.</returns>
        public static PortWatcher Create(string hostname, int port, Action<PortWatcherConfiguration.Default> configurator = null)
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
        /// <param name="port">Port number of the hostname to connect to.</param>
        /// <param name="configurator">A configuration that should be used by watcher.</param>
        /// <returns>A PortWatcher instance.</returns>
        public static PortWatcher Create(string name, string hostname, int port, Action<PortWatcherConfiguration.Default> configurator = null)
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
