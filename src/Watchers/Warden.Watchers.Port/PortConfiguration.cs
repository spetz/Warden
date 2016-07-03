namespace Warden.Watchers.Port
{
    using System;

    /// <summary>
    /// Configuration of port watcher.
    /// </summary>
    public class PortConfiguration
    {
        private PortConfiguration(string host)
        {
            Host = host;
        }

        /// <summary>
        /// The desination host name or IP address.
        /// </summary>
        public string Host { get; }

        /// <summary>
        /// Port number for watching.
        /// </summary>
        public int Port { get; private set; } = 80;

        /// <summary>
        /// Timeout for connection.
        /// </summary>
        public TimeSpan? Timeout { get;  private set; }

        /// <summary>
        /// Gets a factory of ITcpClient provider.
        /// </summary>
        public Func<ITcpClient> TcpClientProvider { get; private set; } = () => new TcpClient();

        /// <summary>
        /// Gets the provider of dns resolver.
        /// </summary>
        public Func<IDnsResolver> DnsResolverProvider { get; private set; } = () => new DnsResolver();

        /// <summary>
        /// Factory method for creating a new instance of fluent builder for the PortConfiguration.
        /// Uses the default 80 port and 30 seconds timeout.
        /// </summary>
        /// <param name="host">A host name.</param>
        public static Builder Create(string host) => new Builder(host);

        /// <summary>
        /// Fluent builder for the PortConfiguration.
        /// </summary>
        public abstract class Configurator<T> : WatcherConfigurator<T, PortConfiguration>
            where T : Configurator<T>
        {
            protected Configurator(string host)
            {
                this.ValidateHostname(host);

                Configuration = new PortConfiguration(host);
            }

            protected Configurator(PortConfiguration configuration) : base(configuration)
            {
            }

            /// <summary>
            /// Sets the port number that will be checked
            /// </summary>
            /// <param name="portNumber">A port number</param>
            /// <returns>Instance of fluent builder for the PortConfiguration.</returns>
            public T ForPort(int portNumber)
            {
                if (portNumber < 1) throw new ArgumentOutOfRangeException(nameof(portNumber));

                Configuration.Port = portNumber;
                return Configurator;
            }

            /// <summary>
            /// Sets the factory fo ITcpClient implementation.
            /// </summary>
            /// <param name="factory">Delegate that creates instance of ITcpClient implementation.</param>
            /// <returns>Instance of fluent builder for the PortConfiguration.</returns>
            public T WithTcpClientProvider(Func<ITcpClient> factory)
            {
                if (factory == null) throw new ArgumentNullException(nameof(factory));

                Configuration.TcpClientProvider = factory;
                return Configurator;
            }

            /// <summary>
            /// Sets the dns resolver provider factory.
            /// </summary>
            /// <param name="factory">A factory instance.</param>
            /// <returns>Instance of fluent builder for the PortConfiguration.</returns>
            public T WithDnsResolver(Func<IDnsResolver> factory)
            {
                if (factory == null) throw new ArgumentNullException(nameof(factory));

                Configuration.DnsResolverProvider = factory;
                return Configurator;
            }

            /// <summary>
            /// Sets the timeout for connection trial.
            /// </summary>
            /// <param name="timeout">An maximum time for connection trial.</param>
            /// <returns>Instance of fluent builder for the PortConfiguration.</returns>
            public T Timeout(TimeSpan timeout)
            {
                Configuration.Timeout = timeout;
                return Configurator;
            }

            private void ValidateHostname(string host)
            {
                if (host == null) throw new ArgumentNullException(nameof(host));
                if (host.Contains("://"))
                    throw new ArgumentException(
                        $"The host name should not contain protocol. Did you mean \" {host.GetHostname()}\"",
                        nameof(host));
            }    
        }

        /// <summary>
        /// Default PortConfiguration fluent builder used while configuring watcher via lambda expression.
        /// </summary>
        public class Default : Configurator<Default>
        {
            public Default(PortConfiguration configuration) : base(configuration)
            {
                SetInstance(this);
            }
        }


        /// <summary>
        /// Extended PortAvailbilityConfiugration fluent builder used while configuring watcher directly.
        /// </summary>
        public class Builder : Configurator<Builder>
        {
            public Builder(string host) : base(host)
            {
                SetInstance(this);
            }

            /// <summary>
            /// Builds the PortAvailbilityConfiugration and return its instance.
            /// </summary>
            /// <returns>Instance of PortAvailbilityConfiugration.</returns>
            public PortConfiguration Build() => Configuration;

            /// <summary>
            /// Operator overload to provide casting the Builder configurator into Default configurator.
            /// </summary>
            /// <param name="builder">Instance of extended Builder configurator.</param>
            /// <returns>Instance of Default builder configurator.</returns>
            public static explicit operator Default(Builder builder) => new Default(builder.Configuration);
        }
    }
}
