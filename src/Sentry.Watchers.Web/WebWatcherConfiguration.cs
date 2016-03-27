using System;
using System.Net.Http;
using System.Threading.Tasks;
using Sentry.Core;

namespace Sentry.Watchers.Web
{
    /// <summary>
    /// Configuration of the hooks for the WebWatcher.
    /// </summary>
    public class WebWatcherConfiguration
    {
        /// <summary>
        /// Base URL of the request.
        /// </summary>
        public Uri Uri { get; protected set; }

        /// <summary>
        /// Instance of IHttpRequest.
        /// </summary>
        public IHttpRequest Request { get; protected set; }

        /// <summary>
        /// Custom provider for the IHttpService.
        /// </summary>
        public Func<IHttpService> HttpServiceProvider { get; protected set; }

        /// <summary>
        /// Flag determining whether the invalid status code should be treated as valid one.
        /// </summary>
        public bool SkipStatusCodeValidation { get; protected set; }

        /// <summary>
        /// Optional timeout of the IHttpRequest.
        /// </summary>
        public TimeSpan? Timeout { get; protected set; }

        /// <summary>
        /// Predicate that has to be satisfied in order to return the valid result.
        /// </summary>
        public Func<IHttpResponse, bool> EnsureThat { get; protected set; }

        /// <summary>
        /// Async predicate that has to be satisfied in order to return the valid result.
        /// </summary>
        public Func<IHttpResponse, Task<bool>> EnsureThatAsync { get; protected set; }

        protected internal WebWatcherConfiguration(string url, IHttpRequest request)
        {
            if (string.IsNullOrEmpty(url))
                throw new ArgumentException("URL can not be empty.", nameof(url));

            if (request == null)
                throw new ArgumentNullException(nameof(request), "Request can not be null.");

            Uri = new Uri(url);
            Request = request;
            HttpServiceProvider = () => new HttpService(new HttpClient());
        }

        /// <summary>
        /// Factory method for creating a new instance of fluent builder for the WebWatcherConfiguration.
        /// Uses the default HTTP GET request.
        /// </summary>
        /// <param name="url">Base URL of the request.</param>
        public static Builder Create(string url) => new Builder(url, HttpRequest.Get());

        /// <summary>
        /// Factory method for creating a new instance of fluent builder for the WebWatcherConfiguration.
        /// </summary>
        /// <param name="url">Base URL of the request.</param>
        /// <param name="request">Instance of IHttpRequest.</param>
        /// <returns>Instance of fluent builder for the WebWatcherConfiguration.</returns>
        public static Builder Create(string url, IHttpRequest request) => new Builder(url, request);

        /// <summary>
        /// Fluent builder for the WebWatcherConfiguration.
        /// </summary>
        public abstract class Configurator<T> : WatcherConfigurator<T, WebWatcherConfiguration>
            where T : Configurator<T>
        {
            protected Configurator(string url, IHttpRequest request)
            {
                Configuration = new WebWatcherConfiguration(url, request);
            }

            protected Configurator(WebWatcherConfiguration configuration) : base(configuration)
            {
            }

            /// <summary>
            /// Sets the HTTP request.
            /// </summary>
            /// <param name="request">Instance of IHttpRequest.</param>
            /// <returns>Instance of fluent builder for the WebWatcherConfiguration.</returns>
            public T WithRequest(IHttpRequest request)
            {
                if (request == null)
                    throw new ArgumentNullException(nameof(request), "HTTP request can not be null.");

                Configuration.Request = request;

                return Configurator;
            }


            /// <summary>
            /// Timeout of the HTTP request.
            /// </summary>
            /// <param name="timeout">Timeout.</param>
            /// <returns>Instance of fluent builder for the WebWatcherConfiguration.</returns>
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
            /// Skips the validation of the status code.
            /// </summary>
            /// <returns>Instance of fluent builder for the WebWatcherConfiguration.</returns>
            public T SkipStatusCodeValidation()
            {
                Configuration.SkipStatusCodeValidation = true;

                return Configurator;
            }

            /// <summary>
            /// Sets the predicate that has to be satisfied in order to return the valid result.
            /// </summary>
            /// <param name="ensureThat">Lambda expression predicate.</param>
            /// <returns>Instance of fluent builder for the WebWatcherConfiguration.</returns>
            public T EnsureThat(Func<IHttpResponse, bool> ensureThat)
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
            /// <returns>Instance of fluent builder for the WebWatcherConfiguration.</returns>
            public T EnsureThatAsync(Func<IHttpResponse, Task<bool>> ensureThat)
            {
                if (ensureThat == null)
                    throw new ArgumentException("Ensure that async predicate can not be null.", nameof(ensureThat));

                Configuration.EnsureThatAsync = ensureThat;

                return Configurator;
            }

            /// <summary>
            /// Sets the custom provider fot the IHttpService.
            /// </summary>
            /// <param name="httpServiceProvider">Custom provider fot the </param>
            /// <returns>Lambda expression returning an instance of the IHttpService.</returns>
            /// <returns>Instance of fluent builder for the WebWatcherConfiguration.</returns>
            public T WithHttpServiceProvider(Func<IHttpService> httpServiceProvider)
            {
                if (httpServiceProvider == null)
                    throw new ArgumentNullException(nameof(httpServiceProvider),
                        "HTTP service provider can not be null.");

                Configuration.HttpServiceProvider = httpServiceProvider;

                return Configurator;
            }
        }

        /// <summary>
        /// Default WebWatcherConfiguration fluent builder used while configuring watcher via lambda expression.
        /// </summary>
        public class Default : Configurator<Default>
        {
            public Default(WebWatcherConfiguration configuration) : base(configuration)
            {
                SetInstance(this);
            }
        }

        /// <summary>
        /// Extended WebWatcherConfiguration fluent builder used while configuring watcher directly.
        /// </summary>
        public class Builder : Configurator<Builder>
        {
            public Builder(string url, IHttpRequest request) : base(url, request)
            {
                SetInstance(this);
            }

            /// <summary>
            /// Builds the WebWatcherConfiguration and return its instance.
            /// </summary>
            /// <returns>Instance of WebWatcherConfiguration.</returns>
            public WebWatcherConfiguration Build() => Configuration;

            /// <summary>
            /// Operator overload to provide casting the Builder configurator into Default configurator.
            /// </summary>
            /// <param name="builder">Instance of extended Builder configurator.</param>
            /// <returns>Instance of Default builder configurator.</returns>
            public static explicit operator Default(Builder builder) => new Default(builder.Configuration);
        }
    }
}