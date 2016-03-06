using System;

namespace Sentry.Watchers.Website
{
    public class WebsiteWatcherConfiguration
    {
        public string Url { get; protected set; }

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

                _configuration.Url = url;

                return this;
            }

            public WebsiteWatcherConfiguration Build() => _configuration;
        }
    }
}