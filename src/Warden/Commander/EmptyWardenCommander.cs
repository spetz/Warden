using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Warden.Commander
{
    public class EmptyWardenCommander : IWardenCommander
    {
        public async Task<IEnumerable<IWardenCommand>> ReceiveAsync()
            => await Task.FromResult(Enumerable.Empty<IWardenCommand>());
    }
}