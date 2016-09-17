using System;
using System.Threading.Tasks;
using Rebus.Handlers;
using Warden.Manager.Events;

namespace Warden.Examples.CommandsAndEvents.Manager
{
    public class GenericEventHandler : IHandleMessages<WardenCommandExecuted>, IHandleMessages<WardenPingResponded>
    {
        public async Task Handle(WardenCommandExecuted message)
            => await HandleEventAsync(message);

        public async Task Handle(WardenPingResponded message)
            => await HandleEventAsync(message);

        private async Task HandleEventAsync<T>(T @event) where T : IWardenEvent
        {
            var eventName = @event.GetType().Name;
            Console.WriteLine($"Received {eventName} event.");
            await Task.CompletedTask;
        }
    }
}