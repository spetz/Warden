using System;
using System.Linq;
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
            _httpClient = new HttpClient
            {
                BaseAddress = _configuration.Uri
            };
            SetRequestHeaders();
            if (_configuration.Timeout > TimeSpan.Zero)
                _httpClient.Timeout = _configuration.Timeout;
        }

        public async Task<IWatcherCheckResult> ExecuteAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync(string.Empty);
                var isValid = HasValidResponse(response);
                if (!isValid)
                {
                    return WatcherCheckResult.Create(this, false,
                        $"Websiste for URL: '{_configuration.Uri}' has returned an invalid response with status code: {response.StatusCode}.");
                }

                if (_configuration.EnsureThatAsync != null)
                    isValid = await _configuration.EnsureThatAsync?.Invoke(response);

                isValid = isValid && (_configuration.EnsureThat?.Invoke(response) ?? true);

                return WebsiteWatcherCheckResult.Create(this, isValid, _configuration.Uri,
                    _httpClient.DefaultRequestHeaders, response,
                    $"Websiste for URL: '{_configuration.Uri}' has returned a response with status code: {response.StatusCode}.");
            }
            catch (TaskCanceledException exception)
            {
                return WebsiteWatcherCheckResult.Create(this, false, _configuration.Uri,
                    _httpClient.DefaultRequestHeaders, null,
                    $"A connection timeout occurred while trying to access the website for URL: '{_configuration.Uri}'");
            }
            catch (Exception exception)
            {
                throw new WatcherException($"There was an error while trying to access the URL: '{_configuration.Uri}'.", exception);
            }
        }

        private void SetRequestHeaders()
        {
            foreach (var header in _configuration.Headers)
            {
                var existingHeader = _httpClient.DefaultRequestHeaders
                    .FirstOrDefault(x => string.Equals(x.Key, header.Key, StringComparison.InvariantCultureIgnoreCase));
                if (existingHeader.Key != null)
                    _httpClient.DefaultRequestHeaders.Remove(existingHeader.Key);

                _httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
            }
        }

        private bool HasValidResponse(HttpResponseMessage response)
            => response.IsSuccessStatusCode || _configuration.SkipStatusCodeValidation;

        public static WebsiteWatcher Create(string name, string url,
            Action<WebsiteWatcherConfiguration.Default> configurator = null)
        {
            var config = WebsiteWatcherConfiguration.Create(url);
            configurator?.Invoke((WebsiteWatcherConfiguration.Default)config);

            return Create(name, config.Build());
        }

        public static WebsiteWatcher Create(string name, WebsiteWatcherConfiguration configuration)
            => new WebsiteWatcher(name, configuration);
    }
}