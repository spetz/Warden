using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Warden.Configurations;
using Warden.Core;
using Warden.Watchers;

namespace Warden
{
    /// <summary>
    /// Processor responsible for executing all of the configured watchers and theirs hooks in a cycle called the iteration.
    /// </summary>
    public interface IIterationProcessor
    {
        /// <summary>
        /// Run a single iteration (cycle) that will execute all of the watchers and theirs hooks.
        /// </summary>
        /// <param name="ordinal">Number (ordinal) of executed iteration</param>
        /// <returns>Single iteration containing its ordinal and results of all executed watcher checks</returns>
        Task<IWardenIteration> ExecuteAsync(long ordinal);
    }


    /// <summary>
    /// Default implementation of the IIterationProcessor.
    /// </summary>
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
        public async Task<IWardenIteration> ExecuteAsync(long ordinal)
        {
            var iterationStartedAt = _configuration.DateTimeProvider();
            var results = new List<WatcherExecutionResult>();
            var iterationTasks = _configuration.Watchers.Select(async watcherConfiguration =>
            {
                var startedAt = _configuration.DateTimeProvider();
                var watcher = watcherConfiguration.Watcher;
                IWardenCheckResult wardenCheckResult = null;
                try
                {
                    await InvokeOnStartHooksAsync(watcherConfiguration, WatcherCheck.Create(watcher));
                    var watcherCheckResult = await watcher.ExecuteAsync();
                    var completedAt = _configuration.DateTimeProvider();
                    wardenCheckResult = WardenCheckResult.Create(watcherCheckResult, startedAt, completedAt);
                    if (watcherCheckResult.IsValid)
                    {
                        var result = wardenCheckResult;
                        results.Add(new WatcherExecutionResult(watcher, WatcherResultState.Success,
                            GetPreviousWatcherState(watcher), result));
                        await UpdateWatcherResultStateAndExecuteHooksPossibleAsync(watcher, WatcherResultState.Success,
                            () => InvokeOnFirstSuccessHooksAsync(watcherConfiguration, result),
                            executeIfLatestStateIsNotSet: false);
                        await InvokeOnSuccessHooksAsync(watcherConfiguration, wardenCheckResult);
                    }
                    else
                    {
                        var result = wardenCheckResult;
                        results.Add(new WatcherExecutionResult(watcher, WatcherResultState.Failure,
                            GetPreviousWatcherState(watcher), result));
                        await UpdateWatcherResultStateAndExecuteHooksPossibleAsync(watcher, WatcherResultState.Failure,
                            () => InvokeOnFirstFailureHooksAsync(watcherConfiguration, result));
                        await InvokeOnFailureHooksAsync(watcherConfiguration, wardenCheckResult);
                    }
                }
                catch (Exception exception)
                {
                    var completedAt = _configuration.DateTimeProvider();
                    wardenCheckResult = WardenCheckResult.Create(WatcherCheckResult.Create(watcher, false),
                        startedAt, completedAt, exception);
                    results.Add(new WatcherExecutionResult(watcher, WatcherResultState.Error,
                        GetPreviousWatcherState(watcher), wardenCheckResult, exception));
                    await UpdateWatcherResultStateAndExecuteHooksPossibleAsync(watcher, WatcherResultState.Error,
                        () => InvokeOnFirstErrorHooksAsync(watcherConfiguration, exception));
                    var wardenException = new WardenException("There was an error while executing Warden " +
                                                              $"caused by watcher: '{watcher.Name}'.", exception);

                    await InvokeOnErrorHooksAsync(watcherConfiguration, wardenException);
                }
                finally
                {
                    await InvokeOnCompletedHooksAsync(watcherConfiguration, wardenCheckResult);
                }
            });

            await Task.WhenAll(iterationTasks);

            var aggregatedHooksTasks = new[]
            {
                InvokeAggregatedOnFirstSuccessHooksAsync(results),
                InvokeAggregatedOnSuccessHooksAsync(results),
                InvokeAggregatedOnFirstFailureHooksAsync(results),
                InvokeAggregatedOnFailureHooksAsync(results),
                InvokeAggregatedOnFirstErrorHooksAsync(results),
                InvokeAggregatedOnErrorHooksAsync(results),
                InvokeAggregatedOnCompletedHooksAsync(results)
            };
            await Task.WhenAll(aggregatedHooksTasks);
            var iterationCompletedAt = _configuration.DateTimeProvider();
            var iteration = WardenIteration.Create(ordinal, results.Select(x => x.WardenCheckResult),
                iterationStartedAt, iterationCompletedAt);

            return iteration;
        }

