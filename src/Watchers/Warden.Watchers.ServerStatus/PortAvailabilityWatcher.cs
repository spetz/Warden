namespace Warden.Watchers.ServerStatus
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading.Tasks;
    using global::Warden.Core;

    public class PortAvailabilityWatcher : IWatcher
    {
        /// <summary>
        /// Default name for this watcher.
        /// </summary>
        public const string DefaultName = "Port availability Watcher";

        /// <summary>
        /// Current name of this watcher.
        /// </summary>
        public string Name { get; }
        
        /// <summary>
        /// Actual configuration of current instance of this watcher.
        /// </summary>
        public PortAvailabilityConfiguration Configuration { get; }

        /// <summary>
        /// Ctor of PortAvailabilityWatcher.
        /// </summary>
        /// <param name="name">Name of this watcher.</param>
        /// <param name="configuration">Configuration to use.</param>
        protected PortAvailabilityWatcher(string name, PortAvailabilityConfiguration configuration)
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
            var client = this.Configuration.TcpClientProvider();
            if (client == null) throw new WardenException("Tcp client is not set.");

            var success = false;

            var dnsResolver = this.Configuration.DnsResolverProvider();
            if (dnsResolver == null) throw new WardenException("Dns resolver is null.");

            try
            {
                var hostInfo = dnsResolver.GetIp(this.Configuration.Host);
                if (hostInfo == null)
                {
                    return PortAvailabilityCheckResult.Create(this, false, this.Configuration.Host,
                           this.Configuration.Port, "Could not resolve host.");
                }

                await client.ConnectAsync(hostInfo, this.Configuration.Port, this.Configuration.Timeout);
                success = client.IsConnected;
            }
            catch (Exception e)
            {
                throw new WardenException("Unhandled exception occured.", e);
            }

            if (!success)
            {
                return PortAvailabilityCheckResult.Create(this, false, this.Configuration.Host,
                    this.Configuration.Port, "Unable to connect to the host.");
            }

            return PortAvailabilityCheckResult.Create(this, true, this.Configuration.Host,
                this.Configuration.Port, "Connected to the host successfully.");
        }

        /// <summary>
        /// Creates a Port availability watcher instance.
        /// </summary>
        /// <param name="name">A name of watcher.</param>
        /// <param name="host">A host name to connect to.</param>
        /// <param name="configurator">A configuration that should be used by watcher.</param>
        /// <returns>A PortAvailabilityWatcher instance.</returns>
        public static PortAvailabilityWatcher Create(string name, string host, Action<PortAvailabilityConfiguration.Default> configurator = null)
        {
            var config = new PortAvailabilityConfiguration.Builder(host);
            configurator?.Invoke((PortAvailabilityConfiguration.Default)config);

            return Create(name, config.Build());
        }

        /// <summary>
        /// Creates a Port availability watcher instance with default name.
        /// </summary>
        /// <param name="host">A host name to connect to.</param>
        /// <param name="configurator">A configuration bulider that should be used by watcher.</param>
        /// <returns>A PortAvailabilityWatcher instance.</returns>
        public static PortAvailabilityWatcher Create(string host, Action<PortAvailabilityConfiguration.Default> configurator = null)
        {
            var config = new PortAvailabilityConfiguration.Builder(host);
            configurator?.Invoke((PortAvailabilityConfiguration.Default)config);

            return Create(DefaultName, config.Build());
        }

        /// <summary>
        /// Creates a Port availability watcher instance with default name.
        /// </summary>
        /// <param name="name">A name of watcher.</param>
        /// <param name="configuration">A configuration that should be used by watcher.</param>
        /// <returns>A PortAvailabilityWatcher instance.</returns>
        public static PortAvailabilityWatcher Create(string name, PortAvailabilityConfiguration configuration)
            => new PortAvailabilityWatcher(name, configuration);
    }
}
