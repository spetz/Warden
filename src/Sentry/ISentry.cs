using System;
using System.Threading.Tasks;
using Sentry.Core;

namespace Sentry
{
    public interface ISentry
    {
        Task StartAsync();
        Task PauseAsync();
        Task StopAsync();
    }

    public class Sentry : ISentry
    {
        private readonly SentryConfiguration _configuration;
        private long _iterationOrdinal = 1;
        private bool _running = false;

        public Sentry(SentryConfiguration configuration)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration), "Sentry configuration has not been provided.");

            _configuration = configuration;
        }

        public async Task StartAsync()
        {
            _running = true;
            _configuration.Hooks.OnStart.Execute();
            await _configuration.Hooks.OnStartAsync.ExecuteAsync();

            try
            {
                while (CanExecuteIteration(_iterationOrdinal))
                {
                    _configuration.Hooks.OnIterationStart.Execute(_iterationOrdinal);
                    await _configuration.Hooks.OnIterationStartAsync.ExecuteAsync(_iterationOrdinal);
                    var iteration = await _configuration.IterationProcessor.ExecuteAsync(_iterationOrdinal);
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
            if (!_running)
                return false;
            if (!_configuration.IterationsCount.HasValue)
                return true;
            if (ordinal <= _configuration.IterationsCount)
                return true;

            return false;
        }

        public async Task PauseAsync()
        {
            _running = false;
            _configuration.Hooks.OnPause.Execute();
            await _configuration.Hooks.OnPauseAsync.ExecuteAsync();
        }

        public async Task StopAsync()
        {
            _running = false;
            _iterationOrdinal = 1;
            _configuration.Hooks.OnStop.Execute();
            await _configuration.Hooks.OnStopAsync.ExecuteAsync();
        }

        public static ISentry Create(SentryConfiguration configuration) => new Sentry(configuration);

        public static ISentry Create(Action<SentryConfiguration.Builder> configuration)
        {
            var config = new SentryConfiguration.Builder();
            configuration?.Invoke(config);

            return Create(config.Build());
        }
    }
}