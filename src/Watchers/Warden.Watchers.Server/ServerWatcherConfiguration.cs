using System;
using System.Threading.Tasks;

namespace Warden.Watchers.Server
{
    /// <summary>
    /// Configuration of the ServerWatcher.
    /// </summary>
    public class ServerWatcherConfiguration
    {
        /// <summary>
        /// The destination hostname or IP address.
        /// </summary>
        public string Hostname { get; }

        /// <summary>
        /// Optional port number for watching (0 means not specified).
        /// </summary>
        public int Port { get; }

        /// <summary>
        /// Optional timeout for connection.
        /// </summary>
        public TimeSpan? Timeout { get;  protected set; }

        /// <summary>
        /// Predicate that has to be satisfied in order to return the valid result.
        /// </summary>
        public Func<ConnectionInfo, bool> EnsureThat { get; protected set; }

        /// <summary>
        /// Async predicate that has to be satisfied in order to return the valid result.
        /// </summary>
        public Func<ConnectionInfo, Task<bool>> EnsureThatAsync { get; protected set; }

        /// <summary>
        /// Custom provider for the ITcpClient.
        /// </summary>
        public Func<ITcpClient> TcpClientProvider { get; protected set; }

        /// <summary>
        /// Custom provider for the IDnsResolver.
        /// </summary>
        public Func<IDnsResolver> DnsResolverProvider { get; protected set; }

        /// <summary>
        /// Custom provider for the IPinger.
        /// </summary>
        public Func<IPinger> PingerProvider { get; protected set; }

        /// <summary>
        /// Factory method for creating a new instance of fluent builder for the ServerWatcherConfiguration.
        /// Uses the default 30 seconds timeout.
        /// </summary>
        /// <param name="hostname">Hostname to be resolved.</param>
        /// <param name="port">Port number of the hostname.</param>
        public static Builder Create(string hostname, int port = 0) => new Builder(hostname, port);

        protected internal ServerWatcherConfiguration(string hostname, int port)
        {
            hostname.ValidateHostname();
            if (port < 0)
                throw new ArgumentException("Port number can not be less than 0.", nameof(port));

            Hostname = hostname;
            Port = port;
            DnsResolverProvider = () => new DnsResolver();
            TcpClientProvider = () => new TcpClient();
            PingerProvider = () => new Pinger();
        }

