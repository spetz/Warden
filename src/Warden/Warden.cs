using System;
using System.Threading.Tasks;
using Warden.Commander;
using Warden.Core;
using Warden.Utils;

namespace Warden
{
    /// <summary>
    /// Default implementation of the IWarden interface.
    /// </summary>
    public class Warden : IWarden
    {
        private readonly IWardenLogger _logger;
        private readonly IWardenCommander _commander;
        private readonly WardenConfiguration _configuration;
        private long _iterationOrdinal = 1;
        private bool _running = false;

        public string Name { get; }

        /// <summary>
        /// Initialize a new instance of the Warden using the provided configuration.
        /// </summary>
        /// <param name="name">Customizable name of the Warden.</param>
        /// <param name="configuration">Configuration of Warden</param>
        internal Warden(string name, WardenConfiguration configuration)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Warden name can not be empty.", nameof(name));
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration), "Warden configuration has not been provided.");

            Name = name;
            _configuration = configuration;
            _logger = _configuration.WardenLoggerProvider();
            _commander = _configuration.WardenCommanderProvider();
        }

        /// <summary>
        /// Start the Warden. 
        /// It will be running iterations in a loop (infinite by default but can be changed) and executing all of the configured hooks.
        /// </summary>
        /// <returns></returns>
        public async Task StartAsync()
        {
            _logger.Info($"Starting Warden: {Name}");
            _running = true;
            _logger.Trace("Executing Warden hooks OnStart.");
            _configuration.Hooks.OnStart.Execute();
            _logger.Trace("Executing Warden hooks OnStartAsync.");
            await _configuration.Hooks.OnStartAsync.ExecuteAsync();
            await TryExecuteIterationsAsync();
        }

        private async Task TryExecuteIterationsAsync()
        {
            var iterationProcessor = _configuration.IterationProcessorProvider();
            while (CanExecuteIteration(_iterationOrdinal))
            {
                try
                {
                    await ExecuteIterationAsync(iterationProcessor);
                    var canExecuteNextIteration = CanExecuteIteration(_iterationOrdinal + 1);
                    if (!canExecuteNextIteration)
                        break;

                    _iterationOrdinal++;
                }
                catch (Exception exception)
                {
                    try
                    {
                        _logger.Error("There was an error while executing Warden iteration " +
                                      $"{_iterationOrdinal}.", exception);
                        _logger.Trace("Executing Warden hooks OnError.");
                        _configuration.Hooks.OnError.Execute(exception);
                        _logger.Trace("Executing Warden hooks OnErrorAsync.");
                        await _configuration.Hooks.OnErrorAsync.ExecuteAsync(exception);
                    }
                    catch (Exception onErrorException)
                    {
                        _logger.Error("There was an error while executing internal Warden error hooks " +
                                      $"for iteration {_iterationOrdinal}.", onErrorException);
                    }
                }
            }
        }

        private async Task ExecuteIterationAsync(IIterationProcessor iterationProcessor)
        {
            await ExecuteCommands();
            _logger.Trace("Executing Warden hooks OnIterationStart.");
            _configuration.Hooks.OnIterationStart.Execute(_iterationOrdinal);
            _logger.Trace("Executing Warden hooks OnIterationStartAsync.");
            await _configuration.Hooks.OnIterationStartAsync.ExecuteAsync(_iterationOrdinal);
            _logger.Info($"Executing Warden iteration {_iterationOrdinal}.");
            var iteration = await iterationProcessor.ExecuteAsync(Name, _iterationOrdinal);
            _logger.Trace("Executing Warden hooks OnIterationCompleted.");
            _configuration.Hooks.OnIterationCompleted.Execute(iteration);
            _logger.Trace("Executing Warden hooks OnIterationCompletedAsync.");
            await _configuration.Hooks.OnIterationCompletedAsync.ExecuteAsync(iteration);
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
            _logger.Info($"Pausing Warden: {Name}");
            _running = false;
            _logger.Trace("Executing Warden hooks OnPause.");
            _configuration.Hooks.OnPause.Execute();
            _logger.Trace("Executing Warden hooks OnPauseAsync.");
            await _configuration.Hooks.OnPauseAsync.ExecuteAsync();
        }

        /// <summary>
        /// Stop the Warden. 
        /// It will reset the current iteration number (ordinal) back to 1. Can be resumed by invoking StartAsync().
        /// </summary>
        /// <returns></returns>
        public async Task StopAsync()
        {
            _logger.Info($"Stopping Warden: {Name}");
            _running = false;
            _iterationOrdinal = 1;
            _logger.Trace("Executing Warden hooks OnStop.");
            _configuration.Hooks.OnStop.Execute();
            _logger.Trace("Executing Warden hooks OnStopAsync.");
            await _configuration.Hooks.OnStopAsync.ExecuteAsync();
        }

        private async Task ExecuteCommands()
        {
            var commands = await _commander.ReceiveAsync();
        }
    }
}