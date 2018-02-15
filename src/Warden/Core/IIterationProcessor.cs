using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Warden.Utils;
using Warden.Watchers;

namespace Warden.Core
{
    /// <summary>
    /// Processor responsible for executing all of the configured watchers and theirs hooks in a cycle called the iteration.
    /// </summary>
    public interface IIterationProcessor
    {
        /// <summary>
        /// Run a single iteration (cycle) that will execute all of the watchers and theirs hooks.
        /// </summary>
        /// <param name="wardenName">Name of the Warden that will execute the iteration.</param>
        /// <param name="ordinal">Number (ordinal) of executed iteration.</param>
        /// <returns>Single iteration containing the warden name, its ordinal and results of all executed watcher checks.</returns>
        Task<IWardenIteration> ExecuteAsync(string wardenName, long ordinal);
    }

    /// <summary>
    /// Default implementation of the IIterationProcessor.
    /// </summary>
    public class IterationProcessor : IIterationProcessor
    {
        private readonly IterationProcessorConfiguration _configuration;
        private readonly IWardenLogger _logger;
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
            _logger = _configuration.WardenLoggerProvider();
        }

        /// <summary>
        /// Run a single iteration (cycle) that will execute all of the watchers and theirs hooks.
        /// </summary>
        /// <param name="wardenName">Name of the Warden that will execute the iteration.</param>
        /// <param name="ordinal">Number (ordinal) of executed iteration</param>
        /// <returns>Single iteration containing its ordinal and results of all executed watcher checks</returns>
        public async Task<IWardenIteration> ExecuteAsync(string wardenName, long ordinal)
        {
            var iterationStartedAt = _configuration.DateTimeProvider();
            var iterationTasks = _configuration.Watchers.Select(TryExecuteWatcherChecksAndHooksAsync);
            var aggregatedResults = await Task.WhenAll(iterationTasks);
            var results = aggregatedResults.SelectMany(x => x).ToList();
            await ExecuteAggregatedHooksAsync(results);
            var iterationCompletedAt = _configuration.DateTimeProvider();
            var iteration = WardenIteration.Create(wardenName, ordinal, results.Select(x => x.WardenCheckResult),
                iterationStartedAt, iterationCompletedAt);

            return iteration;
        }

        private async Task<IList<WatcherExecutionResult>> TryExecuteWatcherChecksAndHooksAsync(
            WatcherConfiguration watcherConfiguration)
        {
            var results = new List<WatcherExecutionResult>();
            var intervals = _configuration.Watchers
                .OrderBy(x => x.Interval)
                .Select(x => x.Interval.TotalMilliseconds);
            var max = intervals.Max();
            var numberOfExecutions = max/watcherConfiguration.Interval.TotalMilliseconds;

            for (var i = 0; i < numberOfExecutions; i++)
            {
                var watcherResults = await TryExecuteWatcherCheckAndHooksAsync(watcherConfiguration);
                results.AddRange(watcherResults);
                await Task.Delay(watcherConfiguration.Interval);
            }

            return results;
        }

