using System;
using Warden.Configurations;

namespace Warden.Integrations.Twilio
{
    public static class Extensions
    {
        /// <summary>
        /// Extension method for adding the Twilio integration to the the WardenConfiguration.
        /// </summary>
        /// <param name="builder">Instance of the Warden configuration builder.</param>
        /// <param name="accountSid">SID of the Twilio account.</param>
        /// <param name="authToken">Authentication token of the Twilio account.</param>
        /// <param name="sender">Phone number of the SMS sender.</param>
        /// <param name="configurator">Optional lambda expression for configuring the TwilioIntegration.</param>
        /// <returns>Instance of fluent builder for the WardenConfiguration.</returns>
        public static WardenConfiguration.Builder IntegrateWithTwilio(
            this WardenConfiguration.Builder builder,
            string accountSid, string authToken, string sender,
            Action<TwilioIntegrationConfiguration.Builder> configurator = null)
        {
            builder.AddIntegration(TwilioIntegration.Create(accountSid, authToken, sender, configurator));

            return builder;
        }

        /// <summary>
        /// Extension method for adding the Twilio integration to the the WardenConfiguration.
        /// </summary>
        /// <param name="builder">Instance of the Warden configuration builder.</param>
        /// <param name="configuration">Configuration of TwilioIntegration.</param>
        /// <returns>Instance of fluent builder for the WardenConfiguration.</returns>
        public static WardenConfiguration.Builder IntegrateWithTwilio(
            this WardenConfiguration.Builder builder,
            TwilioIntegrationConfiguration configuration)
        {
            builder.AddIntegration(TwilioIntegration.Create(configuration));

            return builder;
        }

        /// <summary>
        /// Extension method for resolving the Twilio integration from the IIntegrator.
        /// </summary>
        /// <param name="integrator">Instance of the IIntegrator.</param>
        /// <returns>Instance of fluent builder for the WardenConfiguration.</returns>
        public static TwilioIntegration Twilio(this IIntegrator integrator)
            => integrator.Resolve<TwilioIntegration>();
    }
}