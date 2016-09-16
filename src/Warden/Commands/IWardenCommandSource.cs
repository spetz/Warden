using System;
using System.Threading.Tasks;

namespace Warden.Commands
{
    public interface IWardenCommandSource
    {
        Func<IWardenCommand, Task> CommandReceivedAsync { get; set; }
    }
}