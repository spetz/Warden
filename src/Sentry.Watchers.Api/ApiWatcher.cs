using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Sentry.Core;

namespace Sentry.Watchers.Api
{
    public class ApiWatcher : IWatcher
    {
        private readonly ApiWatcherConfiguration _configuration;
        private readonly IHttpClient _httpClient;
        public string Name { get; }

        protected ApiWatcher(string name, ApiWatcherConfiguration configuration)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Watcher name can not be empty.");

            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration),
                    "API watcher configuration has not been provided.");
            }

            Name = name;
            _configuration = configuration;
            _httpClient = configuration.HttpClientProvider();
            _httpClient.BaseAddress = _configuration.Uri;
            SetRequestHeaders();
            if (_configuration.Timeout > TimeSpan.Zero)
                _httpClient.Timeout = _configuration.Timeout;
        }

        public async Task<IWatcherCheckResult> ExecuteAsync()
        {
            var endpoint = _configuration.Request.Endpoint;
            var data = _configuration.Request.Data;
            var method = _configuration.Request.Type;
            var fullUrl = $"{_configuration.Uri}{endpoint}";
            try
            {
                HttpResponseMessage response = null;
                switch (method)
                {
                    case HttpRequest.MethodType.Get:
                        response = await _httpClient.GetAsync(endpoint);
                        break;
                    case HttpRequest.MethodType.Put:
                        response = await _httpClient.PostAsync(endpoint, data);
                        break;
                    case HttpRequest.MethodType.Post:
                        response = await _httpClient.PostAsync(endpoint, data);
                        break;
                    case HttpRequest.MethodType.Delete:
                        response = await _httpClient.DeleteAsync(endpoint);
                        break;
                    default: throw new ArgumentException($"Invalid HTTP method: {method}.", nameof(method));
                }

                var isValid = HasValidResponse(response);
                if (!isValid)
                {
                    return WatcherCheckResult.Create(this, false,
                        $"API endpoint: '{fullUrl}' has returned an invalid response with status code: {response.StatusCode}.");
                }

                if (_configuration.EnsureThatAsync != null)
                    isValid = await _configuration.EnsureThatAsync?.Invoke(response);

                isValid = isValid && (_configuration.EnsureThat?.Invoke(response) ?? true);

                return ApiWatcherCheckResult.Create(this, isValid, _configuration.Uri, _configuration.Request,
                    _httpClient.RequestHeaders, response,
                    $"API endpoint: '{fullUrl}' has returned a response with status code: {response.StatusCode}.");
            }
            catch (TaskCanceledException exception)
            {
                return ApiWatcherCheckResult.Create(this, false, _configuration.Uri, _configuration.Request,
                    _httpClient.RequestHeaders, null,
                    $"A connection timeout occurred while trying to access the API endpoint: '{fullUrl}'.");
            }
            catch (Exception exception)
            {
                throw new WatcherException($"There was an error while trying to access the API endpoint: '{fullUrl}'.",
                    exception);
            }
        }

        private void SetRequestHeaders()
        {
            foreach (var header in _configuration.Headers)
            {
                var existingHeader = _httpClient.RequestHeaders
                    .FirstOrDefault(x => string.Equals(x.Key, header.Key, StringComparison.InvariantCultureIgnoreCase));
                if (existingHeader.Key != null)
                    _httpClient.RequestHeaders.Remove(existingHeader.Key);

                _httpClient.RequestHeaders.Add(header.Key, header.Value);
            }
        }

        private bool HasValidResponse(HttpResponseMessage response)
            => response.IsSuccessStatusCode || _configuration.SkipStatusCodeValidation;

        public static ApiWatcher Create(string name, string url, HttpRequest request,
            Action<ApiWatcherConfiguration.Default> configurator = null)
        {
            var config = new ApiWatcherConfiguration.Builder(url, request);
            configurator?.Invoke((ApiWatcherConfiguration.Default)config);

            return Create(name, config.Build());
        }

        public static ApiWatcher Create(string name, ApiWatcherConfiguration configuration)
            => new ApiWatcher(name, configuration);
    }
}