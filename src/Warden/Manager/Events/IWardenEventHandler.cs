using System.Threading.Tasks;

namespace Warden.Manager.Events
{
    public interface IWardenEventHandler
    {
        Task HandleAsync<T>(T @event) where T : IWardenEvent;
    }
}