using System;
using System.Threading.Tasks;
using Rebus.Handlers;
using Warden.Commander.Commands;

namespace Warden.Examples.RebusApp
{
    public class GenericHandler : IHandleMessages<StopWarden>, IHandleMessages<StartWarden>,  IHandleMessages<PauseWarden>
    {
        public async Task Handle(StopWarden message)
        {
            Console.WriteLine("Stop");
        }

        public async Task Handle(StartWarden message)
        {
            Console.WriteLine("Start");
        }

        public async Task Handle(PauseWarden message)
        {
            Console.WriteLine("Pause");
        }
    }
}