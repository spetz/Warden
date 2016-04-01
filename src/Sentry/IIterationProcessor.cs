using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sentry.Core;

namespace Sentry
{
    public interface IIterationProcessor
    {
        /// <summary>
        /// Run a single iteration (cycle) that will execute all of the watchers and theirs hooks.
        /// </summary>
        /// <param name="ordinal">Number (ordinal) of executed iteration</param>
        /// <returns>Single iteration containing its ordinal and results of all executed watcher checks</returns>
        Task<ISentryIteration> ExecuteAsync(long ordinal);
    }

    public class IterationProcessor : IIterationProcessor
    {
        private readonly IterationProcessorConfiguration _configuration;

        private readonly Dictionary<IWatcher, WatcherResultState> _latestWatcherResultStates =
            new Dictionary<IWatcher, WatcherResultState>();

        private enum WatcherResultState
        {
            NotSet = 0,
            Success = 1,
            Failure = 2,
            Error = 3
        }

        /// <summary>
        /// Initialize a new instance of the IterationProcessor using the provided configuration.
        /// </summary>
        /// <param name="configuration">Configuration of IterationProcessor</param>
        public IterationProcessor(IterationProcessorConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Run a single iteration (cycle) that will execute all of the watchers and theirs hooks.
        /// </summary>
        /// <param name="ordinal">Number (ordinal) of executed iteration</param>
        /// <returns>Single iteration containing its ordinal and results of all executed watcher checks</returns>
        public async Task<ISentryIteration> ExecuteAsync(long ordinal)
        {
            var iterationStartedAt = _configuration.DateTimeProvider();
            var results = new Dictionary<WatcherConfiguration, ISentryCheckResult>();
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
                    results.Add(watcherConfiguration, sentryCheckResult);
                    if (watcherCheckResult.IsValid)
                    {
                        var result = sentryCheckResult;
                        await UpdateWatcherResultStateAndExecuteHooksPossibleAsync(watcher, WatcherResultState.Success,
                            () => InvokeOnFirstSuccessHooksAsync(watcherConfiguration, result),
                            executeIfLatestStateIsNotSet: false);
                        await InvokeOnSuccessHooksAsync(watcherConfiguration, sentryCheckResult);
                    }
                    else
                    {
                        var result = sentryCheckResult;
                        await UpdateWatcherResultStateAndExecuteHooksPossibleAsync(watcher, WatcherResultState.Failure,
                            () => InvokeOnFirstFailureHooksAsync(watcherConfiguration, result));
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
                    if (sentryCheckResult == null)
                    {
                        sentryCheckResult = SentryCheckResult.Create(WatcherCheckResult.Create(watcher, false),
                            startedAt, _configuration.DateTimeProvider());
                        results.Add(watcherConfiguration, sentryCheckResult);
                    }

                    await InvokeOnCompletedHooksAsync(watcherConfiguration, sentryCheckResult);
                }
            });

            await Task.WhenAll(tasks);
            try
            {
                await InvokeAggregatedOnFailureHooksAsync(results.Select(x => x.Value));
            }
            catch (Exception exception)
            {
                
            }

            var iterationCompletedAt = _configuration.DateTimeProvider();
            var iteration = SentryIteration.Create(ordinal, results.Select(x => x.Value), iterationStartedAt, iterationCompletedAt);

            return iteration;
        }

        private async Task UpdateWatcherResultStateAndExecuteHooksPossibleAsync(IWatcher watcher,
            WatcherResultState state,
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

        //Add more aggregated hooks
        private async Task InvokeAggregatedOnFailureHooksAsync(IEnumerable<ISentryCheckResult> sentryCheckResults)
        {
            var invalidResults = sentryCheckResults.Where(x => !x.IsValid);
            _configuration.AggregatedGlobalWatcherHooks.OnFailure.Execute(invalidResults);
            await _configuration.AggregatedGlobalWatcherHooks.OnFirstFailureAsync.ExecuteAsync(invalidResults);
        }

        private async Task InvokeOnStartHooksAsync(WatcherConfiguration watcherConfiguration, IWatcherCheck check)
        {
            watcherConfiguration.Hooks.OnStart.Execute(check);
            await watcherConfiguration.Hooks.OnStartAsync.ExecuteAsync(check);
            _configuration.GlobalWatcherHooks.OnStart.Execute(check);
            await _configuration.GlobalWatcherHooks.OnStartAsync.ExecuteAsync(check);
        }

        private async Task InvokeOnSuccessHooksAsync(WatcherConfiguration watcherConfiguration,
            ISentryCheckResult checkResult)
        {
            watcherConfiguration.Hooks.OnSuccess.Execute(checkResult);
            await watcherConfiguration.Hooks.OnSuccessAsync.ExecuteAsync(checkResult);
            _configuration.GlobalWatcherHooks.OnSuccess.Execute(checkResult);
            await _configuration.GlobalWatcherHooks.OnSuccessAsync.ExecuteAsync(checkResult);
        }

        private async Task InvokeOnFirstSuccessHooksAsync(WatcherConfiguration watcherConfiguration,
            ISentryCheckResult checkResult)
        {
            watcherConfiguration.Hooks.OnFirstSuccess.Execute(checkResult);
            await watcherConfiguration.Hooks.OnFirstSuccessAsync.ExecuteAsync(checkResult);
            _configuration.GlobalWatcherHooks.OnFirstSuccess.Execute(checkResult);
            await _configuration.GlobalWatcherHooks.OnFirstSuccessAsync.ExecuteAsync(checkResult);
        }

        private async Task InvokeOnFailureHooksAsync(WatcherConfiguration watcherConfiguration,
            ISentryCheckResult checkResult)
        {
            watcherConfiguration.Hooks.OnFailure.Execute(checkResult);
            await watcherConfiguration.Hooks.OnFailureAsync.ExecuteAsync(checkResult);
            _configuration.GlobalWatcherHooks.OnFailure.Execute(checkResult);
            await _configuration.GlobalWatcherHooks.OnFailureAsync.ExecuteAsync(checkResult);
        }

        private async Task InvokeOnFirstFailureHooksAsync(WatcherConfiguration watcherConfiguration,
            ISentryCheckResult checkResult)
        {
            watcherConfiguration.Hooks.OnFirstFailure.Execute(checkResult);
            await watcherConfiguration.Hooks.OnFirstFailureAsync.ExecuteAsync(checkResult);
            _configuration.GlobalWatcherHooks.OnFirstFailure.Execute(checkResult);
            await _configuration.GlobalWatcherHooks.OnFirstFailureAsync.ExecuteAsync(checkResult);
        }

        private async Task InvokeOnCompletedHooksAsync(WatcherConfiguration watcherConfiguration,
            ISentryCheckResult checkResult)
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

        public static IIterationProcessor Create(IterationProcessorConfiguration configuration)
            => new IterationProcessor(configuration);

        public static IIterationProcessor Create(Action<IterationProcessorConfiguration.Builder> configuration)
        {
            var config = new IterationProcessorConfiguration.Builder();
            configuration?.Invoke(config);

            return Create(config.Build());
        }
    }
}