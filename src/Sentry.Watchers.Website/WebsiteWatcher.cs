using System;
using System.Threading.Tasks;

namespace Sentry.Watchers.Website
{
    public class WebsiteWatcher : IWatcher
    {
        private readonly WebsiteWatcherConfiguration _configuration;

        public WebsiteWatcher(WebsiteWatcherConfiguration configuration)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration), "WebsiteWatcher configuration has not been provided.");

            _configuration = configuration;
        }

        public async Task ExecuteAsync()
        {
            throw new System.NotImplementedException();
        }
    }
}