        /// <summary>
        /// Fluent builder for the ServerWatcherConfiguration.
        /// </summary>
        public abstract class Configurator<T> : WatcherConfigurator<T, ServerWatcherConfiguration>
            where T : Configurator<T>
        {
            protected Configurator(string hostname, int port = 0)
            {
                ValidateHostnameAndPort(hostname, port);
                Configuration = new ServerWatcherConfiguration(hostname, port);
            }

            protected Configurator(ServerWatcherConfiguration configuration) : base(configuration)
            {
            }

            private void ValidateHostnameAndPort(string hostname, int port = 0)
            {
                hostname.ValidateHostname();
                if (port < 0)
                    throw new ArgumentException("Port number can not be less than 0.", nameof(port));
            }

            /// <summary>
            /// Timeout of the connection.
            /// </summary>
            /// <param name="timeout">Timeout.</param>
            /// <returns>Instance of fluent builder for the ServerWatcherConfiguration.</returns>
            public T WithTimeout(TimeSpan timeout)
            {
                if (timeout == null)
                    throw new ArgumentNullException(nameof(timeout), "Timeout can not be null.");

                if (timeout == TimeSpan.Zero)
                    throw new ArgumentException("Timeout can not be equal to zero.", nameof(timeout));

                Configuration.Timeout = timeout;

                return Configurator;
            }

            /// <summary>
            /// Sets the predicate that has to be satisfied in order to return the valid result.
            /// </summary>
            /// <param name="ensureThat">Lambda expression predicate.</param>
            /// <returns>Instance of fluent builder for the ServerWatcherConfiguration.</returns>
            public T EnsureThat(Func<ConnectionInfo, bool> ensureThat)
            {
                if (ensureThat == null)
                    throw new ArgumentException("Ensure that predicate can not be null.", nameof(ensureThat));

                Configuration.EnsureThat = ensureThat;

                return Configurator;
            }

            /// <summary>
            /// Sets the async predicate that has to be satisfied in order to return the valid result.
            /// <param name="ensureThat">Lambda expression predicate.</param>
            /// </summary>
            /// <returns>Instance of fluent builder for the ServerWatcherConfiguration.</returns>
            public T EnsureThatAsync(Func<ConnectionInfo, Task<bool>> ensureThat)
            {
                if (ensureThat == null)
                    throw new ArgumentException("Ensure that async predicate can not be null.", nameof(ensureThat));

                Configuration.EnsureThatAsync = ensureThat;

                return Configurator;
            }

            /// <summary>
            /// Sets the custom provider for the ITcpClient.
            /// </summary>
            /// <param name="tcpClientProvider">Custom provider for the ITcpClient.</param>
            /// <returns>Instance of fluent builder for the ServerWatcherConfiguration.</returns>
            public T WithTcpClientProvider(Func<ITcpClient> tcpClientProvider)
            {
                if (tcpClientProvider == null)
                {
                    throw new ArgumentNullException(nameof(tcpClientProvider),
                        "TCP Client provider can not be null.");
                }

                Configuration.TcpClientProvider = tcpClientProvider;

                return Configurator;
            }

            /// <summary>
            /// Sets the custom provider for the IDnsResolver.
            /// </summary>
            /// <param name="dsnResolverProvider">Custom provider for the IDnsResolver.</param>
            /// <returns>Instance of fluent builder for the ServerWatcherConfiguration.</returns>
            public T WithDnsResolverProvider(Func<IDnsResolver> dsnResolverProvider)
            {
                if (dsnResolverProvider == null)
                {
                    throw new ArgumentNullException(nameof(dsnResolverProvider),
                        "DNS Resolver provider can not be null.");
                }

                Configuration.DnsResolverProvider = dsnResolverProvider;

                return Configurator;
            }

            /// <summary>
            /// Sets the custom provider for the IPinger.
            /// </summary>
            /// <param name="pingerProvider">Custom provider for the IPinger.</param>
            /// <returns>Instance of fluent builder for the ServerWatcherConfiguration.</returns>
            public T WithPingerProvider(Func<IPinger> pingerProvider)
            {
                if (pingerProvider == null)
                {
                    throw new ArgumentNullException(nameof(pingerProvider),
                        "Pinger provider can not be null.");
                }

                Configuration.PingerProvider = pingerProvider;

                return Configurator;
            }
        }

        /// <summary>
        /// Default ServerWatcherConfiguration fluent builder used while configuring watcher via lambda expression.
        /// </summary>
        public class Default : Configurator<Default>
        {
            public Default(ServerWatcherConfiguration configuration) : base(configuration)
            {
                SetInstance(this);
            }
        }

        /// <summary>
        /// Extended PortConfiugration fluent builder used while configuring watcher directly.
        /// </summary>
        public class Builder : Configurator<Builder>
        {
            public Builder(string hostname, int port) : base(hostname, port)
            {
                SetInstance(this);
            }

            /// <summary>
            /// Builds the PortConfiugration and return its instance.
            /// </summary>
            /// <returns>Instance of PortConfiugration.</returns>
            public ServerWatcherConfiguration Build() => Configuration;

            /// <summary>
            /// Operator overload to provide casting the Builder configurator into Default configurator.
            /// </summary>
            /// <param name="builder">Instance of extended Builder configurator.</param>
            /// <returns>Instance of Default builder configurator.</returns>
            public static explicit operator Default(Builder builder) => new Default(builder.Configuration);
        }
    }
}
