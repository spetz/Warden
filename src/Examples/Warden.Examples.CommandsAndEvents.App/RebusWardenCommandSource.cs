using System;
using System.Threading.Tasks;
using Warden.Manager.Commands;

namespace Warden.Examples.CommandsAndEvents.App
{
    public class RebusWardenCommandSource : IWardenCommandSource
    {
        public Func<IWardenCommand, Task> CommandReceivedAsync { get; set; }

        public async Task AddCommandAsync(IWardenCommand command)
        {
            if(CommandReceivedAsync == null)
                return;

            await CommandReceivedAsync.Invoke(command);
        }
    }
}