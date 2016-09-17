using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Warden.Manager.Commands;
using Warden.Manager.Events;
using Warden.Utils;

namespace Warden.Manager
{
    //Default implementation of the IWardenManager.
    public class WardenManager : IWardenManager
    {
        private readonly IWardenLogger _logger;
        private readonly IWarden _warden;
        private readonly WardenManagerConfiguration _configuration;
        private readonly IWardenCommandSource _commandSource;
        private readonly IWardenEventHandler _eventHandler;
        private bool _isManagerRunning = false;
        private bool _isWardenRunning = false;

        internal WardenManager(IWarden warden, WardenManagerConfiguration configuration)
        {
            if (warden == null)
                throw new ArgumentException("Warden instance can not be null.", nameof(warden));
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration),
                    "Warden Manager configuration has not been provided.");
            }

            _warden = warden;
            _configuration = configuration;
            _commandSource = _configuration.WardenCommandSourceProvider();
            _eventHandler = _configuration.WardenEventHandlerProvider();
            _logger = _configuration.WardenLoggerProvider();
            _commandSource.CommandReceivedAsync += async (command) => await HandleCommandAsync(command);
        }

        /// <summary>
        /// Start the Warden Manager using the underlying IWarden instance.
        /// </summary>
        /// <returns></returns>
        public async Task StartAsync()
        {
            _logger.Info("Starting Warden Manager.");
            _isManagerRunning = true;
            _isWardenRunning = true;
            await _warden.StartAsync();
            while (_isManagerRunning)
            {
                await Task.Delay(1000);
                if (_isWardenRunning)
                    continue;

                _logger.Trace("Warden has been stopped, awaiting for the start command...");
            }
        }

        /// <summary>
        /// Stop the Warden Manager including the underlying IWarden instance.
        /// </summary>
        /// <returns></returns>
        public async Task StopAsync()
        {
            _logger.Info("Stopping Warden Manager.");
            _isManagerRunning = false;
            _isWardenRunning = false;
            await Task.CompletedTask;
        }

        private async Task HandleCommandAsync<T>(T command) where T : IWardenCommand
        {
            var commandName = command.GetType().Name;
            _logger.Trace($"Executing command {commandName}.");
            if (command is PingWarden)
                await PingAsync();
            else if (command is StopWarden)
            {
                _isWardenRunning = false;
                await _warden.StopAsync();
            }
            else if (command is StartWarden)
            {
                _isWardenRunning = true;
                await _warden.StartAsync();

            }
            else if (command is PauseWarden)
            {
                _isWardenRunning = false;
                await _warden.PauseAsync();
            }
            else if (command is KillWarden)
            {
                await PublishCommandExecutedEvent(commandName);
                Process.GetCurrentProcess().Kill();
            }
            await PublishCommandExecutedEvent(commandName);
        }

        private async Task PingAsync()
            => await SendEventAsync(new WardenPingResponded());

        private async Task PublishCommandExecutedEvent(string name)
            => await SendEventAsync(new WardenCommandExecuted
            {
                Name = name
            });

        private async Task SendEventAsync<T>(T @event) where T : IWardenEvent
        {
            var eventName = @event.GetType().Name;
            _logger.Trace($"Sending event {eventName}.");
            await _eventHandler.HandleAsync(@event);
        }

        /// <summary>
        /// Factory method for creating a new Warden Manager instance with provided configuration.".
        /// </summary>
        /// <param name="warden">Instance of the IWarden.</param>
        /// <param name="configuration">Configuration of WardenManager.</param>
        /// <returns>Instance of IWardenManager.</returns>
        public static IWardenManager Create(IWarden warden, WardenManagerConfiguration configuration)
            => new WardenManager(warden, configuration);

        /// <summary>
        /// Factory method for creating a new Warden Manager instance, for which the configuration can be provided via the lambda expression.
        /// </summary>
        /// <param name="warden">Instance of the IWarden.</param>
        /// <param name="configurator">Lambda expression to build the configuration of WardenManager.</param>
        /// <returns>Instance of IWardenManager.</returns>
        public static IWardenManager Create(IWarden warden, Action<WardenManagerConfiguration.Builder> configurator)
        {
            var config = new WardenManagerConfiguration.Builder();
            configurator?.Invoke(config);

            return Create(warden, config.Build());
        }
    }
}