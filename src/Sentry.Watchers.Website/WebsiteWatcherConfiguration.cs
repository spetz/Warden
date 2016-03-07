using System;

namespace Sentry.Watchers.Website
{
    public class WebsiteWatcherConfiguration
    {
        public string Name { get; protected set; }
        public Uri Uri { get; protected set; }
        public bool SkipStatusCodeValidation { get; protected set; }

        protected internal WebsiteWatcherConfiguration()
        {
        }

        public static WebsiteWatcherConfiguration Empty => new WebsiteWatcherConfiguration();

        public static WebsiteWatcherConfigurationBuilder Create(string name) => new WebsiteWatcherConfigurationBuilder(name);

        public class WebsiteWatcherConfigurationBuilder
        {
            private readonly WebsiteWatcherConfiguration _configuration = new WebsiteWatcherConfiguration();

            public WebsiteWatcherConfigurationBuilder(string name)
            {
                if (string.IsNullOrEmpty(name))
                    throw new ArgumentException("Watcher name can not be empty.");

                _configuration.Name = name;
            }

            public WebsiteWatcherConfigurationBuilder WithUrl(string url)
            {
                if (string.IsNullOrEmpty(url))
                    throw new ArgumentException("URL can not be empty.");

                _configuration.Uri = new Uri(url);

                return this;
            }


            public WebsiteWatcherConfigurationBuilder SkipStatusCodeValidation()
            {
                _configuration.SkipStatusCodeValidation = true;

                return this;
            }

            public WebsiteWatcherConfiguration Build() => _configuration;
        }
    }
}