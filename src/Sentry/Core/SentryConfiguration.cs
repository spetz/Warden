using System.Collections.Generic;

namespace Sentry.Core
{
    public class SentryConfiguration
    {
        public IEnumerable<IWatcher> Watchers { get; } 

        public static SentryConfigurationBuilder Configure() => new SentryConfigurationBuilder();
    }
}