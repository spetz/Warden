using System;

namespace Warden.Integrations.Cachet
{
    /// <summary>
    /// Integration with the Cachet (https://cachethq.io) - an open source status page system.
    /// </summary>
    public class CachetIntegration : IIntegration
    {
        private readonly CachetIntegrationConfiguration _configuration;

        public CachetIntegration(CachetIntegrationConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration),
                    "Cachet Integration configuration has not been provided.");
            }

            _configuration = configuration;
        }

        /// <summary>
        /// Factory method for creating a new instance of CachetIntegration.
        /// </summary>
        /// <param name="apiUrl">URL of the Cachet API.</param>
        /// <param name="accessToken">Access token of the Cachet account.</param>
        /// <param name="configurator">Lambda expression for configuring the Cachet integration.</param>
        /// <returns>Instance of CachetIntegration.</returns>
        public static CachetIntegration Create(string apiUrl, string accessToken,
            Action<CachetIntegrationConfiguration.Builder> configurator = null)
        {
            var config = new CachetIntegrationConfiguration.Builder(apiUrl, accessToken);
            configurator?.Invoke(config);

            return Create(config.Build());
        }

        /// <summary>
        /// Factory method for creating a new instance of CachetIntegration.
        /// </summary>
        /// <param name="apiUrl">URL of the Cachet API.</param>
        /// <param name="username">Username of the Cachet account.</param>
        /// <param name="password">Password of the Cachet account.</param>
        /// <param name="configurator">Lambda expression for configuring the Cachet integration.</param>
        /// <returns>Instance of CachetIntegration.</returns>
        public static CachetIntegration Create(string apiUrl, string username, string password, 
            Action<CachetIntegrationConfiguration.Builder> configurator)
        {
            var config = new CachetIntegrationConfiguration.Builder(apiUrl, username, password);
            configurator?.Invoke(config);

            return Create(config.Build());
        }

        /// <summary>
        /// Factory method for creating a new instance of CachetIntegration.
        /// </summary>
        /// <param name="configuration">Configuration of Cachet integration.</param>
        /// <returns>Instance of CachetIntegration.</returns>
        public static CachetIntegration Create(CachetIntegrationConfiguration configuration)
            => new CachetIntegration(configuration);
    }
}