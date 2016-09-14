using System.Collections.Generic;
using System.Threading.Tasks;

namespace Warden.Commander
{
    public interface IWardenCommander
    {
        Task<IEnumerable<IWardenCommand>> ReceiveAsync();
    }
}