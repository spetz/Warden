using System;
using System.Threading.Tasks;

namespace Warden.Watchers.Web
{
    /// <summary>
    /// WebWatcher designed for website or API monitoring.
    /// </summary>
    public class WebWatcher : IWatcher
    {
        private readonly WebWatcherConfiguration _configuration;
        private readonly IHttpService _httpService;
        public string Name { get; }
        public string Group { get; }
        public const string DefaultName = "Web Watcher";

        protected WebWatcher(string name, WebWatcherConfiguration configuration, string group)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Watcher name can not be empty.");

            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration),
                    "Web Watcher configuration has not been provided.");
            }

            Name = name;
            _configuration = configuration;
            Group = group;
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

                return await EnsureAsync(fullUrl, response);
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

        private async Task<IWatcherCheckResult> EnsureAsync(string fullUrl, IHttpResponse response)
        {
            var isValid = true;
            if (_configuration.EnsureThatAsync != null)
                isValid = await _configuration.EnsureThatAsync?.Invoke(response);

            isValid = isValid && (_configuration.EnsureThat?.Invoke(response) ?? true);

            return WebWatcherCheckResult.Create(this,
                isValid, _configuration.Uri,
                _configuration.Request, response,
                $"Web endpoint: '{fullUrl}' has returned a response with status code: {response.StatusCode}.");
        }

        private bool HasValidResponse(IHttpResponse response)
            => response.IsValid || _configuration.SkipStatusCodeValidation;

        /// <summary>
        /// Factory method for creating a new instance of WebWatcher with default name of Web Watcher.
        /// Uses the default HTTP GET request.
        /// </summary>
        /// <param name="url">Base URL of the request.</param>
        /// <param name="configurator">Optional lambda expression for configuring the WebWatcher.</param>
        /// <param name="group">Optional name of the group that WebWatcher belongs to.</param>
        /// <returns>Instance of WebWatcher.</returns>
        public static WebWatcher Create(string url, Action<WebWatcherConfiguration.Default> configurator = null, 
            string group = null)
        {
            var config = new WebWatcherConfiguration.Builder(url);
            configurator?.Invoke((WebWatcherConfiguration.Default)config);

            return Create(DefaultName, config.Build(), group);
        }

        /// <summary>
        /// Factory method for creating a new instance of WebWatcher with default name of Web Watcher.
        /// </summary>
        /// <param name="url">Base URL of the request.</param>
        /// <param name="request">Instance of the IHttpRequest.</param>
        /// <param name="configurator">Optional lambda expression for configuring the WebWatcher.</param>
        /// <param name="group">Optional name of the group that WebWatcher belongs to.</param>
        /// <returns>Instance of WebWatcher.</returns>
        public static WebWatcher Create(string url, IHttpRequest request,
            Action<WebWatcherConfiguration.Default> configurator = null, string group = null)
            => Create(DefaultName, url, request, configurator, group);

        /// <summary>
        /// Factory method for creating a new instance of WebWatcher.
        /// </summary>
        /// <param name="name">Name of the WebWatcher.</param>
        /// <param name="url">Base URL of the request.</param>
        /// <param name="request">Instance of the IHttpRequest.</param>
        /// <param name="configurator">Optional lambda expression for configuring the WebWatcher.</param>
        /// <param name="group">Optional name of the group that WebWatcher belongs to.</param>
        /// <returns>Instance of WebWatcher.</returns>
        public static WebWatcher Create(string name, string url, IHttpRequest request,
            Action<WebWatcherConfiguration.Default> configurator = null, string group = null)
        {
            var config = new WebWatcherConfiguration.Builder(url, request);
            configurator?.Invoke((WebWatcherConfiguration.Default) config);

            return Create(name, config.Build(), group);
        }

        /// <summary>
        /// Factory method for creating a new instance of WebWatcher with default name of Web Watcher.
        /// </summary>
        /// <param name="configuration">Configuration of WebWatcher.</param>
        /// <param name="group">Optional name of the group that WebWatcher belongs to.</param>
        /// <returns>Instance of WebWatcher.</returns>
        public static WebWatcher Create(WebWatcherConfiguration configuration, string group = null)
            => Create(DefaultName, configuration, group);

        /// <summary>
        /// Factory method for creating a new instance of WebWatcher.
        /// </summary>
        /// <param name="name">Name of the WebWatcher.</param>
        /// <param name="configuration">Configuration of WebWatcher.</param>
        /// <param name="group">Optional name of the group that WebWatcher belongs to.</param>
        /// <returns>Instance of WebWatcher.</returns>
        public static WebWatcher Create(string name, WebWatcherConfiguration configuration, string group = null)
            => new WebWatcher(name, configuration, group);
    }
}