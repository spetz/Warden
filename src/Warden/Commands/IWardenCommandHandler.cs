using System.Collections.Generic;
using System.Threading.Tasks;

namespace Warden.Commands
{
    public interface IWardenCommandHandler
    {
        Task<IEnumerable<IWardenCommand>> ReceiveAsync();
    }
}