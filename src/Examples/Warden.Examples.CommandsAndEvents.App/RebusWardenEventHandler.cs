using System.Threading.Tasks;
using Rebus.Bus;
using Warden.Events;

namespace Warden.Examples.CommandsAndEvents.App
{
    public class RebusWardenEventHandler : IWardenEventHandler
    {
        private readonly IBus _bus;

        public RebusWardenEventHandler(IBus bus)
        {
            _bus = bus;
        }

        public async Task HandleAsync<T>(T @event) where T : IWardenEvent
        {
            await _bus.Publish(@event);
        }
    }
}