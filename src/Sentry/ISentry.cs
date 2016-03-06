using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sentry.Core;

namespace Sentry
{
    public interface ISentry
    {
        Task<IEnumerable<ISentryOutcome>>  ExecuteAsync();
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

        public async Task<IEnumerable<ISentryOutcome>> ExecuteAsync()
        {
            var outcomes = new List<ISentryOutcome>();
            var tasks = _configuration.Watchers.Select(async watcherConfiguration =>
            {
                var startedAt = DateTime.UtcNow;
                var watcher = watcherConfiguration.Watcher;
                ISentryOutcome sentryOutcome = null;
                try
                {
                    await InvokeOnStartHooksAsync(watcherConfiguration);
                    var watcherOutcome = await watcher.ExecuteAsync();
                    sentryOutcome = SentryOutcome.Valid(watcherOutcome, startedAt, DateTime.UtcNow);
                    outcomes.Add(sentryOutcome);
                    await InvokeOnSuccessHooksAsync(watcherConfiguration, sentryOutcome);
                }
                catch (Exception exception)
                {
                    var sentryException = new SentryException("There was an error while executing Sentry " +
                                                              $"caused by watcher: '{watcher.Name}'", exception);
                    var watcherOutcome = WatcherOutcome.Create(watcher.Name);
                    sentryOutcome = SentryOutcome.Invalid(watcherOutcome, startedAt, DateTime.UtcNow,
                        sentryException);
                    outcomes.Add(sentryOutcome);
                    await InvokeOnFailureHooksAsync(watcherConfiguration, sentryOutcome);
                }
                finally
                {
                    await InvokeOnCompletedHooksAsync(watcherConfiguration, sentryOutcome);
                }

            });
            await Task.WhenAll(tasks);

            return outcomes;
        }

        private async Task InvokeOnStartHooksAsync(WatcherConfiguration watcherConfiguration)
        {
            watcherConfiguration.Hooks.OnStart.Invoke();
            await watcherConfiguration.Hooks.OnStartAsync();
            _configuration.Hooks.OnStart.Invoke();
            await _configuration.Hooks.OnStartAsync();
        }

        private async Task InvokeOnSuccessHooksAsync(WatcherConfiguration watcherConfiguration, ISentryOutcome outcome)
        {
            watcherConfiguration.Hooks.OnSuccess.Invoke(outcome);
            await watcherConfiguration.Hooks.OnSuccessAsync(outcome);
            _configuration.Hooks.OnSuccess.Invoke(outcome);
            await _configuration.Hooks.OnSuccessAsync(outcome);
        }

        private async Task InvokeOnFailureHooksAsync(WatcherConfiguration watcherConfiguration, ISentryOutcome outcome)
        {
            watcherConfiguration.Hooks.OnFailure.Invoke(outcome);
            await watcherConfiguration.Hooks.OnFailureAsync(outcome);
            _configuration.Hooks.OnFailure.Invoke(outcome);
            await _configuration.Hooks.OnFailureAsync(outcome);
        }

        private async Task InvokeOnCompletedHooksAsync(WatcherConfiguration watcherConfiguration, ISentryOutcome outcome)
        {
            watcherConfiguration.Hooks.OnCompleted.Invoke(outcome);
            await watcherConfiguration.Hooks.OnCompletedAsync(outcome);
            _configuration.Hooks.OnCompleted.Invoke(outcome);
            await _configuration.Hooks.OnCompletedAsync(outcome);
        }
    }
}