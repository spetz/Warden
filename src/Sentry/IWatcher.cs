using System.Threading.Tasks;

namespace Sentry
{
    public interface IWatcher
    {
        Task ExecuteAsync();
    }
}