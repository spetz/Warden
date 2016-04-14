using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Warden.Configurations;

namespace Warden.Integrations.Api
{
    /// <summary>
    /// Custom extension methods for the HTTP API integration.
    /// </summary>
    public static class Extensions
    {
        private static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings
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
        /// Extension method for adding the HTTP API integration to the the WardenConfiguration.
        /// </summary>
        /// <param name="builder">Instance of the Warden configuration builder.</param>
        /// <param name="url">URL of the HTTP API.</param>
        /// <param name="apiKey">API key of the HTTP API.</param>
        /// <param name="configurator">Optional lambda expression for configuring the HttpApiIntegration.</param>
        public static WardenConfiguration.Builder IntegrateWithHttpApi(
            this WardenConfiguration.Builder builder,
            string url, string apiKey,
            Action<HttpApiIntegrationConfiguration.Builder> configurator = null)
        {
            builder.AddIntegration(HttpApiIntegration.Create(url, apiKey));

            return builder;
        }

        /// <summary>
        /// Extension method for resolving the HTTP API integration from the IIntegrator.
        /// </summary>
        /// <param name="integrator">Instance of the IIntegrator.</param>
        /// <returns>Instance of fluent builder for the WardenConfiguration.</returns>
        public static HttpApiIntegration HttpApi(this IIntegrator integrator)
            => integrator.Resolve<HttpApiIntegration>();

        /// <summary>
        /// Serialize object to the JSON string.
        /// </summary>
        /// <param name="data">Data to be serialized.</param>
        /// <returns>JSON string.</returns>
        public static string ToJson(this object data)
        {
            return data.ToJson(null);
        }

        /// <summary>
        /// Serialize object to the JSON.
        /// </summary>
        /// <param name="data">Data to be serialized.</param>
        /// <param name="serializerSettings">Custom settings of the JSON Serializer.</param>
        /// <returns>JSON string.</returns>
        public static string ToJson(this object data, JsonSerializerSettings serializerSettings)
        {
            serializerSettings = serializerSettings ?? JsonSerializerSettings;

            return JsonConvert.SerializeObject(data, serializerSettings);
        }
    }
}