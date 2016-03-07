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
            _configuration.Hooks.OnStart.ToList().ForEach(x => x());
            await Task.WhenAll(_configuration.Hooks.OnStartAsync.ToList().Select(x => x()));

            try
            {
                while (CanExecuteIteration(_iterationOrdinal))
                {
                    _configuration.Hooks.OnIterationStart.ToList().ForEach(x => x(_iterationOrdinal));
                    await Task.WhenAll(_configuration.Hooks.OnIterationStartAsync.ToList().Select(x => x(_iterationOrdinal)));
                    var iteration = await ExecuteIterationAsync(_iterationOrdinal);
                    _configuration.Hooks.OnIterationCompleted.ToList().ForEach(x => x(iteration));
                    await Task.WhenAll(_configuration.Hooks.OnIterationCompletedAsync.ToList().Select(x => x(iteration)));
                    await Task.Delay(_configuration.IterationDelay);
                    _iterationOrdinal++;
                }
            }
            catch (Exception exception)
            {
                _configuration.Hooks.OnError.ToList().ForEach(x => x(exception));
                await Task.WhenAll(_configuration.Hooks.OnErrorAsync.ToList().Select(x => x(exception)));
            }
        }

        private bool CanExecuteIteration(long ordinal)
        {
            if (!_started)
                return false;
            if (!_configuration.IterationsCount.HasValue)
                return true;
            if (ordinal <= _configuration.IterationsCount)
                return true;

            return false;
        }

        public async Task StopAsync()
        {
            _started = false;
            _configuration.Hooks.OnStop.ToList().ForEach(x => x());
            await Task.WhenAll(_configuration.Hooks.OnStopAsync.ToList().Select(x => x()));
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
            watcherConfiguration.Hooks.OnStart.ToList().ForEach(x => x(check));
            await Task.WhenAll(watcherConfiguration.Hooks.OnStartAsync.ToList().Select(x => x(check)));
            _configuration.GlobalWatcherHooks.OnStart.ToList().ForEach(x => x(check));
            await Task.WhenAll(_configuration.GlobalWatcherHooks.OnStartAsync.ToList().Select(x => x(check)));
        }

        private async Task InvokeOnSuccessHooksAsync(WatcherConfiguration watcherConfiguration, ISentryCheckResult checkResult)
        {
            watcherConfiguration.Hooks.OnSuccess.ToList().ForEach(x => x(checkResult));
            await Task.WhenAll(watcherConfiguration.Hooks.OnSuccessAsync.ToList().Select(x => x(checkResult)));
            _configuration.GlobalWatcherHooks.OnSuccess.ToList().ForEach(x => x(checkResult));
            await Task.WhenAll(_configuration.GlobalWatcherHooks.OnSuccessAsync.ToList().Select(x => x(checkResult)));
        }

        private async Task InvokeOnFailureHooksAsync(WatcherConfiguration watcherConfiguration, ISentryCheckResult checkResult)
        {
            watcherConfiguration.Hooks.OnFailure.ToList().ForEach(x => x(checkResult));
            await Task.WhenAll(watcherConfiguration.Hooks.OnFailureAsync.ToList().Select(x => x(checkResult)));
            _configuration.GlobalWatcherHooks.OnFailure.ToList().ForEach(x => x(checkResult));
            await Task.WhenAll(_configuration.GlobalWatcherHooks.OnFailureAsync.ToList().Select(x => x(checkResult)));
        }

        private async Task InvokeOnCompletedHooksAsync(WatcherConfiguration watcherConfiguration, ISentryCheckResult checkResult)
        {
            watcherConfiguration.Hooks.OnCompleted.ToList().ForEach(x => x(checkResult));
            await Task.WhenAll(watcherConfiguration.Hooks.OnCompletedAsync.ToList().Select(x => x(checkResult)));
            _configuration.GlobalWatcherHooks.OnCompleted.ToList().ForEach(x => x(checkResult));
            await Task.WhenAll(_configuration.GlobalWatcherHooks.OnCompletedAsync.ToList().Select(x => x(checkResult)));
        }
    }
}