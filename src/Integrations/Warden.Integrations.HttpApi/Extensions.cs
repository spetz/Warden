using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Warden.Configurations;

namespace Warden.Integrations.Api
{
    /// <summary>
    /// Custom extension methods for the HTTP API integration.
    /// </summary>
    public static class Extensions
    {
        internal static string GetFullUrl(this string baseUrl, string endpoint)
        {
            if (string.IsNullOrWhiteSpace(endpoint))
                return baseUrl;

            if (baseUrl.EndsWith("/"))
                return $"{baseUrl}{(endpoint.StartsWith("/") ? endpoint.Substring(1) : $"{endpoint}")}";

            return $"{baseUrl}{(endpoint.StartsWith("/") ? endpoint : $"/{endpoint}")}";
        }

        internal static string ToJson(this object data, JsonSerializerSettings serializerSettings)
        {
            return JsonConvert.SerializeObject(data, serializerSettings);
        }

        /// <summary>
        /// Extension method for adding the HTTP API integration to the the WardenConfiguration.
        /// </summary>
        /// <param name="builder">Instance of the Warden configuration builder.</param>
        /// <param name="url">URL of the HTTP API.</param>
        /// <param name="configurator">Optional lambda expression for configuring the HttpApiIntegration.</param>
        public static WardenConfiguration.Builder IntegrateWithHttpApi(
            this WardenConfiguration.Builder builder,
            string url, 
            Action<HttpApiIntegrationConfiguration.Builder> configurator = null)
        {
            builder.AddIntegration(HttpApiIntegration.Create(url, configurator: configurator));

            return builder;
        }

        /// <summary>
        /// Extension method for adding the HTTP API integration to the the WardenConfiguration.
        /// </summary>
        /// <param name="builder">Instance of the Warden configuration builder.</param>
        /// <param name="url">URL of the HTTP API.</param>
        /// <param name="apiKey">API key of the HTTP API passed inside the custom "X-Api-Key" header.</param>
        /// <param name="configurator">Optional lambda expression for configuring the HttpApiIntegration.</param>
        public static WardenConfiguration.Builder IntegrateWithHttpApi(
            this WardenConfiguration.Builder builder,
            string url, string apiKey, 
            Action<HttpApiIntegrationConfiguration.Builder> configurator = null)
        {
            builder.AddIntegration(HttpApiIntegration.Create(url, configurator: configurator));

            return builder;
        }

        /// <summary>
        /// Extension method for adding the HTTP API integration to the the WardenConfiguration.
        /// </summary>
        /// <param name="builder">Instance of the Warden configuration builder.</param>
        /// <param name="url">URL of the HTTP API.</param>
        /// <param name="apiKey">API key of the HTTP API passed inside the custom "X-Api-Key" header.</param>
        /// <param name="headers">Request headers.</param>
        /// <param name="configurator">Optional lambda expression for configuring the HttpApiIntegration.</param>
        public static WardenConfiguration.Builder IntegrateWithHttpApi(
            this WardenConfiguration.Builder builder,
            string url, string apiKey, IDictionary<string, string> headers,
            Action<HttpApiIntegrationConfiguration.Builder> configurator = null)
        {
            builder.AddIntegration(HttpApiIntegration.Create(url, apiKey, headers, configurator));

            return builder;
        }

        /// <summary>
        /// Extension method for adding the HTTP API integration to the the WardenConfiguration.
        /// </summary>
        /// <param name="builder">Instance of the Warden configuration builder.</param>
        /// <param name="configuration">Configuration of HttpApiIntegration.</param>
        /// <returns>Instance of fluent builder for the WardenConfiguration.</returns>
        public static WardenConfiguration.Builder IntegrateWithHttpApi(
            this WardenConfiguration.Builder builder,
            HttpApiIntegrationConfiguration configuration)
        {
            builder.AddIntegration(HttpApiIntegration.Create(configuration));

            return builder;
        }

        /// <summary>
        /// Extension method for resolving the HTTP API integration from the IIntegrator.
        /// </summary>
        /// <param name="integrator">Instance of the IIntegrator.</param>
        /// <returns>Instance of fluent builder for the WardenConfiguration.</returns>
        public static HttpApiIntegration HttpApi(this IIntegrator integrator)
            => integrator.Resolve<HttpApiIntegration>();
    }
}