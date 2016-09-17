using System.Threading.Tasks;

namespace Warden.Manager.Events
{
    public class EmptyWardenEventHandler : IWardenEventHandler
    {
        public async Task HandleAsync<T>(T @event) where T : IWardenEvent
        => await Task.CompletedTask;
    }
}