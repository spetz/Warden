using System;
using Rebus.Activation;
using Rebus.Config;
using Rebus.Logging;
using Warden.Commander.Commands;
using Rebus.Routing.TypeBased;
using Rebus.Transport.Msmq;

namespace Warden.Examples.RebusApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            using (var activator = new BuiltinHandlerActivator())
            {
                Console.Title = "Warden.Examples.RebusApp";
                activator.Register((bus, message) => new GenericHandler());
                Configure.With(activator)
                    .Logging(l => l.ColoredConsole(minLevel: LogLevel.Debug))
                    .Transport(t => t.UseMsmq("warden-commander-app"))
                    .Routing(r => r.TypeBased()
                        .Map<StopWarden>("warden-commander")
                        .Map<PauseWarden>("warden-commander")
                        .Map<StartWarden>("warden-commander"))
                    .Start();

                activator.Bus.Subscribe<StopWarden>().Wait();
                activator.Bus.Subscribe<PauseWarden>().Wait();
                activator.Bus.Subscribe<StartWarden>().Wait();

                Console.WriteLine("Type q to quit");
                var isRunning = true;
                while (isRunning)
                {
                    var key = Console.ReadKey(true);
                    switch (char.ToLower(key.KeyChar))
                    {
                        case 'q':
                            Console.WriteLine("Bye!");
                            isRunning = false;
                            break;
                    }
                }

                Console.WriteLine("Stopping the bus...");
            }
        }
    }
}
