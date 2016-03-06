using System.Linq;
using System.Threading.Tasks;
using Sentry.Core;

namespace Sentry
{
    public interface ISentry
    {
        Task ExecuteAsync();
    }

    public class Sentry : ISentry
    {
        private readonly SentryConfiguration _configuration;

        public Sentry(SentryConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task ExecuteAsync()
        {
            var tasks = _configuration.Watchers.Select(x => x.ExecuteAsync());
            await Task.WhenAll(tasks);
        }
    }
}