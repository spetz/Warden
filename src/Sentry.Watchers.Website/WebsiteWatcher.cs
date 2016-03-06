using System;
using System.Net.Http;
using System.Threading.Tasks;
using Sentry.Core;

namespace Sentry.Watchers.Website
{
    public class WebsiteWatcher : IWatcher
    {
        private readonly WebsiteWatcherConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public WebsiteWatcher(WebsiteWatcherConfiguration configuration)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration), "WebsiteWatcher configuration has not been provided.");

            _configuration = configuration;
            _httpClient = new HttpClient();
        }

        public async Task ExecuteAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync(_configuration.Uri);
                if (response.IsSuccessStatusCode)
                    return;

                throw new WatcherException($"The server has returned invalid response while trying to access URL: '{_configuration.Uri}' " +
                                           $"(status code: {response.StatusCode}). {response.ReasonPhrase}");
            }
            catch (HttpRequestException ex)
            {
                throw new WatcherException($"There was an error while trying to access URL: '{_configuration.Uri}'.", ex);
            }
        }
    }
}