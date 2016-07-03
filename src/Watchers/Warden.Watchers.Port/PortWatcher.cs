namespace Warden.Watchers.Port
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading.Tasks;
    using global::Warden.Core;

    public class PortWatcher : IWatcher
    {
        /// <summary>
        /// Default name for this watcher.
        /// </summary>
        public const string DefaultName = "Port Watcher";

        /// <summary>
        /// Current name of this watcher.
        /// </summary>
        public string Name { get; }
        
        /// <summary>
        /// Actual configuration of current instance of this watcher.
        /// </summary>
        public PortConfiguration Configuration { get; }

        /// <summary>
        /// Ctor of PortWatcher.
        /// </summary>
        /// <param name="name">Name of this watcher.</param>
        /// <param name="configuration">Configuration to use.</param>
        protected PortWatcher(string name, PortConfiguration configuration)
        {
            this.Name = name;
            this.Configuration = configuration;
        }

        /// <summary>
        /// Performs a single check for the watcher.
        /// </summary>
        /// <returns>Instance of IWatcherCheckResult.</returns>
        public async Task<IWatcherCheckResult> ExecuteAsync()
        {           
            bool? success;

            try
            {
                using (var servicesProvider = this.GetServicesProvider())
                {
                    success = await this.TryConnectionAsync(servicesProvider);
                }
            }
            catch (Exception e)
            {
                throw new WardenException("Unhandled exception occured.", e);
            }

            if (success == null)
            {
                return PortCheckResult.Create(this, false, this.Configuration.Host,
                    this.Configuration.Port, "Could not resolve host.");
            }

            if (!success.Value)
            {
                return PortCheckResult.Create(this, false, this.Configuration.Host,
                    this.Configuration.Port, "Unable to connect to the host.");
            }

            return PortCheckResult.Create(this, true, this.Configuration.Host,
                this.Configuration.Port, "Connected to the host successfully.");
        }

        /// <summary>
        /// Creates a Port availability watcher instance.
        /// </summary>
        /// <param name="name">A name of watcher.</param>
        /// <param name="host">A host name to connect to.</param>
        /// <param name="configurator">A configuration that should be used by watcher.</param>
        /// <returns>A PortWatcher instance.</returns>
        public static PortWatcher Create(string name, string host, Action<PortConfiguration.Default> configurator = null)
        {
            var config = new PortConfiguration.Builder(host);
            configurator?.Invoke((PortConfiguration.Default)config);

            return Create(name, config.Build());
        }

        /// <summary>
        /// Creates a Port availability watcher instance with default name.
        /// </summary>
        /// <param name="host">A host name to connect to.</param>
        /// <param name="configurator">A configuration bulider that should be used by watcher.</param>
        /// <returns>A PortWatcher instance.</returns>
        public static PortWatcher Create(string host, Action<PortConfiguration.Default> configurator = null)
        {
            var config = new PortConfiguration.Builder(host);
            configurator?.Invoke((PortConfiguration.Default)config);

            return Create(DefaultName, config.Build());
        }

        /// <summary>
        /// Creates a Port availability watcher instance with default name.
        /// </summary>
        /// <param name="name">A name of watcher.</param>
        /// <param name="configuration">A configuration that should be used by watcher.</param>
        /// <returns>A PortWatcher instance.</returns>
        public static PortWatcher Create(string name, PortConfiguration configuration)
            => new PortWatcher(name, configuration);

        /// <summary>
        /// Checks if port on remote host is available.
        /// </summary>
        /// <param name="servicesProvider"></param>
        /// <returns>True, if remote host acceppted connection. False otherwise. Null, if could not resolve host.</returns>
        private async Task<bool?> TryConnectionAsync(PortServicesProvider servicesProvider)
        {
            var hostInfo = servicesProvider.DnsResolver.GetIp(this.Configuration.Host);
            if (hostInfo == null)
            {
                return null;
            }

            await servicesProvider.TcpClient.ConnectAsync(hostInfo, this.Configuration.Port, this.Configuration.Timeout);
            return servicesProvider.TcpClient.IsConnected;
        }

        /// <summary>
        /// Creates PortServicesProvider instance.
        /// </summary>
        /// <returns>PortServicesProvider instance.</returns>
        private PortServicesProvider GetServicesProvider()
        {
            var client = Configuration.TcpClientProvider();
            if (client == null) throw new WardenException("Tcp client is not set.");

            var dnsResolver = Configuration.DnsResolverProvider();
            if (dnsResolver == null) throw new WardenException("Dns resolver is null.");

            return new PortServicesProvider(client, dnsResolver);
        }



        /// <summary>
        /// Class that holds useful providers to connection check.
        /// </summary>
        private class PortServicesProvider : IDisposable
        {
            public PortServicesProvider(ITcpClient tcpClient, IDnsResolver dnsResolver)
            {
                TcpClient = tcpClient;
                DnsResolver = dnsResolver;
            }

            /// <summary>
            /// ITcpClinet implementation instance.
            /// </summary>
            public ITcpClient TcpClient { get; private set; }

            /// <summary>
            /// IDnsResolver implementation instance.
            /// </summary>
            public IDnsResolver DnsResolver { get; private set; }

            public void Dispose()
            {
                this.TcpClient.Dispose();
                this.DnsResolver.Dispose();
            }
        }
    }
}
