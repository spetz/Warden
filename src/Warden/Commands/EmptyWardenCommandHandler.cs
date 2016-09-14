using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Warden.Commands
{
    public class EmptyWardenCommandHandler : IWardenCommandHandler
    {
        public async Task<IEnumerable<IWardenCommand>> ReceiveAsync()
            => await Task.FromResult(Enumerable.Empty<IWardenCommand>());
    }
}