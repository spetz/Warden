using System;
using System.Threading.Tasks;
using Warden.Core;

namespace Warden
{
    public interface IWarden
    {
        /// <summary>
        /// Start the Warden. It will be running iterations in a loop (infinite by default but can bo changed) and executing all of the configured hooks.
        /// </summary>
        /// <returns></returns>
        Task StartAsync();

        /// <summary>
        /// Pause the Warden. It will not reset the current iteration number (ordinal) back to 1. Can be resumed by invoking StartAsync().
        /// </summary>
        /// <returns></returns>
        Task PauseAsync();

        /// <summary>
        /// Stop the Warden. It will reset the current iteration number (ordinal) back to 1. Can be resumed by invoking StartAsync().
        /// </summary>
        /// <returns></returns>
        Task StopAsync();
    }

    public class Warden : IWarden
    {
        private readonly WardenConfiguration _configuration;
        private long _iterationOrdinal = 1;
        private bool _running = false;
        /// <summary>
        /// Initialize a new instance of the Warden using the provided configuration.
        /// </summary>
        /// <param name="configuration">Configuration of Warden</param>
        public Warden(WardenConfiguration configuration)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration), "Warden configuration has not been provided.");

            _configuration = configuration;
        }

        /// <summary>
        /// Start the Warden. 
        /// It will be running iterations in a loop (infinite by default but can bo changed) and executing all of the configured hooks.
        /// </summary>
        /// <returns></returns>
        public async Task StartAsync()
        {
            _running = true;
            _configuration.Hooks.OnStart.Execute();
            await _configuration.Hooks.OnStartAsync.ExecuteAsync();
            var iterationProcessor = _configuration.IterationProcessorProvider();

            while (CanExecuteIteration(_iterationOrdinal))
            {
                try
                {
                    _configuration.Hooks.OnIterationStart.Execute(_iterationOrdinal);
                    await _configuration.Hooks.OnIterationStartAsync.ExecuteAsync(_iterationOrdinal);
                    var iteration = await iterationProcessor.ExecuteAsync(_iterationOrdinal);
                    _configuration.Hooks.OnIterationCompleted.Execute(iteration);
                    await _configuration.Hooks.OnIterationCompletedAsync.ExecuteAsync(iteration);
                    var canExecuteNextIteration = CanExecuteIteration(_iterationOrdinal + 1);
                    if (!canExecuteNextIteration)
                        break;

                    await Task.Delay(_configuration.IterationDelay);
                    _iterationOrdinal++;
                }
                catch (Exception exception)
                {
                    try
                    {
                        _configuration.Hooks.OnError.Execute(exception);
                        await _configuration.Hooks.OnErrorAsync.ExecuteAsync(exception);
                    }
                    catch (Exception onErrorException)
                    {
                        //Think what to do about it
                    }
                }
            }
        }

        private bool CanExecuteIteration(long ordinal)
        {
            if (!_running)
                return false;
            if (!_configuration.IterationsCount.HasValue)
                return true;
            if (ordinal <= _configuration.IterationsCount)
                return true;

            return false;
        }

        /// <summary>
        /// Pause the Warden. 
        /// It will not reset the current iteration number (ordinal) back to 1. Can be resumed by invoking StartAsync().
        /// </summary>
        /// <returns></returns>
        public async Task PauseAsync()
        {
            _running = false;
            _configuration.Hooks.OnPause.Execute();
            await _configuration.Hooks.OnPauseAsync.ExecuteAsync();
        }

        /// <summary>
        /// Stop the Warden. 
        /// It will reset the current iteration number (ordinal) back to 1. Can be resumed by invoking StartAsync().
        /// </summary>
        /// <returns></returns>
        public async Task StopAsync()
        {
            _running = false;
            _iterationOrdinal = 1;
            _configuration.Hooks.OnStop.Execute();
            await _configuration.Hooks.OnStopAsync.ExecuteAsync();
        }

        /// <summary>
        /// Factory method for creating a new Warden instance with provided configuration.
        /// </summary>
        /// <param name="configuration">Configuration of Warden.</param>
        /// <returns>Instance of IWarden.</returns>
        public static IWarden Create(WardenConfiguration configuration) => new Warden(configuration);

        /// <summary>
        /// Factory method for creating a new Warden instance, for which the configuration can be provided via the lambda expression.
        /// </summary>
        /// <param name="configuration">Lambda expression to build configuration of Warden</param>
        /// <returns>Instance of IWarden.</returns>
        public static IWarden Create(Action<WardenConfiguration.Builder> configuration)
        {
            var config = new WardenConfiguration.Builder();
            configuration?.Invoke(config);

            return Create(config.Build());
        }
    }
}