        private async Task<IList<WatcherExecutionResult>> TryExecuteWatcherCheckAndHooksAsync(
            WatcherConfiguration watcherConfiguration)
        {
            var startedAt = _configuration.DateTimeProvider();
            var watcher = watcherConfiguration.Watcher;
            IWardenCheckResult wardenCheckResult = null;
            var results = new List<WatcherExecutionResult>();
            try
            {
                await InvokeOnStartHooksAsync(watcherConfiguration, WatcherCheck.Create(watcher));
                _logger.Info($"Executing Watcher: {watcher.Name}.");
                var watcherCheckResult = await watcher.ExecuteAsync();
                _logger.Info($"Completed executing Watcher: {watcher.Name}. " +
                              $"Is valid: {watcherCheckResult.IsValid}. " +
                              $"Description: {watcherCheckResult.Description}");
                var completedAt = _configuration.DateTimeProvider();
                wardenCheckResult = WardenCheckResult.Create(watcherCheckResult, startedAt, completedAt);
                var watcherResults = await ExecuteWatcherCheckAsync(watcher, watcherCheckResult, wardenCheckResult,
                    watcherConfiguration);
                results.AddRange(watcherResults);
            }
            catch (Exception exception)
            {
                _logger.Error($"There was an error while executing Watcher: {watcher.Name}.", exception);
                var completedAt = _configuration.DateTimeProvider();
                wardenCheckResult = WardenCheckResult.Create(WatcherCheckResult.Create(watcher, false, exception.Message),
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

            return results;
        }

        private async Task<IList<WatcherExecutionResult>>  ExecuteWatcherCheckAsync(IWatcher watcher, IWatcherCheckResult watcherCheckResult, 
            IWardenCheckResult wardenCheckResult, WatcherConfiguration watcherConfiguration)
        {
            var results = new List<WatcherExecutionResult>();
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

            return results;
        }

        private async Task ExecuteAggregatedHooksAsync(IEnumerable<WatcherExecutionResult> results)
        {
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

            try
            {
                await Task.WhenAll(aggregatedHooksTasks);
            }
            catch (Exception exception)
            {
                _logger.Error("There was an error while executing internal Aggregated Global Watcher hooks ", exception);
            }
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
            _logger.Trace("Executing Aggregated Global Watcher hooks OnSuccess.");
            _configuration.AggregatedGlobalWatcherHooks.OnSuccess.Execute(validResults);
            _logger.Trace("Executing Aggregated Global Watcher hooks OnSuccessAsync.");
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

            _logger.Trace("Executing Aggregated Global Watcher hooks OnFirstSuccess.");
            _configuration.AggregatedGlobalWatcherHooks.OnFirstSuccess.Execute(filteredResults);
            _logger.Trace("Executing Aggregated Global Watcher hooks OnFirstSuccessAsync.");
            await _configuration.AggregatedGlobalWatcherHooks.OnFirstSuccessAsync.ExecuteAsync(filteredResults);
        }

        private async Task InvokeAggregatedOnFailureHooksAsync(IEnumerable<WatcherExecutionResult> results)
        {
            var invalidResults = results.Select(x => x.WardenCheckResult).Where(x => !x.IsValid);
            _logger.Trace("Executing Aggregated Global Watcher hooks OnFailure.");
            _configuration.AggregatedGlobalWatcherHooks.OnFailure.Execute(invalidResults);
            _logger.Trace("Executing Aggregated Global Watcher hooks OnFailureAsync.");
            await _configuration.AggregatedGlobalWatcherHooks.OnFailureAsync.ExecuteAsync(invalidResults);
        }

        private async Task InvokeAggregatedOnFirstFailureHooksAsync(IEnumerable<WatcherExecutionResult> results)
        {
            var filteredResults = results.Where(x => x.CurrentState == WatcherResultState.Failure
                                                     && x.PreviousState != WatcherResultState.Failure)
                .Select(x => x.WardenCheckResult);
            if (!filteredResults.Any())
                return;

            _logger.Trace("Executing Aggregated Global Watcher hooks OnFirstFailure.");
            _configuration.AggregatedGlobalWatcherHooks.OnFirstFailure.Execute(filteredResults);
            _logger.Trace("Executing Aggregated Global Watcher hooks OnFirstFailureAsync.");
            await _configuration.AggregatedGlobalWatcherHooks.OnFirstFailureAsync.ExecuteAsync(filteredResults);
        }

        private async Task InvokeAggregatedOnErrorHooksAsync(IEnumerable<WatcherExecutionResult> results)
        {
            var exceptions = results.Select(x => x.Exception).Where(x => x != null);
            _logger.Trace("Executing Aggregated Global Watcher hooks OnError.");
            _configuration.AggregatedGlobalWatcherHooks.OnError.Execute(exceptions);
            _logger.Trace("Executing Aggregated Global Watcher hooks OnErrorAsync.");
            await _configuration.AggregatedGlobalWatcherHooks.OnErrorAsync.ExecuteAsync(exceptions);
        }

        private async Task InvokeAggregatedOnFirstErrorHooksAsync(IEnumerable<WatcherExecutionResult> results)
        {
            var exceptions = results.Where(x => x.CurrentState == WatcherResultState.Error
                                                     && x.PreviousState != WatcherResultState.Error)
                .Select(x => x.Exception);
            if (!exceptions.Any())
                return;

            _logger.Trace("Executing Aggregated Global Watcher hooks OnFirstError.");
            _configuration.AggregatedGlobalWatcherHooks.OnFirstError.Execute(exceptions);
            _logger.Trace("Executing Aggregated Global Watcher hooks OnFirstErrorAsync.");
            await _configuration.AggregatedGlobalWatcherHooks.OnFirstErrorAsync.ExecuteAsync(exceptions);
        }

        private async Task InvokeAggregatedOnCompletedHooksAsync(IEnumerable<WatcherExecutionResult> results)
        {
            var allResults = results.Select(x => x.WardenCheckResult);
            _logger.Trace("Executing Aggregated Global Watcher hooks OnCompleted.");
            _configuration.AggregatedGlobalWatcherHooks.OnCompleted.Execute(allResults);
            _logger.Trace("Executing Aggregated Global Watcher hooks OnCompletedAsync.");
            await _configuration.AggregatedGlobalWatcherHooks.OnCompletedAsync.ExecuteAsync(allResults);
        }

        private async Task InvokeOnStartHooksAsync(WatcherConfiguration watcherConfiguration, IWatcherCheck check)
        {
            _logger.Trace($"Executing Watcher: {check.WatcherName} hooks OnStart.");
            watcherConfiguration.Hooks.OnStart.Execute(check);
            _logger.Trace($"Executing Watcher: {check.WatcherName} hooks OnStartAsync.");
            await watcherConfiguration.Hooks.OnStartAsync.ExecuteAsync(check);
            _logger.Trace("Executing Global Watcher hooks OnStart.");
            _configuration.GlobalWatcherHooks.OnStart.Execute(check);
            _logger.Trace("Executing Global Watcher hooks OnStartAsync.");
            await _configuration.GlobalWatcherHooks.OnStartAsync.ExecuteAsync(check);
        }

        private async Task InvokeOnSuccessHooksAsync(WatcherConfiguration watcherConfiguration,
            IWardenCheckResult checkResult)
        {
            _logger.Trace($"Executing Watcher: {checkResult.WatcherCheckResult.WatcherName} hooks OnSuccess.");
            watcherConfiguration.Hooks.OnSuccess.Execute(checkResult);
            _logger.Trace($"Executing Watcher: {checkResult.WatcherCheckResult.WatcherName} hooks OnSuccessAsync.");
            await watcherConfiguration.Hooks.OnSuccessAsync.ExecuteAsync(checkResult);
            _logger.Trace("Executing Global Watcher hooks OnSuccess.");
            _configuration.GlobalWatcherHooks.OnSuccess.Execute(checkResult);
            _logger.Trace("Executing Global Watcher hooks OnSuccessAsync.");
            await _configuration.GlobalWatcherHooks.OnSuccessAsync.ExecuteAsync(checkResult);
        }

        private async Task InvokeOnFirstSuccessHooksAsync(WatcherConfiguration watcherConfiguration,
            IWardenCheckResult checkResult)
        {
            _logger.Trace($"Executing Watcher: {checkResult.WatcherCheckResult.WatcherName} hooks OnFirstSuccess.");
            watcherConfiguration.Hooks.OnFirstSuccess.Execute(checkResult);
            _logger.Trace($"Executing Watcher: {checkResult.WatcherCheckResult.WatcherName} hooks OnFirstSuccessAsync.");
            await watcherConfiguration.Hooks.OnFirstSuccessAsync.ExecuteAsync(checkResult);
            _logger.Trace("Executing Global Watcher hooks OnFirstSuccess.");
            _configuration.GlobalWatcherHooks.OnFirstSuccess.Execute(checkResult);
            _logger.Trace("Executing Global Watcher hooks OnFirstSuccessAsync.");
            await _configuration.GlobalWatcherHooks.OnFirstSuccessAsync.ExecuteAsync(checkResult);
        }

        private async Task InvokeOnFailureHooksAsync(WatcherConfiguration watcherConfiguration,
            IWardenCheckResult checkResult)
        {
            _logger.Trace($"Executing Watcher: {checkResult.WatcherCheckResult.WatcherName} hooks OnFailure.");
            watcherConfiguration.Hooks.OnFailure.Execute(checkResult);
            _logger.Trace($"Executing Watcher: {checkResult.WatcherCheckResult.WatcherName} hooks OnFailureAsync.");
            await watcherConfiguration.Hooks.OnFailureAsync.ExecuteAsync(checkResult);
            _logger.Trace("Executing Global Watcher hooks OnFailure.");
            _configuration.GlobalWatcherHooks.OnFailure.Execute(checkResult);
            _logger.Trace("Executing Global Watcher hooks OnFailureAsync.");
            await _configuration.GlobalWatcherHooks.OnFailureAsync.ExecuteAsync(checkResult);
        }

        private async Task InvokeOnFirstFailureHooksAsync(WatcherConfiguration watcherConfiguration,
            IWardenCheckResult checkResult)
        {
            _logger.Trace($"Executing Watcher: {checkResult.WatcherCheckResult.WatcherName} hooks OnFirstFailure.");
            watcherConfiguration.Hooks.OnFirstFailure.Execute(checkResult);
            _logger.Trace($"Executing Watcher: {checkResult.WatcherCheckResult.WatcherName} hooks OnFirstFailureAsync.");
            await watcherConfiguration.Hooks.OnFirstFailureAsync.ExecuteAsync(checkResult);
            _logger.Trace("Executing Global Watcher hooks OnFirstFailure.");
            _configuration.GlobalWatcherHooks.OnFirstFailure.Execute(checkResult);
            _logger.Trace("Executing Global Watcher hooks OnFirstFailureAsync.");
            await _configuration.GlobalWatcherHooks.OnFirstFailureAsync.ExecuteAsync(checkResult);
        }

        private async Task InvokeOnCompletedHooksAsync(WatcherConfiguration watcherConfiguration,
            IWardenCheckResult checkResult)
        {
            _logger.Trace($"Executing Watcher: {checkResult.WatcherCheckResult.WatcherName} hooks OnCompleted.");
            watcherConfiguration.Hooks.OnCompleted.Execute(checkResult);
            _logger.Trace($"Executing Watcher: {checkResult.WatcherCheckResult.WatcherName} hooks OnCompletedAsync.");
            await watcherConfiguration.Hooks.OnCompletedAsync.ExecuteAsync(checkResult);
            _logger.Trace("Executing Global Watcher hooks OnCompleted.");
            _configuration.GlobalWatcherHooks.OnCompleted.Execute(checkResult);
            _logger.Trace("Executing Global Watcher hooks OnCompletedAsync.");
            await _configuration.GlobalWatcherHooks.OnCompletedAsync.ExecuteAsync(checkResult);
        }

        private async Task InvokeOnErrorHooksAsync(WatcherConfiguration watcherConfiguration, Exception exception)
        {
            _logger.Trace($"Executing Watcher: {watcherConfiguration.Watcher.Name} hooks OnError.");
            watcherConfiguration.Hooks.OnError.Execute(exception);
            _logger.Trace($"Executing Watcher: {watcherConfiguration.Watcher.Name} hooks OnErrorAsync.");
            await watcherConfiguration.Hooks.OnErrorAsync.ExecuteAsync(exception);
            _logger.Trace("Executing Global Watcher hooks OnError.");
            _configuration.GlobalWatcherHooks.OnError.Execute(exception);
            _logger.Trace("Executing Global Watcher hooks OnErrorAsync.");
            await _configuration.GlobalWatcherHooks.OnErrorAsync.ExecuteAsync(exception);
        }

        private async Task InvokeOnFirstErrorHooksAsync(WatcherConfiguration watcherConfiguration, Exception exception)
        {
            _logger.Trace($"Executing Watcher: {watcherConfiguration.Watcher.Name} hooks OnFirstError.");
            watcherConfiguration.Hooks.OnFirstError.Execute(exception);
            _logger.Trace($"Executing Watcher: {watcherConfiguration.Watcher.Name} hooks OnFirstErrorAsync.");
            await watcherConfiguration.Hooks.OnFirstErrorAsync.ExecuteAsync(exception);
            _logger.Trace("Executing Global Watcher OnFirstError OnError.");
            _configuration.GlobalWatcherHooks.OnFirstError.Execute(exception);
            _logger.Trace("Executing Global Watcher OnFirstErrorAsync OnError.");
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
