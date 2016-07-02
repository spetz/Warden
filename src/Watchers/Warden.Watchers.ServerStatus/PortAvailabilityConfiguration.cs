namespace Warden.Watchers.ServerStatus
{
    using System;

    /// <summary>
    /// Configuraiton of port availability watcher.
    /// </summary>
    public class PortAvailabilityConfiguration
    {
        private PortAvailabilityConfiguration(string host)
        {
            this.Host = host;
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
        /// Factory of ITcpClient instance.
        /// </summary>
        public Func<ITcpClient> TcpClientProvider { get; private set; } = () => new DefaultTcpClient();

        /// <summary>
        /// Gets the provider of dns resolver.
        /// </summary>
        public Func<IDnsResolver> DnsResolverProvider { get; private set; } = () => new DefaultDnsResolver();

        /// <summary>
        /// Factory method for creating a new instance of fluent builder for the PortAvailabilityConfiguration.
        /// Uses the default 80 port and 30 seconds timeout.
        /// </summary>
        /// <param name="host">A host name.</param>
        public static Builder Create(string host) => new Builder(host);

        /// <summary>
        /// Fluent builder for the PortAvailabilityConfiguration.
        /// </summary>
        public abstract class Configurator<T> : WatcherConfigurator<T, PortAvailabilityConfiguration>
            where T : Configurator<T>
        {
            protected Configurator(string host)
            {
                Configuration = new PortAvailabilityConfiguration(host);
            }

            protected Configurator(PortAvailabilityConfiguration configuration) : base(configuration)
            {
            }

            /// <summary>
            /// Sets the port number that will be checked
            /// </summary>
            /// <param name="portNumber">A port number</param>
            /// <returns>Instance of fluent builder for the PortAvailabilityConfiguration.</returns>
            public T ForPort(int portNumber)
            {
                if (portNumber < 1) throw new ArgumentOutOfRangeException(nameof(portNumber));

                this.Configuration.Port = portNumber;
                return this.Configurator;
            }

            /// <summary>
            /// Sets the factory fo ITcpClient implementation.
            /// </summary>
            /// <param name="factory">Delegate that creates instance of ITcpClient implementation.</param>
            /// <returns>Instance of fluent builder for the PortAvailabilityConfiguration.</returns>
            public T WithTcpClientProvider(Func<ITcpClient> factory)
            {
                if (factory == null) throw new ArgumentNullException(nameof(factory));

                this.Configuration.TcpClientProvider = factory;
                return this.Configurator;
            }

            /// <summary>
            /// Sets the dns resolver provider dactory.
            /// </summary>
            /// <param name="factory">A factory instance.</param>
            /// <returns>Instance of fluent builder for the PortAvailabilityConfiguration.</returns>
            public T WithDnsResolver(Func<IDnsResolver> factory)
            {
                if (factory == null) throw new ArgumentNullException(nameof(factory));

                this.Configuration.DnsResolverProvider = factory;
                return this.Configurator;
            }

            /// <summary>
            /// Sets the timeout for connetion trial.
            /// </summary>
            /// <param name="timeout">An maximum time for connectio ntrial.</param>
            /// <returns>Instance of fluent builder for the PortAvailabilityConfiguration.</returns>
            public T Timeout(TimeSpan timeout)
            {
                this.Configuration.Timeout = timeout;
                return this.Configurator;
            }       
        }

        /// <summary>
        /// Default PortAvailabilityConfiguration fluent builder used while configuring watcher via lambda expression.
        /// </summary>
        public class Default : Configurator<Default>
        {
            public Default(PortAvailabilityConfiguration configuration) : base(configuration)
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
            public PortAvailabilityConfiguration Build() => Configuration;

            /// <summary>
            /// Operator overload to provide casting the Builder configurator into Default configurator.
            /// </summary>
            /// <param name="builder">Instance of extended Builder configurator.</param>
            /// <returns>Instance of Default builder configurator.</returns>
            public static explicit operator Default(Builder builder) => new Default(builder.Configuration);
        }
    }
}
