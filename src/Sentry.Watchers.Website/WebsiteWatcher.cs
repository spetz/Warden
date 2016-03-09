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
        public string Name { get; }

        protected WebsiteWatcher(string name, WebsiteWatcherConfiguration configuration)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Watcher name can not be empty.");

            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration),
                    "WebsiteWatcher configuration has not been provided.");
            }

            Name = name;
            _configuration = configuration;
            _httpClient = new HttpClient();
        }

        public async Task<IWatcherCheckResult> ExecuteAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync(_configuration.Uri);
                var isValid = HasValidRespons(response);

                return WebsiteWatcherCheckResult.Create(this, isValid, _configuration.Uri,
                    _httpClient.DefaultRequestHeaders, response,
                    $"Websiste for URL: '{_configuration.Uri}' has returned a response with status code: {response.StatusCode}.");
            }
            catch (Exception ex)
            {
                throw new WatcherException($"There was an error while trying to access URL: '{_configuration.Uri}'.", ex);
            }
        }

        private bool HasValidRespons(HttpResponseMessage response)
            => (_configuration.WhereValidResponseIs?.Invoke(response) ?? true) &&
               (response.IsSuccessStatusCode || _configuration.SkipStatusCodeValidation);

        public static WebsiteWatcher Create(string name, string url, Action<WebsiteWatcherConfiguration.Builder> configuration = null)
        {
            var config = new WebsiteWatcherConfiguration.Builder(url);
            configuration?.Invoke(config);

            return Create(name, config.Build());
        }

        public static WebsiteWatcher Create(string name, WebsiteWatcherConfiguration configuration)
            => new WebsiteWatcher(name, configuration);
    }
}