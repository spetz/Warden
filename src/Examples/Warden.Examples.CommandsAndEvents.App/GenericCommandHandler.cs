using System;
using System.Threading.Tasks;
using Rebus.Handlers;
using Warden.Commands;

namespace Warden.Examples.CommandsAndEvents.App
{
    public class GenericCommandHandler : IHandleMessages<StopWarden>, IHandleMessages<StartWarden>,  
        IHandleMessages<PauseWarden>, IHandleMessages<KillWarden>
    {
        private readonly RebusWardenCommandHandler _commandHandler;

        public GenericCommandHandler(RebusWardenCommandHandler commandHandler)
        {
            _commandHandler = commandHandler;
        }

        public async Task Handle(StopWarden message)
        {
            Console.WriteLine("Received StopWarden command.");
            _commandHandler.AddCommand(message);
            await Task.CompletedTask;
        }

        public async Task Handle(StartWarden message)
        {
            Console.WriteLine("Received StartWarden command.");
            _commandHandler.AddCommand(message);
            await Task.CompletedTask;
        }

        public async Task Handle(PauseWarden message)
        {
            Console.WriteLine("Received PauseWarden command.");
            _commandHandler.AddCommand(message);
            await Task.CompletedTask;
        }

        public async Task Handle(KillWarden message)
        {
            Console.WriteLine("Received KillWarden command.");
            _commandHandler.AddCommand(message);
            await Task.CompletedTask;
        }
    }
}