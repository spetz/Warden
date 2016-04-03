using Sentry.Core;

namespace Sentry.Integrations.SendGrid
{
    public static class Extensions
    {
        public static SentryConfiguration.Builder AddSendGridIntegration(this SentryConfiguration.Builder builder,
            SendGridIntegrationConfiguration configuration)
        {
            return builder;
        }

        public static SendGridIntegration SendGrid(this IIntegrator provider)
            => provider.Resolve<SendGridIntegration>();

        public static string Or(this string value, string target) => string.IsNullOrWhiteSpace(value) ? target : value;
    }
}