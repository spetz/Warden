using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Sentry.Core;

namespace Sentry.Watchers.Website
{
    public class WebsiteWatcherConfiguration
    {
        public Uri Uri { get; protected set; }
        public bool SkipStatusCodeValidation { get; protected set; }
        public Func<IHttpClient> HttpClientProvider { get; protected set; }
        public IDictionary<string, string> Headers { get; protected set; }
        public TimeSpan Timeout { get; protected set; }
        public Func<HttpResponseMessage, bool> EnsureThat { get; protected set; }
        public Func<HttpResponseMessage, Task<bool>> EnsureThatAsync { get; protected set; }

        protected internal WebsiteWatcherConfiguration(string url)
        {
            if (string.IsNullOrEmpty(url))
                throw new ArgumentException("URL can not be empty.", nameof(url));

            Uri = new Uri(url);
            Headers = new Dictionary<string, string>();
            Timeout = TimeSpan.Zero;
            HttpClientProvider = () => new HttpClientWrapper(new HttpClient());
        }

        public static Builder Create(string url) => new Builder(url);

        public abstract class Configurator<T> : WatcherConfigurator<T, WebsiteWatcherConfiguration> where T : Configurator<T>
        {
            protected Configurator(string url)
            {
                Configuration = new WebsiteWatcherConfiguration(url);
            }

            protected Configurator(WebsiteWatcherConfiguration configuration) : base(configuration)
            {
            }

            public T WithHeaders(IDictionary<string, string> headers)
            {
                if (headers == null)
                    throw new ArgumentNullException(nameof(headers), "Headers can not be null.");

                foreach (var header in headers)
                {
                    WithHeader(header);
                }

                return Configurator;
            }

            public T WithHeader(KeyValuePair<string, string> header)
            {
                Configuration.Headers.Add(header);

                return Configurator;
            }

            public T WithTimeout(TimeSpan timeout)
            {
                if (timeout == null)
                    throw new ArgumentNullException(nameof(timeout), "Timeout can not be null.");

                if (timeout == TimeSpan.Zero)
                    throw new ArgumentException("Timeout can not be equal to zero.", nameof(timeout));

                Configuration.Timeout = timeout;

                return Configurator;
            }

            public T SkipStatusCodeValidation()
            {
                Configuration.SkipStatusCodeValidation = true;

                return Configurator;
            }

            public T WithHttpClientProvider(Func<IHttpClient> httpClientProvider)
            {
                if (httpClientProvider == null)
                    throw new ArgumentNullException(nameof(httpClientProvider), "HTTP client provider can not be null.");

                Configuration.HttpClientProvider = httpClientProvider;

                return Configurator;
            }

            public T EnsureThat(Func<HttpResponseMessage, bool> ensureThat)
            {
                if (ensureThat == null)
                    throw new ArgumentException("Ensure that predicate can not be null.", nameof(ensureThat));

                Configuration.EnsureThat = ensureThat;

                return Configurator;
            }

            public T EnsureThatAsync(Func<HttpResponseMessage, Task<bool>> ensureThat)
            {
                if (ensureThat == null)
                    throw new ArgumentException("Ensure that async predicate can not be null.", nameof(ensureThat));

                Configuration.EnsureThatAsync = ensureThat;

                return Configurator;
            }
        }

        public class Default : Configurator<Default>
        {
            public Default(WebsiteWatcherConfiguration configuration) : base(configuration)
            {
                SetInstance(this);
            }
        }

        public class Builder : Configurator<Builder>
        {
            public Builder(string url) : base(url)
            {
                SetInstance(this);
            }
        
            public WebsiteWatcherConfiguration Build() => Configuration;

            public static explicit operator Default(Builder builder) => new Default(builder.Configuration);
        }
    }
}