        private class WatcherExecutionResult
        {
            public IWatcher Watcher { get; }
            public WatcherResultState CurrentState { get; }
            public WatcherResultState PreviousState { get; }
            public IWardenCheckResult WardenCheckResult { get; }
            public Exception Exception { get; }

            public WatcherExecutionResult(IWatcher watcher, WatcherResultState currentState,
                WatcherResultState previousState, IWardenCheckResult wardenCheckResult, 
                Exception exception = null)
            {
                Watcher = watcher;
                CurrentState = currentState;
                PreviousState = previousState;
                WardenCheckResult = wardenCheckResult;
                Exception = exception;
            }
        }

        private async Task UpdateWatcherResultStateAndExecuteHooksPossibleAsync(IWatcher watcher,
            WatcherResultState state, Func<Task> hooks, bool executeIfLatestStateIsNotSet = true)
        {
            var previousState = GetPreviousWatcherState(watcher);
            if (previousState == state)
                return;

            if (previousState == WatcherResultState.NotSet && !executeIfLatestStateIsNotSet)
                return;

            _latestWatcherResultStates[watcher] = state;
            await hooks();
        }

        private WatcherResultState GetPreviousWatcherState(IWatcher watcher)
        {
            return _latestWatcherResultStates.ContainsKey(watcher) ? _latestWatcherResultStates[watcher] : WatcherResultState.NotSet;
        }

        private async Task InvokeAggregatedOnSuccessHooksAsync(IEnumerable<WatcherExecutionResult> results)
        {
            var validResults = results.Select(x => x.WardenCheckResult).Where(x => x.IsValid);
            _configuration.AggregatedGlobalWatcherHooks.OnSuccess.Execute(validResults);
            await _configuration.AggregatedGlobalWatcherHooks.OnSuccessAsync.ExecuteAsync(validResults);
        }

        private async Task InvokeAggregatedOnFirstSuccessHooksAsync(IEnumerable<WatcherExecutionResult> results)
        {
            var filteredResults = results.Where(x => x.CurrentState == WatcherResultState.Success
                                                     && x.PreviousState != WatcherResultState.Success
                                                     && x.PreviousState != WatcherResultState.NotSet)
                .Select(x => x.WardenCheckResult);
            if (!filteredResults.Any())
                return;

            _configuration.AggregatedGlobalWatcherHooks.OnFirstSuccess.Execute(filteredResults);
            await _configuration.AggregatedGlobalWatcherHooks.OnFirstSuccessAsync.ExecuteAsync(filteredResults);
        }

        private async Task InvokeAggregatedOnFailureHooksAsync(IEnumerable<WatcherExecutionResult> results)
        {
            var invalidResults = results.Select(x => x.WardenCheckResult).Where(x => x.IsValid);
            _configuration.AggregatedGlobalWatcherHooks.OnFailure.Execute(invalidResults);
            await _configuration.AggregatedGlobalWatcherHooks.OnFailureAsync.ExecuteAsync(invalidResults);
        }

        private async Task InvokeAggregatedOnFirstFailureHooksAsync(IEnumerable<WatcherExecutionResult> results)
        {
            var filteredResults = results.Where(x => x.CurrentState == WatcherResultState.Failure
                                                     && x.PreviousState != WatcherResultState.Failure)
                .Select(x => x.WardenCheckResult);
            if (!filteredResults.Any())
                return;

            _configuration.AggregatedGlobalWatcherHooks.OnFirstFailure.Execute(filteredResults);
            await _configuration.AggregatedGlobalWatcherHooks.OnFirstFailureAsync.ExecuteAsync(filteredResults);
        }

