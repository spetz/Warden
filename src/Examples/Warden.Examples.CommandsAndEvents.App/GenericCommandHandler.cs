using System;
using System.Threading.Tasks;
using Rebus.Handlers;
using Warden.Manager.Commands;

namespace Warden.Examples.CommandsAndEvents.App
{
    public class GenericCommandHandler : IHandleMessages<StopWarden>, IHandleMessages<StartWarden>,
        IHandleMessages<PauseWarden>, IHandleMessages<KillWarden>, IHandleMessages<PingWarden>
    {
        private readonly RebusWardenCommandSource _commandSource;

        public GenericCommandHandler(RebusWardenCommandSource commandSource)
        {
            _commandSource = commandSource;
        }

        public async Task Handle(StopWarden message)
            => await HandleCommandAsync(message);

        public async Task Handle(StartWarden message)
            => await HandleCommandAsync(message);

        public async Task Handle(PauseWarden message)
            => await HandleCommandAsync(message);

        public async Task Handle(KillWarden message)
            => await HandleCommandAsync(message);

        public async Task Handle(PingWarden message)
            => await HandleCommandAsync(message);

        private async Task HandleCommandAsync<T>(T command) where T : IWardenCommand
        {
            var commandName = command.GetType().Name;
            Console.WriteLine($"Received {commandName} command.");
            await _commandSource.AddCommandAsync(command);
        }
    }
}