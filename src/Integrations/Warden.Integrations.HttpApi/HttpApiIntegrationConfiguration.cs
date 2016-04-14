using System;

namespace Warden.Integrations.Api
{
    public class HttpApiIntegrationConfiguration
    {
        /// <summary>
        /// URL of the HTTP API.
        /// </summary>
        public Uri Url { get; protected set; }

        /// <summary>
        /// API key of the HTTP API.
        /// </summary>
        public string ApiKey { get; protected set; }

        protected HttpApiIntegrationConfiguration(string url, string apiKey)
        {
            if (string.IsNullOrEmpty(url))
                throw new ArgumentException("URL can not be empty.", nameof(url));

            if (string.IsNullOrWhiteSpace(apiKey))
                throw new ArgumentException("API key can not be empty.", nameof(apiKey));

            Url = new Uri(url);
            ApiKey = apiKey;
        }

        /// <summary>
        /// Fluent builder for the HttpApiIntegrationConfiguration.
        /// </summary>
        public class Builder
        {
            protected readonly HttpApiIntegrationConfiguration Configuration;

            public Builder(string url, string apiKey)
            {
                Configuration = new HttpApiIntegrationConfiguration(url, apiKey);
            }

            /// <summary>
            /// Builds the HttpApiIntegrationConfiguration and return its instance.
            /// </summary>
            /// <returns>Instance of HttpApiIntegrationConfiguration.</returns>
            public HttpApiIntegrationConfiguration Build() => Configuration;
        }
    }
}