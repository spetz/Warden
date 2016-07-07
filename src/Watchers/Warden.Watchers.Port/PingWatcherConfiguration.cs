using System;
using System.Net;
using System.Threading.Tasks;

namespace Warden.Watchers.Port
{
    /// <summary>
    /// Configuration of the PingWatcher.
    /// </summary>
    public class PingWatcherConfiguration
    {
        /// <summary>
        /// The destination hostname or IP address.
        /// </summary>
        public string Hostname { get; }


        /// <summary>
        /// Optional timeout for connection.
        /// </summary>
        public TimeSpan? Timeout { get;  protected set; }

        /// <summary>
        /// Predicate that has to be satisfied in order to return the valid result.
        /// </summary>
        public Func<IPAddress, bool> EnsureThat { get; protected set; }

        /// <summary>
        /// Async predicate that has to be satisfied in order to return the valid result.
        /// </summary>
        public Func<IPAddress, Task<bool>> EnsureThatAsync { get; protected set; }

        /// <summary>
        /// Custom provider for the IPingProvider.
        /// </summary>
        public Func<IPingProvider> PingProvider { get; protected set; }

        /// <summary>
        /// Custom provider for the IDnsResolver.
        /// </summary>
        public Func<IDnsResolver> DnsResolverProvider { get; protected set; }

        /// <summary>
        /// Factory method for creating a new instance of fluent builder for the PingWatcherConfiguration.
        /// Uses the default 30 seconds timeout.
        /// </summary>
        /// <param name="hostname">Hostname to be resolved.</param>
        public static Builder Create(string hostname) => new Builder(hostname);

        protected internal PingWatcherConfiguration(string hostname)
        {
            hostname.ValidateHostname();
            
            Hostname = hostname;
            DnsResolverProvider = () => new DnsResolver();
            PingProvider = () => new PingProvider();
        }

        /// <summary>
        /// Fluent builder for the PingWatcherConfiguration.
        /// </summary>
        public abstract class Configurator<T> : WatcherConfigurator<T, PingWatcherConfiguration>
            where T : Configurator<T>
        {
            protected Configurator(string hostname)
            {
                Configuration = new PingWatcherConfiguration(hostname);
            }

            protected Configurator(PingWatcherConfiguration configuration) : base(configuration)
            {
            }

            /// <summary>
            /// Timeout of the connection.
            /// </summary>
            /// <param name="timeout">Timeout.</param>
            /// <returns>Instance of fluent builder for the PingWatcherConfiguration.</returns>
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
            /// <returns>Instance of fluent builder for the PingWatcherConfiguration.</returns>
            public T EnsureThat(Func<IPAddress, bool> ensureThat)
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
            /// <returns>Instance of fluent builder for the PingWatcherConfiguration.</returns>
            public T EnsureThatAsync(Func<IPAddress, Task<bool>> ensureThat)
            {
                if (ensureThat == null)
                    throw new ArgumentException("Ensure that async predicate can not be null.", nameof(ensureThat));

                Configuration.EnsureThatAsync = ensureThat;

                return Configurator;
            }

            /// <summary>
            /// Sets the custom provider for the ITcpClient.
            /// </summary>
            /// <param name="pingProvider">Custom provider for the IPingProvider.</param>
            /// <returns>Instance of fluent builder for the PingWatcherConfiguration.</returns>
            public T WithPingProvider(Func<IPingProvider> pingProvider)
            {
                if (pingProvider == null)
                {
                    throw new ArgumentNullException(nameof(pingProvider),
                        "Ping provider can not be null.");
                }

                Configuration.PingProvider = pingProvider;
                return Configurator;
            }

            /// <summary>
            /// Sets the custom provider for the IDnsResolver.
            /// </summary>
            /// <param name="dsnResolverProvider">Custom provider for the IDnsResolver.</param>
            /// <returns>Instance of fluent builder for the PingWatcherConfiguration.</returns>
            public T WithDnsResolver(Func<IDnsResolver> dsnResolverProvider)
            {
                if (dsnResolverProvider == null)
                {
                    throw new ArgumentNullException(nameof(dsnResolverProvider),
                        "DNS Resolver provider can not be null.");
                }

                Configuration.DnsResolverProvider = dsnResolverProvider;

                return Configurator;
            }
        }

        /// <summary>
        /// Default PingWatcherConfiguration fluent builder used while configuring watcher via lambda expression.
        /// </summary>
        public class Default : Configurator<Default>
        {
            public Default(PingWatcherConfiguration configuration) : base(configuration)
            {
                SetInstance(this);
            }
        }

        /// <summary>
        /// Extended PingConfiugration fluent builder used while configuring watcher directly.
        /// </summary>
        public class Builder : Configurator<Builder>
        {
            public Builder(string hostname) : base(hostname)
            {
                SetInstance(this);
            }

            /// <summary>
            /// Builds the PingConfiugration and return its instance.
            /// </summary>
            /// <returns>Instance of PingConfiugration.</returns>
            public PingWatcherConfiguration Build() => Configuration;

            /// <summary>
            /// Operator overload to provide casting the Builder configurator into Default configurator.
            /// </summary>
            /// <param name="builder">Instance of extended Builder configurator.</param>
            /// <returns>Instance of Default builder configurator.</returns>
            public static explicit operator Default(Builder builder) => new Default(builder.Configuration);
        }
    }
}
