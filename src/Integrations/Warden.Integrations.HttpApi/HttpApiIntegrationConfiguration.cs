using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Warden.Integrations.Api
{
    /// <summary>
    /// Configuration of the HttpApiIntegration.
    /// </summary>
    public class HttpApiIntegrationConfiguration
    {
        public static readonly JsonSerializerSettings DefaultJsonSerializerSettings = new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            DateFormatString = "yyyy-MM-dd H:mm:ss",
            Formatting = Formatting.Indented,
            DefaultValueHandling = DefaultValueHandling.Populate,
            NullValueHandling = NullValueHandling.Include,
            Converters = new List<JsonConverter>
            {
                new Newtonsoft.Json.Converters.StringEnumConverter
                {
                    AllowIntegerValues = true,
                    CamelCaseText = true
                }
            }
        };

        /// <summary>
        /// Default request header name of the API key.
        /// </summary>
        public const string ApiKeyHeader = "X-Api-Key";

        /// <summary>
        /// URI of the HTTP API.
        /// </summary>
        public Uri Uri { get; protected set; }

        /// <summary>
        /// API key of the HTTP API passed inside the custom "X-Api-Key" header.
        /// </summary>
        public string ApiKey { get; protected set; }

        /// <summary>
        /// Request headers.
        /// </summary>
        public IDictionary<string, string> Headers { get; protected set; }

        /// <summary>
        /// Optional timeout of the HTTP request.
        /// </summary>
        public TimeSpan? Timeout { get; protected set; }

        /// <summary>
        /// Flag determining whether an exception should be thrown on if PostAsync() returns invalid reponse (false by default).
        /// </summary>
        public bool FailFast { get; protected set; }

        /// <summary>
        /// Custom JSON serializer settings of the Newtonsoft.Json library.
        /// </summary>
        public JsonSerializerSettings JsonSerializerSettings { get; protected set; } = DefaultJsonSerializerSettings;

        /// <summary>
        /// Custom provider for the IHttpService.
        /// </summary>
        public Func<IHttpService> HttpServiceProvider { get; protected set; }

        /// <summary>
        /// Factory method for creating a new instance of fluent builder for the HttpApiIntegrationConfiguration.
        /// </summary>
        /// <param name="url">URL of the HTTP API.</param>
        /// <param name="apiKey">Optional API key of the HTTP API passed inside the custom "X-Api-Key" header.</param>
        /// <param name="headers">Optional request headers.</param>
        /// <returns>Instance of fluent builder for the HttpApiIntegrationConfiguration.</returns>
        public static Builder Create(string url, string apiKey = null, IDictionary<string, string> headers = null)
            => new Builder(url, apiKey, headers);

        protected HttpApiIntegrationConfiguration(string url, string apiKey = null,
            IDictionary<string, string> headers = null)
        {
            if (string.IsNullOrEmpty(url))
                throw new ArgumentException("URL can not be empty.", nameof(url));

            Uri = new Uri(url);
            Headers = headers ?? new Dictionary<string, string>();
            HttpServiceProvider = () => new HttpService(new HttpClient());
            if(string.IsNullOrWhiteSpace(apiKey))
                return;

            ApiKey = apiKey;
            Headers.Add(ApiKeyHeader, ApiKey);
        }

        /// <summary>
        /// Fluent builder for the HttpApiIntegrationConfiguration.
        /// </summary>
        public class Builder
        {
            protected readonly HttpApiIntegrationConfiguration Configuration;

            public Builder(string url, string apiKey = null, IDictionary<string, string> headers = null)
            {
                Configuration = new HttpApiIntegrationConfiguration(url, apiKey, headers);
            }

            /// <summary>
            /// Timeout of the HTTP request.
            /// </summary>
            /// <param name="timeout">Timeout.</param>
            /// <returns>Instance of fluent builder for the HttpApiIntegrationConfiguration.</returns>
            public Builder WithTimeout(TimeSpan timeout)
            {
                if (timeout == null)
                    throw new ArgumentNullException(nameof(timeout), "Timeout can not be null.");

                if (timeout == TimeSpan.Zero)
                    throw new ArgumentException("Timeout can not be equal to zero.", nameof(timeout));

                Configuration.Timeout = timeout;

                return this;
            }

            /// <summary>
            /// Request headers of the HTTP request.
            /// </summary>
            /// <param name="headers">Collection of the HTTP request headers.</param>
            /// <returns>Instance of fluent builder for the HttpApiIntegrationConfiguration.</returns>
            public Builder WithHeaders(IDictionary<string, string> headers)
            {
                if (headers == null || !headers.Any())
                    throw new ArgumentNullException(nameof(headers), "Request headers can not be empty.");

                Configuration.Headers = headers;

                return this;
            }

            /// <summary>
            /// Sets the custom JSON serializer settings of the Newtonsoft.Json library.
            /// </summary>
            /// <param name="jsonSerializerSettings">Custom JSON serializer settings of the Newtonsoft.Json library.</param>
            /// <returns>Instance of fluent builder for the HttpApiIntegrationConfiguration.</returns>
            public Builder WithJsonSerializerSettings(JsonSerializerSettings jsonSerializerSettings)
            {
                if (jsonSerializerSettings == null)
                    throw new ArgumentNullException(nameof(jsonSerializerSettings),
                        "JSON serializer settings can not be null.");

                Configuration.JsonSerializerSettings = jsonSerializerSettings;

                return this;
            }

            /// <summary>
            /// Sets the custom provider for the IHttpService.
            /// </summary>
            /// <param name="httpServiceProvider">Custom provider for the IHttpService.</param>
            /// <returns>Lambda expression returning an instance of the IHttpService.</returns>
            /// <returns>Instance of fluent builder for the HttpApiIntegrationConfiguration.</returns>
            public Builder WithHttpServiceProvider(Func<IHttpService> httpServiceProvider)
            {
                if (httpServiceProvider == null)
                    throw new ArgumentNullException(nameof(httpServiceProvider),
                        "HTTP service provider can not be null.");

                Configuration.HttpServiceProvider = httpServiceProvider;

                return this;
            }

            /// <summary>
            /// Flag determining whether an exception should be thrown on if PostAsync() returns invalid reponse (false by default).
            /// </summary>
            /// <returns>Instance of fluent builder for the HttpApiIntegrationConfiguration.</returns>
            public Builder FailFast()
            {
                Configuration.FailFast = true;

                return this;
            }

            /// <summary>
            /// Builds the HttpApiIntegrationConfiguration and return its instance.
            /// </summary>
            /// <returns>Instance of HttpApiIntegrationConfiguration.</returns>
            public HttpApiIntegrationConfiguration Build() => Configuration;
        }
    }
}