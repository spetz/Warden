using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sentry.Core;

namespace Sentry
{
    public interface ISentry
    {
        Task<IEnumerable<ISentryCheckResult>>  ExecuteAsync();
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

        public async Task<IEnumerable<ISentryCheckResult>> ExecuteAsync()
        {
            var results = new List<ISentryCheckResult>();
            var tasks = _configuration.Watchers.Select(async watcherConfiguration =>
            {
                var startedAt = DateTime.UtcNow;
                var watcher = watcherConfiguration.Watcher;
                ISentryCheckResult sentryCheckResult = null;
                try
                {
                    await InvokeOnStartHooksAsync(watcherConfiguration, WatcherCheck.Create(watcher));
                    var watcherCheckResult = await watcher.ExecuteAsync();
                    sentryCheckResult = SentryCheckResult.Valid(watcherCheckResult, startedAt, DateTime.UtcNow);
                    results.Add(sentryCheckResult);
                    await InvokeOnSuccessHooksAsync(watcherConfiguration, sentryCheckResult);
                }
                catch (Exception exception)
                {
                    var sentryException = new SentryException("There was an error while executing Sentry " +
                                                              $"caused by watcher: '{watcher.Name}'.", exception);
                    var watcherCheckResult = WatcherCheckResult.Create(watcher);
                    sentryCheckResult = SentryCheckResult.Invalid(watcherCheckResult, startedAt, DateTime.UtcNow,
                        sentryException);
                    results.Add(sentryCheckResult);
                    await InvokeOnFailureHooksAsync(watcherConfiguration, sentryCheckResult);
                }
                finally
                {
                    await InvokeOnCompletedHooksAsync(watcherConfiguration, sentryCheckResult);
                }

            });
            await Task.WhenAll(tasks);

            return results;
        }

        private async Task InvokeOnStartHooksAsync(WatcherConfiguration watcherConfiguration, IWatcherCheck check)
        {
            watcherConfiguration.Hooks.OnStart.Invoke(check);
            await watcherConfiguration.Hooks.OnStartAsync(check);
            _configuration.Hooks.OnStart.Invoke(check);
            await _configuration.Hooks.OnStartAsync(check);
        }

        private async Task InvokeOnSuccessHooksAsync(WatcherConfiguration watcherConfiguration, ISentryCheckResult checkResult)
        {
            watcherConfiguration.Hooks.OnSuccess.Invoke(checkResult);
            await watcherConfiguration.Hooks.OnSuccessAsync(checkResult);
            _configuration.Hooks.OnSuccess.Invoke(checkResult);
            await _configuration.Hooks.OnSuccessAsync(checkResult);
        }

        private async Task InvokeOnFailureHooksAsync(WatcherConfiguration watcherConfiguration, ISentryCheckResult checkResult)
        {
            watcherConfiguration.Hooks.OnFailure.Invoke(checkResult);
            await watcherConfiguration.Hooks.OnFailureAsync(checkResult);
            _configuration.Hooks.OnFailure.Invoke(checkResult);
            await _configuration.Hooks.OnFailureAsync(checkResult);
        }

        private async Task InvokeOnCompletedHooksAsync(WatcherConfiguration watcherConfiguration, ISentryCheckResult checkResult)
        {
            watcherConfiguration.Hooks.OnCompleted.Invoke(checkResult);
            await watcherConfiguration.Hooks.OnCompletedAsync(checkResult);
            _configuration.Hooks.OnCompleted.Invoke(checkResult);
            await _configuration.Hooks.OnCompletedAsync(checkResult);
        }
    }
}