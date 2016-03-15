using System;
using System.Collections.Generic;
using System.Net.Http;

namespace Sentry.Watchers.Api
{
    public class ApiWatcherConfiguration
    {
        public Uri Uri { get; protected set; }
        public HttpRequest Request { get; protected set; }
        public bool SkipStatusCodeValidation { get; protected set; }
        public IDictionary<string, string> Headers { get; protected set; }
        public TimeSpan Timeout { get; protected set; }
        public Func<HttpResponseMessage, bool> WhereValidResponseIs { get; protected set; }

        protected internal ApiWatcherConfiguration(string url, HttpRequest request)
        {
            if (string.IsNullOrEmpty(url))
                throw new ArgumentException("URL can not be empty.", nameof(url));

            if (request == null)
                throw new ArgumentNullException(nameof(request), "Request can not be null.");

            Uri = new Uri(url);
            Headers = new Dictionary<string, string>();
            Timeout = TimeSpan.Zero;
            Request = request;
        }

        public static Builder Create(string url, HttpRequest request) => new Builder(url, request);

        public class Builder
        {
            private readonly ApiWatcherConfiguration _configuration;

            public Builder(string url, HttpRequest request)
            {
                _configuration = new ApiWatcherConfiguration(url, request);
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

            public Builder WithRequest(HttpRequest request)
            {
                if (request == null)
                    throw new ArgumentNullException(nameof(request), "HTTP request can not be null.");

                _configuration.Request = request;

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

            public Builder EnsureThat(Func<HttpResponseMessage, bool> predicate)
            {
                _configuration.WhereValidResponseIs = predicate;

                return this;
            }

            public ApiWatcherConfiguration Build() => _configuration;
        }
    }
}