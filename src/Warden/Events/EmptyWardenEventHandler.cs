using System.Threading.Tasks;

namespace Warden.Events
{
    public class EmptyWardenEventHandler : IWardenEventHandler
    {
        public async Task HandleAsync<T>(T @event) where T : IWardenEvent
        => await Task.CompletedTask;
    }
}