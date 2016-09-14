using System.Threading.Tasks;
using Warden.Events;

namespace Warden.Examples.CommandsAndEvents.App
{
    public class RebusWardenEventHandler : IWardenEventHandler
    {
        public async Task HandleAsync<T>(T @event) where T : IWardenEvent
        {
            await Task.CompletedTask;
        }
    }
}