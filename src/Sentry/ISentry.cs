using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sentry.Core;

namespace Sentry
{
    public interface ISentry
    {
        Task StartAsync();
        Task StopAsync();
    }

    public class Sentry : ISentry
    {
        private readonly SentryConfiguration _configuration;
        private long _iterationOrdinal = 1;
        private bool _started = false;

        public Sentry(SentryConfiguration configuration)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration), "Sentry configuration has not been provided.");

            _configuration = configuration;
        }

        public async Task StartAsync()
        {
            _started = true;
            _configuration.Hooks.OnStart();
            await _configuration.Hooks.OnStartAsync();

            try
            {
                while (CanExecuteIteration(_iterationOrdinal))
                {
                    _configuration.Hooks.OnIterationStart(_iterationOrdinal);
                    await _configuration.Hooks.OnIterationStartAsync(_iterationOrdinal);
                    var iteration = await ExecuteIterationAsync(_iterationOrdinal);
                    _configuration.Hooks.OnIterationCompleted(iteration);
                    await _configuration.Hooks.OnIterationCompletedAsync(iteration);
                    await Task.Delay(_configuration.IterationDelay);
                    _iterationOrdinal++;
                }
            }
            catch (Exception exception)
            {
                _configuration.Hooks.OnError(exception);
                await _configuration.Hooks.OnErrorAsync(exception);
            }
        }

        private bool CanExecuteIteration(long ordinal)
        {
            if (!_started)
                return false;
            if (!_configuration.TotalNumberOfIterations.HasValue)
                return true;
            if (ordinal <= _configuration.TotalNumberOfIterations)
                return true;

            return false;
        }

        public async Task StopAsync()
        {
            _started = false;
            _configuration.Hooks.OnStop();
            await _configuration.Hooks.OnStopAsync();
        }

        private async Task<ISentryIteration> ExecuteIterationAsync(long ordinal)
        {
            var iterationStartedAtUtc = DateTime.UtcNow;
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
            var iterationCompleteddAtUtc = DateTime.UtcNow;
            var iteration = SentryIteration.Create(ordinal, results, iterationStartedAtUtc, iterationCompleteddAtUtc);

            return iteration;
        }

        private async Task InvokeOnStartHooksAsync(WatcherConfiguration watcherConfiguration, IWatcherCheck check)
        {
            watcherConfiguration.Hooks.OnStart.Invoke(check);
            await watcherConfiguration.Hooks.OnStartAsync(check);
            _configuration.GlobalWatcherHooks.OnStart.Invoke(check);
            await _configuration.GlobalWatcherHooks.OnStartAsync(check);
        }

        private async Task InvokeOnSuccessHooksAsync(WatcherConfiguration watcherConfiguration, ISentryCheckResult checkResult)
        {
            watcherConfiguration.Hooks.OnSuccess.Invoke(checkResult);
            await watcherConfiguration.Hooks.OnSuccessAsync(checkResult);
            _configuration.GlobalWatcherHooks.OnSuccess.Invoke(checkResult);
            await _configuration.GlobalWatcherHooks.OnSuccessAsync(checkResult);
        }

        private async Task InvokeOnFailureHooksAsync(WatcherConfiguration watcherConfiguration, ISentryCheckResult checkResult)
        {
            watcherConfiguration.Hooks.OnFailure.Invoke(checkResult);
            await watcherConfiguration.Hooks.OnFailureAsync(checkResult);
            _configuration.GlobalWatcherHooks.OnFailure.Invoke(checkResult);
            await _configuration.GlobalWatcherHooks.OnFailureAsync(checkResult);
        }

        private async Task InvokeOnCompletedHooksAsync(WatcherConfiguration watcherConfiguration, ISentryCheckResult checkResult)
        {
            watcherConfiguration.Hooks.OnCompleted.Invoke(checkResult);
            await watcherConfiguration.Hooks.OnCompletedAsync(checkResult);
            _configuration.GlobalWatcherHooks.OnCompleted.Invoke(checkResult);
            await _configuration.GlobalWatcherHooks.OnCompletedAsync(checkResult);
        }
    }
}