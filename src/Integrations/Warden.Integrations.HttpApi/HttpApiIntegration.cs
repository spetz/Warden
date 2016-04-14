using System;
using System.Threading.Tasks;

namespace Warden.Integrations.Api
{
    /// <summary>
    /// Integration with the HTTP API for sending information about performed checks.
    /// </summary>
    public class HttpApiIntegration : IIntegration
    {
        private readonly HttpApiIntegrationConfiguration _configuration;

        public HttpApiIntegration(HttpApiIntegrationConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration),
                    "HTTP API Integration configuration has not been provided.");
            }

            _configuration = configuration;
        }

        /// <summary>
        /// Sends a POST request to the base URL of the HTTP API.
        /// </summary>
        /// <param name="data">Request data that will be serialized to the JSON.</param>
        /// <returns></returns>
        public async Task PostAsync(object data)
        {
            await PostAsync(string.Empty, data);
        }

        /// <summary>
        /// Sends a POST request to the specified endpoint in the HTTP API.
        /// </summary>
        /// <param name="endpoint">Endpoint of the HTTP operation (e.g. /iterations).</param>
        /// <param name="data">Request data that will be serialized to the JSON.</param>
        /// <returns></returns>
        public async Task PostAsync(string endpoint, object data)
        {
            var baseUrl = _configuration.Uri.ToString();
            var fullUrl = baseUrl.GetFullUrl(endpoint);

            await _configuration.HttpServiceProvider().PostAsync(fullUrl,
                data.ToJson(_configuration.JsonSerializerSettings), _configuration.Headers,
                _configuration.Timeout, _configuration.FailFast);
        }

        /// <summary>
        /// Factory method for creating a new instance of HttpApiIntegration.
        /// </summary>
        /// <param name="url">URL of the HTTP API.</param>
        /// <param name="apiKey">API key of the HTTP API.</param>
        /// <param name="configurator">Lambda expression for configuring the HttpApiIntegration integration.</param>
        /// <returns>Instance of HttpApiIntegration.</returns>
        public static HttpApiIntegration Create(string url, string apiKey,
            Action<HttpApiIntegrationConfiguration.Builder> configurator = null)
        {
            var config = new HttpApiIntegrationConfiguration.Builder(url, apiKey);
            configurator?.Invoke(config);

            return Create(config.Build());
        }

        /// <summary>
        /// Factory method for creating a new instance of HttpApiIntegration.
        /// </summary>
        /// <param name="configuration">Configuration of HTTP API integration.</param>
        /// <returns>Instance of HttpApiIntegration.</returns>
        public static HttpApiIntegration Create(HttpApiIntegrationConfiguration configuration)
            => new HttpApiIntegration(configuration);
    }
}