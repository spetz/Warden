using System;
using System.Threading.Tasks;
using Rebus.Handlers;
using Warden.Events;

namespace Warden.Examples.CommandsAndEvents.Manager
{
    public class GenericEventHandler : IHandleMessages<WardenCommandExecuted>
    {
        public async Task Handle(WardenCommandExecuted message)
        {
            Console.WriteLine("Pause");
            await Task.CompletedTask;
        }
    }
}