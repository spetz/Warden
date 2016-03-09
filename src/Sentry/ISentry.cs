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
        private enum WatcherResultState
        {
            NotSet = 0,
            Success = 1,
            Failure = 2,
            Error = 3
        }

        private readonly SentryConfiguration _configuration;
        private long _iterationOrdinal = 1;
        private bool _started = false;
        private readonly Dictionary<IWatcher, WatcherResultState> _latestWatcherResultStates = new Dictionary<IWatcher, WatcherResultState>();

        public Sentry(SentryConfiguration configuration)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration), "Sentry configuration has not been provided.");

            _configuration = configuration;
        }

        public async Task StartAsync()
        {
            _started = true;
            _configuration.Hooks.OnStart.Execute();
            await _configuration.Hooks.OnStartAsync.ExecuteAsync();

            try
            {
                while (CanExecuteIteration(_iterationOrdinal))
                {
                    _configuration.Hooks.OnIterationStart.Execute(_iterationOrdinal);
                    await _configuration.Hooks.OnIterationStartAsync.ExecuteAsync(_iterationOrdinal);
                    var iteration = await ExecuteIterationAsync(_iterationOrdinal);
                    _configuration.Hooks.OnIterationCompleted.Execute(iteration);
                    await _configuration.Hooks.OnIterationCompletedAsync.ExecuteAsync(iteration);
                    var canExecuteNextIteration = CanExecuteIteration(_iterationOrdinal + 1);
                    if (!canExecuteNextIteration)
                        break;

                    await Task.Delay(_configuration.IterationDelay);
                    _iterationOrdinal++;
                }
            }
            catch (Exception exception)
            {
                _configuration.Hooks.OnError.Execute(exception);
                await _configuration.Hooks.OnErrorAsync.ExecuteAsync(exception);
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
            _configuration.Hooks.OnStop.Execute();
            await _configuration.Hooks.OnStopAsync.ExecuteAsync();
        }

        private async Task<ISentryIteration> ExecuteIterationAsync(long ordinal)
        {
            var iterationStartedAt = _configuration.DateTimeProvider();
            var results = new List<ISentryCheckResult>();
            var tasks = _configuration.Watchers.Select(async watcherConfiguration =>
            {
                var startedAt = _configuration.DateTimeProvider();
                var watcher = watcherConfiguration.Watcher;
                ISentryCheckResult sentryCheckResult = null;
                try
                {
                    await InvokeOnStartHooksAsync(watcherConfiguration, WatcherCheck.Create(watcher));
                    var watcherCheckResult = await watcher.ExecuteAsync();
                    var completedAt = _configuration.DateTimeProvider();
                    sentryCheckResult = SentryCheckResult.Create(watcherCheckResult, startedAt, completedAt);
                    results.Add(sentryCheckResult);
                    if (watcherCheckResult.IsValid)
                    {
                        await UpdateWatcherResultStateAndExecuteHooksPossibleAsync(watcher, WatcherResultState.Success,
                            () => InvokeOnFirstSuccessHooksAsync(watcherConfiguration, sentryCheckResult),
                            executeIfLatestStateIsNotSet: false);
                        await InvokeOnSuccessHooksAsync(watcherConfiguration, sentryCheckResult);
                    }
                    else
                    {
                        await UpdateWatcherResultStateAndExecuteHooksPossibleAsync(watcher, WatcherResultState.Failure,
                            () => InvokeOnFirstFailureHooksAsync(watcherConfiguration, sentryCheckResult));
                        await InvokeOnFailureHooksAsync(watcherConfiguration, sentryCheckResult);
                    }
                }
                catch (Exception exception)
                {
                    await UpdateWatcherResultStateAndExecuteHooksPossibleAsync(watcher, WatcherResultState.Error,
                        () => InvokeOnFirstErrorHooksAsync(watcherConfiguration, exception));
                    var sentryException = new SentryException("There was an error while executing Sentry " +
                                                              $"caused by watcher: '{watcher.Name}'.", exception);

                    await InvokeOnErrorHooksAsync(watcherConfiguration, sentryException);
                }
                finally
                {
                    await InvokeOnCompletedHooksAsync(watcherConfiguration, sentryCheckResult);
                }
            });

            await Task.WhenAll(tasks);
            var iterationCompletedAt = _configuration.DateTimeProvider();
            var iteration = SentryIteration.Create(ordinal, results, iterationStartedAt, iterationCompletedAt);

            return iteration;
        }

        private async Task UpdateWatcherResultStateAndExecuteHooksPossibleAsync(IWatcher watcher, WatcherResultState state, 
            Func<Task> hooks, bool executeIfLatestStateIsNotSet = true)
        {
            if (_latestWatcherResultStates.ContainsKey(watcher))
            {
                var latestState = _latestWatcherResultStates[watcher];
                if (latestState == state)
                    return;

                _latestWatcherResultStates[watcher] = state;
                await hooks();
            }


            if (!executeIfLatestStateIsNotSet)
                return;

            _latestWatcherResultStates[watcher] = state;
            await hooks();
        }

        private async Task InvokeOnStartHooksAsync(WatcherConfiguration watcherConfiguration, IWatcherCheck check)
        {
            watcherConfiguration.Hooks.OnStart.Execute(check);
            await watcherConfiguration.Hooks.OnStartAsync.ExecuteAsync(check);
            _configuration.GlobalWatcherHooks.OnStart.Execute(check);
            await _configuration.GlobalWatcherHooks.OnStartAsync.ExecuteAsync(check);
        }

        private async Task InvokeOnSuccessHooksAsync(WatcherConfiguration watcherConfiguration, ISentryCheckResult checkResult)
        {
            watcherConfiguration.Hooks.OnSuccess.Execute(checkResult);
            await watcherConfiguration.Hooks.OnSuccessAsync.ExecuteAsync(checkResult);
            _configuration.GlobalWatcherHooks.OnSuccess.Execute(checkResult);
            await _configuration.GlobalWatcherHooks.OnSuccessAsync.ExecuteAsync(checkResult);
        }

        private async Task InvokeOnFirstSuccessHooksAsync(WatcherConfiguration watcherConfiguration, ISentryCheckResult checkResult)
        {
            watcherConfiguration.Hooks.OnFirstSuccess.Execute(checkResult);
            await watcherConfiguration.Hooks.OnFirstSuccessAsync.ExecuteAsync(checkResult);
            _configuration.GlobalWatcherHooks.OnFirstSuccess.Execute(checkResult);
            await _configuration.GlobalWatcherHooks.OnFirstSuccessAsync.ExecuteAsync(checkResult);
        }

        private async Task InvokeOnFailureHooksAsync(WatcherConfiguration watcherConfiguration, ISentryCheckResult checkResult)
        {
            watcherConfiguration.Hooks.OnFailure.Execute(checkResult);
            await watcherConfiguration.Hooks.OnFailureAsync.ExecuteAsync(checkResult);
            _configuration.GlobalWatcherHooks.OnFailure.Execute(checkResult);
            await _configuration.GlobalWatcherHooks.OnFailureAsync.ExecuteAsync(checkResult);
        }

        private async Task InvokeOnFirstFailureHooksAsync(WatcherConfiguration watcherConfiguration, ISentryCheckResult checkResult)
        {
            watcherConfiguration.Hooks.OnFirstFailure.Execute(checkResult);
            await watcherConfiguration.Hooks.OnFirstFailureAsync.ExecuteAsync(checkResult);
            _configuration.GlobalWatcherHooks.OnFirstFailure.Execute(checkResult);
            await _configuration.GlobalWatcherHooks.OnFirstFailureAsync.ExecuteAsync(checkResult);
        }

        private async Task InvokeOnCompletedHooksAsync(WatcherConfiguration watcherConfiguration, ISentryCheckResult checkResult)
        {
            watcherConfiguration.Hooks.OnCompleted.Execute(checkResult);
            await watcherConfiguration.Hooks.OnCompletedAsync.ExecuteAsync(checkResult);
            _configuration.GlobalWatcherHooks.OnCompleted.Execute(checkResult);
            await _configuration.GlobalWatcherHooks.OnCompletedAsync.ExecuteAsync(checkResult);
        }

        private async Task InvokeOnErrorHooksAsync(WatcherConfiguration watcherConfiguration, Exception exception)
        {
            watcherConfiguration.Hooks.OnError.Execute(exception);
            await watcherConfiguration.Hooks.OnErrorAsync.ExecuteAsync(exception);
            _configuration.GlobalWatcherHooks.OnError.Execute(exception);
            await _configuration.GlobalWatcherHooks.OnErrorAsync.ExecuteAsync(exception);
        }

        private async Task InvokeOnFirstErrorHooksAsync(WatcherConfiguration watcherConfiguration, Exception exception)
        {
            watcherConfiguration.Hooks.OnFirstError.Execute(exception);
            await watcherConfiguration.Hooks.OnFirstErrorAsync.ExecuteAsync(exception);
            _configuration.GlobalWatcherHooks.OnFirstError.Execute(exception);
            await _configuration.GlobalWatcherHooks.OnFirstErrorAsync.ExecuteAsync(exception);
        }
    }
}