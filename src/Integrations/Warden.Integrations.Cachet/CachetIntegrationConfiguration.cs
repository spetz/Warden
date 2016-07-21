using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Warden.Integrations.Cachet
{
    /// <summary>
    /// Configuration of the CachetIntegration.
    /// </summary>
    public class CachetIntegrationConfiguration
    {
        public static readonly JsonSerializerSettings DefaultJsonSerializerSettings = new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            DateFormatString = "yyyy-MM-dd H:mm:ss",
            Formatting = Formatting.Indented,
            DefaultValueHandling = DefaultValueHandling.Populate,
            NullValueHandling = NullValueHandling.Include,
            Error = (serializer, error) => { error.ErrorContext.Handled = true; },
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
        public const string AccessTokenHeader = "X-Cachet-Token";

        /// <summary>
        /// URL of the Cachet API.
        /// </summary>
        public Uri ApiUrl { get; protected set; }

        /// <summary>
        /// Access token of the Cachet account.
        /// </summary>
        public string AccessToken { get; protected set; }

        /// <summary>
        /// Username of the Cachet account.
        /// </summary>
        public string Username { get; protected set; }

        /// <summary>
        /// Password of the Cachet account.
        /// </summary>
        public string Password { get; protected set; }

        /// <summary>
        /// Optional timeout of the HTTP request.
        /// </summary>
        public TimeSpan? Timeout { get; protected set; }

        /// <summary>
        /// Custom JSON serializer settings of the Newtonsoft.Json library.
        /// </summary>
        public JsonSerializerSettings JsonSerializerSettings { get; protected set; } = DefaultJsonSerializerSettings;

        /// <summary>
        /// Factory method for creating a new instance of fluent builder for the CachetIntegrationConfiguration.
        /// </summary>
        /// <param name="apiUrl">URL of the Cachet API.</param>
        /// <param name="accessToken">Access token of the Cachet account.</param>
        /// <returns>Instance of fluent builder for the CachetIntegrationConfiguration.</returns>
        public static Builder Create(string apiUrl, string accessToken)
            => new Builder(apiUrl, accessToken);

        /// <summary>
        /// Factory method for creating a new instance of fluent builder for the CachetIntegrationConfiguration.
        /// </summary>
        /// <param name="apiUrl">URL of the Cachet API.</param>
        /// <param name="username">Username of the Cachet account.</param>
        /// <param name="password">Password of the Cachet account.</param>
        /// <returns>Instance of fluent builder for the CachetIntegrationConfiguration.</returns>
        public static Builder Create(string apiUrl, string username, string password) 
            => new Builder(apiUrl, username, password);

        protected CachetIntegrationConfiguration(string apiUrl, string accessToken)
        {
            if (string.IsNullOrEmpty(apiUrl))
                throw new ArgumentException("API URL can not be empty.", nameof(apiUrl));
            if (string.IsNullOrWhiteSpace(accessToken))
                throw new ArgumentException("Access token can not be empty.", nameof(accessToken));

            ApiUrl = new Uri(apiUrl);
            AccessToken = accessToken;
        }

        protected CachetIntegrationConfiguration(string apiUrl, string username, string password)
        {
            if (string.IsNullOrEmpty(apiUrl))
                throw new ArgumentException("API URL can not be empty.", nameof(apiUrl));
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("Username can not be empty.", nameof(username));
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Password can not be empty.", nameof(password));

            ApiUrl = new Uri(apiUrl);
            Username = username;
            Password = password;
        }

        /// <summary>
        /// Fluent builder for the SlackIntegrationConfiguration.
        /// </summary>
        public class Builder
        {
            protected readonly CachetIntegrationConfiguration Configuration;

            /// <summary>
            /// Constructor of fluent builder for the CachetIntegrationConfiguration.
            /// </summary>
            /// <param name="apiUrl">URL of the Cachet API.</param>
            /// <param name="accessToken">Access token of the Cachet account.</param>
            /// <returns>Instance of fluent builder for the CachetIntegrationConfiguration.</returns>
            public Builder(string apiUrl, string accessToken)
            {
                Configuration = new CachetIntegrationConfiguration(apiUrl, accessToken);
            }

            /// <summary>
            /// Constructor of fluent builder for the CachetIntegrationConfiguration.
            /// </summary>
            /// <param name="apiUrl">URL of the Cachet API.</param>
            /// <param name="username">Username of the Cachet account.</param>
            /// <param name="password">Password of the Cachet account.</param>
            /// <returns>Instance of fluent builder for the CachetIntegrationConfiguration.</returns>
            public Builder(string apiUrl, string username, string password)
            {
                Configuration = new CachetIntegrationConfiguration(apiUrl, username, password);
            }

            /// <summary>
            /// Timeout of the HTTP request.
            /// </summary>
            /// <param name="timeout">Timeout.</param>
            /// <returns>Instance of fluent builder for the CachetIntegrationConfiguration.</returns>
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
            /// Sets the custom JSON serializer settings of the Newtonsoft.Json library.
            /// </summary>
            /// <param name="jsonSerializerSettings">Custom JSON serializer settings of the Newtonsoft.Json library.</param>
            /// <returns>Instance of fluent builder for the CachetIntegrationConfiguration.</returns>
            public Builder WithJsonSerializerSettings(JsonSerializerSettings jsonSerializerSettings)
            {
                if (jsonSerializerSettings == null)
                    throw new ArgumentNullException(nameof(jsonSerializerSettings),
                        "JSON serializer settings can not be null.");

                Configuration.JsonSerializerSettings = jsonSerializerSettings;

                return this;
            }

            /// <summary>
            /// Builds the CachetIntegrationConfiguration and return its instance.
            /// </summary>
            /// <returns>Instance of CachetIntegrationConfiguration.</returns>
            public CachetIntegrationConfiguration Build() => Configuration;
        }
    }
}