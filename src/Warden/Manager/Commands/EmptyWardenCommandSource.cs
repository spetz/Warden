using System;
using System.Threading.Tasks;

namespace Warden.Manager.Commands
{
    public class EmptyWardenCommandSource : IWardenCommandSource
    {
        public Func<IWardenCommand, Task> CommandReceivedAsync { get; set; }
    }
}