        private async Task InvokeAggregatedOnErrorHooksAsync(IEnumerable<WatcherExecutionResult> results)
        {
            var exceptions = results.Select(x => x.Exception).Where(x => x != null);
            _configuration.AggregatedGlobalWatcherHooks.OnError.Execute(exceptions);
            await _configuration.AggregatedGlobalWatcherHooks.OnErrorAsync.ExecuteAsync(exceptions);
        }

        private async Task InvokeAggregatedOnFirstErrorHooksAsync(IEnumerable<WatcherExecutionResult> results)
        {
            var exceptions = results.Where(x => x.CurrentState == WatcherResultState.Error
                                                     && x.PreviousState != WatcherResultState.Error)
                .Select(x => x.Exception);
            if (!exceptions.Any())
                return;

            _configuration.AggregatedGlobalWatcherHooks.OnFirstError.Execute(exceptions);
            await _configuration.AggregatedGlobalWatcherHooks.OnFirstErrorAsync.ExecuteAsync(exceptions);
        }

        private async Task InvokeAggregatedOnCompletedHooksAsync(IEnumerable<WatcherExecutionResult> results)
        {
            var allResults = results.Select(x => x.WardenCheckResult);
            _configuration.AggregatedGlobalWatcherHooks.OnCompleted.Execute(allResults);
            await _configuration.AggregatedGlobalWatcherHooks.OnCompletedAsync.ExecuteAsync(allResults);
        }

        private async Task InvokeOnStartHooksAsync(WatcherConfiguration watcherConfiguration, IWatcherCheck check)
        {
            watcherConfiguration.Hooks.OnStart.Execute(check);
            await watcherConfiguration.Hooks.OnStartAsync.ExecuteAsync(check);
            _configuration.GlobalWatcherHooks.OnStart.Execute(check);
            await _configuration.GlobalWatcherHooks.OnStartAsync.ExecuteAsync(check);
        }

        private async Task InvokeOnSuccessHooksAsync(WatcherConfiguration watcherConfiguration,
            IWardenCheckResult checkResult)
        {
            watcherConfiguration.Hooks.OnSuccess.Execute(checkResult);
            await watcherConfiguration.Hooks.OnSuccessAsync.ExecuteAsync(checkResult);
            _configuration.GlobalWatcherHooks.OnSuccess.Execute(checkResult);
            await _configuration.GlobalWatcherHooks.OnSuccessAsync.ExecuteAsync(checkResult);
        }

        private async Task InvokeOnFirstSuccessHooksAsync(WatcherConfiguration watcherConfiguration,
            IWardenCheckResult checkResult)
        {
            watcherConfiguration.Hooks.OnFirstSuccess.Execute(checkResult);
            await watcherConfiguration.Hooks.OnFirstSuccessAsync.ExecuteAsync(checkResult);
            _configuration.GlobalWatcherHooks.OnFirstSuccess.Execute(checkResult);
            await _configuration.GlobalWatcherHooks.OnFirstSuccessAsync.ExecuteAsync(checkResult);
        }

        private async Task InvokeOnFailureHooksAsync(WatcherConfiguration watcherConfiguration,
            IWardenCheckResult checkResult)
        {
            watcherConfiguration.Hooks.OnFailure.Execute(checkResult);
            await watcherConfiguration.Hooks.OnFailureAsync.ExecuteAsync(checkResult);
            _configuration.GlobalWatcherHooks.OnFailure.Execute(checkResult);
            await _configuration.GlobalWatcherHooks.OnFailureAsync.ExecuteAsync(checkResult);
        }

        private async Task InvokeOnFirstFailureHooksAsync(WatcherConfiguration watcherConfiguration,
            IWardenCheckResult checkResult)
        {
            watcherConfiguration.Hooks.OnFirstFailure.Execute(checkResult);
            await watcherConfiguration.Hooks.OnFirstFailureAsync.ExecuteAsync(checkResult);
            _configuration.GlobalWatcherHooks.OnFirstFailure.Execute(checkResult);
            await _configuration.GlobalWatcherHooks.OnFirstFailureAsync.ExecuteAsync(checkResult);
        }

        private async Task InvokeOnCompletedHooksAsync(WatcherConfiguration watcherConfiguration,
            IWardenCheckResult checkResult)
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