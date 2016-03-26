using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Sentry.Core;

namespace Sentry.Watchers.Web
{
    public class WebWatcherConfiguration
    {
        public Uri Uri { get; protected set; }
        public IHttpRequest Request { get; protected set; }
        public Func<IHttpService> HttpServiceProvider { get; protected set; }
        public bool SkipStatusCodeValidation { get; protected set; }
        public TimeSpan? Timeout { get; protected set; }
        public Func<IHttpResponse, bool> EnsureThat { get; protected set; }
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

        public static Builder Create(string url) => new Builder(url, HttpRequest.Get());

        public static Builder Create(string url, IHttpRequest request) => new Builder(url, request);

        public abstract class Configurator<T> : WatcherConfigurator<T, WebWatcherConfiguration> where T : Configurator<T>
        {
            protected Configurator(string url, IHttpRequest request)
            {
                Configuration = new WebWatcherConfiguration(url, request);
            }

            protected Configurator(WebWatcherConfiguration configuration) : base(configuration)
            {
            }

            public T WithRequest(HttpRequest request)
            {
                if (request == null)
                    throw new ArgumentNullException(nameof(request), "HTTP request can not be null.");

                Configuration.Request = request;

                return Configurator;
            }

            public T WithTimeout(TimeSpan timeout)
            {
                if (timeout == null)
                    throw new ArgumentNullException(nameof(timeout), "Timeout can not be null.");

                if(timeout == TimeSpan.Zero)
                    throw new ArgumentException("Timeout can not be equal to zero.", nameof(timeout));

                Configuration.Timeout = timeout;

                return Configurator;
            }

            public T WithHttpServiceProvider(Func<IHttpService> httpClientProvider)
            {
                if (httpClientProvider == null)
                    throw new ArgumentNullException(nameof(httpClientProvider), "HTTP client provider can not be null.");

                Configuration.HttpServiceProvider = httpClientProvider;

                return Configurator;
            }

            public T SkipStatusCodeValidation()
            {
                Configuration.SkipStatusCodeValidation = true;

                return Configurator;
            }

            public T EnsureThat(Func<IHttpResponse, bool> ensureThat)
            {
                if (ensureThat == null)
                    throw new ArgumentException("Ensure that predicate can not be null.", nameof(ensureThat));

                Configuration.EnsureThat = ensureThat;

                return Configurator;
            }

            public T EnsureThatAsync(Func<IHttpResponse, Task<bool>> ensureThat)
            {
                if (ensureThat == null)
                    throw new ArgumentException("Ensure that async predicate can not be null.", nameof(ensureThat));

                Configuration.EnsureThatAsync = ensureThat;

                return Configurator;
            }
        }

        public class Default : Configurator<Default>
        {
            public Default(WebWatcherConfiguration configuration) : base(configuration)
            {
                SetInstance(this);
            }
        }

        public class Builder : Configurator<Builder>
        {
            public Builder(string url, IHttpRequest request) : base(url, request)
            {
                SetInstance(this);
            }

            public WebWatcherConfiguration Build() => Configuration;

            public static explicit operator Default(Builder builder) => new Default(builder.Configuration);
        }
    }
}