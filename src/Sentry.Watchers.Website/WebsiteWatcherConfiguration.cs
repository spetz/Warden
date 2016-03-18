using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Sentry.Watchers.Website
{
    public class WebsiteWatcherConfiguration
    {
        public Uri Uri { get; protected set; }
        public bool SkipStatusCodeValidation { get; protected set; }
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
        }

        public static Builder Create(string url) => new Builder(url);

        public class Builder
        {
            private readonly WebsiteWatcherConfiguration _configuration;

            public Builder(string url)
            {
                _configuration = new WebsiteWatcherConfiguration(url);
            }

            public Builder WithHeaders(IDictionary<string, string> headers)
            {
                if (headers == null)
                    throw new ArgumentNullException(nameof(headers), "Headers can not be null.");

                foreach (var header in headers)
                {
                    WithHeader(header);
                }

                return this;
            }

            public Builder WithHeader(KeyValuePair<string, string> header)
            {
                _configuration.Headers.Add(header);

                return this;
            }

            public Builder WithTimeout(TimeSpan timeout)
            {
                if (timeout == null)
                    throw new ArgumentNullException(nameof(timeout), "Timeout can not be null.");

                if(timeout == TimeSpan.Zero)
                    throw new ArgumentException("Timeout can not be equal to zero.", nameof(timeout));

                _configuration.Timeout = timeout;

                return this;
            }

            public Builder SkipStatusCodeValidation()
            {
                _configuration.SkipStatusCodeValidation = true;

                return this;
            }

            public Builder EnsureThat(Func<HttpResponseMessage, bool> ensureThat)
            {
                if (ensureThat == null)
                    throw new ArgumentException("Ensure that predicate can not be null.", nameof(ensureThat));

                _configuration.EnsureThat = ensureThat;

                return this;
            }

            public Builder EnsureThatAsync(Func<HttpResponseMessage, Task<bool>> ensureThat)
            {
                if (ensureThat == null)
                    throw new ArgumentException("Ensure that async predicate can not be null.", nameof(ensureThat));

                _configuration.EnsureThatAsync = ensureThat;

                return this;
            }

            public WebsiteWatcherConfiguration Build() => _configuration;
        }
    }
}