using System;
using System.Net.Http;

namespace Sentry.Watchers.Website
{
    public class WebsiteWatcherConfiguration
    {
        public Uri Uri { get; protected set; }
        public bool SkipStatusCodeValidation { get; protected set; }
        public Func<HttpResponseMessage, bool> WhereValidResponseIs { get; protected set; }

        protected internal WebsiteWatcherConfiguration(string url)
        {
            if (string.IsNullOrEmpty(url))
                throw new ArgumentException("URL can not be empty.");

            Uri = new Uri(url);
        }

        public static Builder Create(string url) => new Builder(url);

        public class Builder
        {
            private readonly WebsiteWatcherConfiguration _configuration;

            public Builder(string url)
            {
                _configuration = new WebsiteWatcherConfiguration(url);
            }

            public Builder SkipStatusCodeValidation()
            {
                _configuration.SkipStatusCodeValidation = true;

                return this;
            }

            public Builder WhereValidResponseIs(Func<HttpResponseMessage, bool> predicate)
            {
                _configuration.WhereValidResponseIs = predicate;

                return this;
            }

            public WebsiteWatcherConfiguration Build() => _configuration;
        }
    }
}