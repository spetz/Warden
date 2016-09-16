using System;
using System.Threading.Tasks;

namespace Warden.Commands
{
    public class EmptyWardenCommandSource : IWardenCommandSource
    {
        public Func<IWardenCommand, Task> CommandReceivedAsync { get; set; }
    }
}