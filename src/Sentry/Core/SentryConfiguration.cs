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
    }
}