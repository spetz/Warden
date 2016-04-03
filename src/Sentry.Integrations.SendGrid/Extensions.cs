using System;
using Sentry.Core;

namespace Sentry.Integrations.SendGrid
{
    public static class Extensions
    {
        /// <summary>
        /// Extension method for adding the SendGrid integration to the the SentryConfiguration.
        /// </summary>
        /// <param name="builder">Instance of the Sentry configuration builder.</param>
        /// <param name="sender">Email address of the message sender.</param>
        /// <param name="configurator">Lambda expression for configuring the SendGridIntegration.</param>
        /// <returns>Instance of fluent builder for the SentryConfiguration.</returns>
        public static SentryConfiguration.Builder IntegrateWithSendGrid(
            this SentryConfiguration.Builder builder, string sender,
            Action<SendGridIntegrationConfiguration.Builder> configurator)
        {
            builder.AddIntegration(SendGridIntegration.Create(sender, configurator));

            return builder;
        }

        /// <summary>
        /// Extension method for adding the SendGrid integration to the the SentryConfiguration.
        /// </summary>
        /// <param name="builder">Instance of the Sentry configuration builder.</param>
        /// <param name="configuration">Configuration of SendGridIntegration.</param>
        /// <returns>Instance of fluent builder for the SentryConfiguration.</returns>
        public static SentryConfiguration.Builder IntegrateWithSendGrid(
            this SentryConfiguration.Builder builder,
            SendGridIntegrationConfiguration configuration)
        {
            builder.AddIntegration(SendGridIntegration.Create(configuration));

            return builder;
        }

        /// <summary>
        /// Extension method for resolving the SendGrid integration from the IIntegrator.
        /// </summary>
        /// <param name="integrator">Instance of the IIntegrator.</param>
        /// <returns>Instance of fluent builder for the SentryConfiguration.</returns>
        public static SendGridIntegration SendGrid(this IIntegrator integrator)
            => integrator.Resolve<SendGridIntegration>();

        internal static string Or(this string value, string target)
            => string.IsNullOrWhiteSpace(value) ? target : value;
    }
}