using System;
using Sentry.Core;

namespace Sentry.Integrations.SendGrid
{
    public static class Extensions
    {
        public static SentryConfiguration.Builder IntegrateWithSendGrid(
            this SentryConfiguration.Builder builder, string sender,
            Action<SendGridIntegrationConfiguration.Builder> configurator)
        {
            builder.AddIntegration(SendGridIntegration.Create(sender, configurator));

            return builder;
        }

        public static SentryConfiguration.Builder IntegrateWithSendGrid(
            this SentryConfiguration.Builder builder,
            SendGridIntegrationConfiguration configuration)
        {
            builder.AddIntegration(SendGridIntegration.Create(configuration));

            return builder;
        }

        public static SendGridIntegration SendGrid(this IIntegrator provider)
            => provider.Resolve<SendGridIntegration>();

        public static string Or(this string value, string target)
            => string.IsNullOrWhiteSpace(value) ? target : value;
    }
}