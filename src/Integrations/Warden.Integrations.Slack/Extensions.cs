using System;
using Warden.Core;

namespace Warden.Integrations.Slack
{
    /// <summary>
    /// Custom extension methods for the Slack integration.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Extension method for adding the Slack integration to the the WardenConfiguration.
        /// </summary>
        /// <param name="builder">Instance of the Warden configuration builder.</param>
        /// <param name="webhookUrl">Full URL of the Slack webhook integration.</param>
        /// <param name="configurator">Optional lambda expression for configuring the SlackIntegration.</param>
        public static WardenConfiguration.Builder IntegrateWithSlack(
            this WardenConfiguration.Builder builder,
            string webhookUrl, 
            Action<SlackIntegrationConfiguration.Builder> configurator = null)
        {
            builder.AddIntegration(SlackIntegration.Create(webhookUrl, configurator));

            return builder;
        }

        /// <summary>
        /// Extension method for adding the Slack integration to the the WardenConfiguration.
        /// </summary>
        /// <param name="builder">Instance of the Warden configuration builder.</param>
        /// <param name="configuration">Configuration of SlackIntegration.</param>
        /// <returns>Instance of fluent builder for the WardenConfiguration.</returns>
        public static WardenConfiguration.Builder IntegrateWithSlack(
            this WardenConfiguration.Builder builder,
            SlackIntegrationConfiguration configuration)
        {
            builder.AddIntegration(SlackIntegration.Create(configuration));

            return builder;
        }

        /// <summary>
        /// Extension method for resolving the Slack integration from the IIntegrator.
        /// </summary>
        /// <param name="integrator">Instance of the IIntegrator.</param>
        /// <returns>Instance of fluent builder for the WardenConfiguration.</returns>
        public static SlackIntegration Slack(this IIntegrator integrator)
            => integrator.Resolve<SlackIntegration>();
    }
}