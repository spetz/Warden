using System.Threading.Tasks;

namespace Warden.Events
{
    public interface IWardenEventHandler
    {
        Task HandleAsync<T>(T @event) where T : IWardenEvent;
    }
}