namespace Sentry.Core
{
    public class SentryConfigurationBuilder
    {
        private readonly SentryConfigurationFluent _configuration = new SentryConfigurationFluent();

        public SentryConfigurationFluent Setup() => _configuration;

        public class SentryConfigurationFluent
        {
            public SentryConfiguration Build() => new SentryConfiguration();
        }
    }
}