using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Warden.Commands;

namespace Warden.Examples.CommandsAndEvents.App
{
    public class RebusWardenCommandHandler : IWardenCommandHandler
    {
        private readonly ConcurrentQueue<IWardenCommand> _commands = new ConcurrentQueue<IWardenCommand>();

        public void AddCommand(IWardenCommand command)
        {
            _commands.Enqueue(command);
        }

        public async Task<IEnumerable<IWardenCommand>> ReceiveAsync()
        {
            var commands = new List<IWardenCommand>();
            IWardenCommand command;
            while (_commands.TryDequeue(out command))
            {
                commands.Add(command);
            }

            return await Task.FromResult(commands);
        }
    }
}