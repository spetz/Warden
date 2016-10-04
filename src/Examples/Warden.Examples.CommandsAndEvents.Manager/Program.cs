using System;
using Rebus.Activation;
using Rebus.Config;
using Rebus.Logging;
using Rebus.Routing.TypeBased;
using Warden.Manager.Commands;
using Warden.Manager.Events;

namespace Warden.Examples.CommandsAndEvents.Manager
{
    public class Program
    {
        private static readonly string EventsRoute = "Warden.Examples.CommandsAndEvents.App";

        public static void Main(string[] args)
        {
            using (var activator = new BuiltinHandlerActivator())
            {
                Console.Title = "Warden.Examples.CommandsAndEvents.Manager";
                activator.Register((bus, message) => new GenericEventHandler());
                Configure.With(activator)
                    .Logging(l => l.ColoredConsole(minLevel: LogLevel.Debug))
                    .Transport(t => t.UseMsmq("Warden.Examples.CommandsAndEvents.Manager"))
                    .Routing(r => r.TypeBased()
                        .Map<WardenCommandExecuted>(EventsRoute)
                        .Map<WardenPingResponded>(EventsRoute))
                    .Start();

                activator.Bus.Subscribe<WardenCommandExecuted>().Wait();
                activator.Bus.Subscribe<WardenPingResponded>().Wait();

                Console.WriteLine("Type q to quit\n1 to send PingWarden\n2 to send PauseWarden\n3 to send StopWarden" +
                                  "\n4 to send StartWarden\n5 to send KillWarden");
                var isRunning = true;
                while (isRunning)
                {
                    var key = Console.ReadKey(true);
                    switch (char.ToLower(key.KeyChar))
                    {
                        case '1':
                            Console.WriteLine("Sending PingWarden!");
                            activator.Bus.Publish(new PingWarden()).Wait();
                            break;
                        case '2':
                            Console.WriteLine("Sending PauseWarden!");
                            activator.Bus.Publish(new PauseWarden()).Wait();
                            break;
                        case '3':
                            Console.WriteLine("Sending StopWarden!");
                            activator.Bus.Publish(new StopWarden()).Wait();
                            break;
                        case '4':
                            Console.WriteLine("Sending StartWarden!");
                            activator.Bus.Publish(new StartWarden()).Wait();
                            break;
                        case '5':
                            Console.WriteLine("Sending KillWarden!");
                            activator.Bus.Publish(new KillWarden()).Wait();
                            break;
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
