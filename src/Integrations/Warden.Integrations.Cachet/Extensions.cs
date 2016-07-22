using System;
using Newtonsoft.Json;
using Warden.Core;

namespace Warden.Integrations.Cachet
{
    /// <summary>
    /// Custom extension methods for the Cachet (https://cachethq.io) integration.
    /// </summary>
    public static class Extensions
    {
        internal static string ToJson(this object data, JsonSerializerSettings serializerSettings)
        {
            return JsonConvert.SerializeObject(data, serializerSettings);
        }

        /// <summary>
        /// Extension method for adding the Cachet integration to the the WardenConfiguration.
        /// </summary>
        /// <param name="builder">Instance of the Warden configuration builder.</param>
        /// <param name="apiUrl">URL of the Cachet API.</param>
        /// <param name="accessToken">Access token of the Cachet account.</param>
        /// <param name="configurator">Optional lambda expression for configuring the CachetIntegration.</param>
        /// <returns>Instance of fluent builder for the WardenConfiguration.</returns>
        public static WardenConfiguration.Builder IntegrateWithCachet(
            this WardenConfiguration.Builder builder,
            string apiUrl, string accessToken,
            Action<CachetIntegrationConfiguration.Builder> configurator = null)
        {
            builder.AddIntegration(CachetIntegration.Create(apiUrl, accessToken, configurator));

            return builder;
        }

        /// <summary>
        /// Extension method for adding the Cachet integration to the the WardenConfiguration.
        /// </summary>
        /// <param name="builder">Instance of the Warden configuration builder.</param>
        /// <param name="apiUrl">URL of the Cachet API.</param>
        /// <param name="username">Username of the Cachet account.</param>
        /// <param name="password">Password of the Cachet account.</param>
        /// <param name="configurator">Optional lambda expression for configuring the CachetIntegration.</param>
        /// <returns>Instance of fluent builder for the WardenConfiguration.</returns>
        public static WardenConfiguration.Builder IntegrateWithCachet(
            this WardenConfiguration.Builder builder,
            string apiUrl, string username, string password,
            Action<CachetIntegrationConfiguration.Builder> configurator = null)
        {
            builder.AddIntegration(CachetIntegration.Create(apiUrl, username, password, configurator));

            return builder;
        }

        /// <summary>
        /// Extension method for adding the Cachet integration to the the WardenConfiguration.
        /// </summary>
        /// <param name="builder">Instance of the Warden configuration builder.</param>
        /// <param name="configuration">Configuration of CachetIntegration.</param>
        /// <returns>Instance of fluent builder for the WardenConfiguration.</returns>
        public static WardenConfiguration.Builder IntegrateWithCachet(
            this WardenConfiguration.Builder builder,
            CachetIntegrationConfiguration configuration)
        {
            builder.AddIntegration(CachetIntegration.Create(configuration));

            return builder;
        }

        /// <summary>
        /// Extension method for resolving the Cachet integration from the IIntegrator.
        /// </summary>
        /// <param name="integrator">Instance of the IIntegrator.</param>
        /// <returns>Instance of fluent builder for the WardenConfiguration.</returns>
        public static CachetIntegration Cachet(this IIntegrator integrator)
            => integrator.Resolve<CachetIntegration>();
    }
}