using System;
using System.Threading.Tasks;
using Sentry.Core;

namespace Sentry.Watchers.Web
{
    /// <summary>
    /// WebWatcher designed for website or API monitoring.
    /// </summary>
    public class WebWatcher : IWatcher
    {
        private readonly WebWatcherConfiguration _configuration;
        private readonly IHttpService _httpService;
        public string Name { get; }
        public const string DefaultName = "Web Watcher";

        protected WebWatcher(string name, WebWatcherConfiguration configuration)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Watcher name can not be empty.");

            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration),
                    "Web watcher configuration has not been provided.");
            }

            Name = name;
            _configuration = configuration;
            _httpService = configuration.HttpServiceProvider();
        }

        public async Task<IWatcherCheckResult> ExecuteAsync()
        {
            var baseUrl = _configuration.Uri.ToString();
            var fullUrl = _configuration.Request.GetFullUrl(baseUrl);
            try
            {
                var response = await _httpService.ExecuteAsync(baseUrl, _configuration.Request, _configuration.Timeout);
                var isValid = HasValidResponse(response);
                if (!isValid)
                {
                    return WebWatcherCheckResult.Create(this, false,
                        _configuration.Uri, _configuration.Request, response,
                        $"Web endpoint: '{fullUrl}' has returned an invalid response with status code: {response.StatusCode}.");
                }

                if (_configuration.EnsureThatAsync != null)
                    isValid = await _configuration.EnsureThatAsync?.Invoke(response);

                isValid = isValid && (_configuration.EnsureThat?.Invoke(response) ?? true);
                return WebWatcherCheckResult.Create(this,
                    isValid, _configuration.Uri,
                    _configuration.Request, response,
                    $"Web endpoint: '{fullUrl}' has returned a response with status code: {response.StatusCode}.");
            }
            catch (TaskCanceledException exception)
            {
                return WebWatcherCheckResult.Create(this,
                    false, _configuration.Uri,
                    _configuration.Request, null,
                    $"A connection timeout occurred while trying to access the Web endpoint: '{fullUrl}'.");
            }
            catch (Exception exception)
            {
                throw new WatcherException($"There was an error while trying to access the Web endpoint: '{fullUrl}'.",
                    exception);
            }
        }

        private bool HasValidResponse(IHttpResponse response)
            => response.IsValid || _configuration.SkipStatusCodeValidation;

        /// <summary>
        /// Factory method for creating a new instance of WebWatcher with default name of Web Watcher.
        /// Uses the default HTTP GET request.
        /// </summary>
        /// <param name="url">Base URL of the request.</param>
        /// <param name="configurator">Optional lambda expression for configuring the WebWatcher.</param>
        /// <returns>Instance of WebWatcher.</returns>
        public static WebWatcher Create(string url, Action<WebWatcherConfiguration.Default> configurator = null)
        {
            var config = new WebWatcherConfiguration.Builder(url);
            configurator?.Invoke((WebWatcherConfiguration.Default)config);

            return Create(DefaultName, config.Build());
        }

        /// <summary>
        /// Factory method for creating a new instance of WebWatcher with default name of Web Watcher.
        /// </summary>
        /// <param name="url">Base URL of the request.</param>
        /// <param name="request">Instance of the IHttpRequest.</param>
        /// <param name="configurator">Optional lambda expression for configuring the WebWatcher.</param>
        /// <returns>Instance of WebWatcher.</returns>
        public static WebWatcher Create(string url, IHttpRequest request,
            Action<WebWatcherConfiguration.Default> configurator = null)
            => Create(DefaultName, url, request, configurator);

        /// <summary>
        /// Factory method for creating a new instance of WebWatcher.
        /// </summary>
        /// <param name="name">Name of the WebWatcher.</param>
        /// <param name="url">Base URL of the request.</param>
        /// <param name="request">Instance of the IHttpRequest.</param>
        /// <param name="configurator">Optional lambda expression for configuring the WebWatcher.</param>
        /// <returns>Instance of WebWatcher.</returns>
        public static WebWatcher Create(string name, string url, IHttpRequest request,
            Action<WebWatcherConfiguration.Default> configurator = null)
        {
            var config = new WebWatcherConfiguration.Builder(url, request);
            configurator?.Invoke((WebWatcherConfiguration.Default) config);

            return Create(name, config.Build());
        }

        /// <summary>
        /// Factory method for creating a new instance of WebWatcher with default name of Web Watcher.
        /// </summary>
        /// <param name="configuration">Configuration of WebWatcher.</param>
        /// <returns>Instance of WebWatcher.</returns>
        public static WebWatcher Create(WebWatcherConfiguration configuration)
            => Create(DefaultName, configuration);

        /// <summary>
        /// Factory method for creating a new instance of WebWatcher.
        /// </summary>
        /// <param name="name">Name of the WebWatcher.</param>
        /// <param name="configuration">Configuration of WebWatcher.</param>
        /// <returns>Instance of WebWatcher.</returns>
        public static WebWatcher Create(string name, WebWatcherConfiguration configuration)
            => new WebWatcher(name, configuration);
    }
}