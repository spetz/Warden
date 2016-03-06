using System;
using System.Linq;
using System.Threading.Tasks;
using Sentry.Core;

namespace Sentry
{
    public interface ISentry
    {
        Task ExecuteAsync();
    }

    public class Sentry : ISentry
    {
        private readonly SentryConfiguration _configuration;

        public Sentry(SentryConfiguration configuration)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration), "Sentry configuration has not been provided.");

            _configuration = configuration;
        }

        public async Task ExecuteAsync()
        {
            var tasks = _configuration.Watchers.Select(x => x.Watcher.ExecuteAsync());
            try
            {
                await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                throw new SentryException("There was an error while executing Sentry.", ex);
            }

            var onCompletedHooks = _configuration.Watchers
                .Select(x => x.Hooks.OnCompleted)
                .Where(x => x != null);

            foreach (var hook in onCompletedHooks)
            {
                hook();
            }

            var onCompletedHooksAsync = _configuration.Watchers
                .Select(x => x.Hooks.OnCompletedAsync)
                .Where(x => x != null)
                .Select(task => task());

            await Task.WhenAll(onCompletedHooksAsync);
        }
    }
}