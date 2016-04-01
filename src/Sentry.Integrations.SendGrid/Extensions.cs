namespace Sentry.Integrations.SendGrid
{
    public static class Extensions
    {
        public static SendGridIntegration SendGrid(this IIntegrator provider,
            SendGridIntegrationConfiguration configuration)
            => new SendGridIntegration(configuration);

        public static string Or(this string value, string target) => string.IsNullOrWhiteSpace(value) ? target : value;
    }
}