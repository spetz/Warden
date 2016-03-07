using System.Threading.Tasks;

namespace Sentry
{
    public interface IWatcher
    {
        string Name { get; }
        Task<IWatcherCheckResult> ExecuteAsync();
    }
}