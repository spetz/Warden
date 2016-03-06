using System;

namespace Sentry.Watchers.Website
{
    public class WebsiteWatcherConfiguration
    {
        public Uri Uri { get; protected set; }

        protected internal WebsiteWatcherConfiguration()
        {
        }

        public static WebsiteWatcherConfiguration Empty => new WebsiteWatcherConfiguration();
        public static WebsiteWatcherConfigurationBuilder Configure() => new WebsiteWatcherConfigurationBuilder();

        public class WebsiteWatcherConfigurationBuilder
        {
            private readonly WebsiteWatcherConfiguration _configuration = new WebsiteWatcherConfiguration();

            public WebsiteWatcherConfigurationBuilder WithUrl(string url)
            {
                if (string.IsNullOrEmpty(url))
                    throw new ArgumentException("URL can not be empty.");

                _configuration.Uri = new Uri(url);

                return this;
            }

            public WebsiteWatcherConfiguration Build() => _configuration;
        }
    }
}