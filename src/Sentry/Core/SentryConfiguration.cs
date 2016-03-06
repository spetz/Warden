using System.Collections.Generic;

namespace Sentry.Core
{
    public class SentryConfiguration
    {
        public IEnumerable<IWatcher> Watchers { get; } = new List<IWatcher>();

        protected internal SentryConfiguration()
        {
        }

        public static SentryConfiguration Empty => new SentryConfiguration();
        public static SentryConfigurationBuilder Configure() => new SentryConfigurationBuilder();

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
}