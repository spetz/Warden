using System;
using Rebus.Activation;
using Rebus.Config;
using Rebus.Logging;
using Rebus.Transport.Msmq;
using Warden.Commander.Commands;
using Rebus.Routing.TypeBased;

namespace Warden.Examples.RebusCommander
{
    public class Program
    {
        public static void Main(string[] args)
        {
            using (var activator = new BuiltinHandlerActivator())
            {
                Console.Title = "Warden.Examples.RebusCommander";
                Configure.With(activator)
                    .Logging(l => l.ColoredConsole(minLevel: LogLevel.Debug))
                    .Transport(t => t.UseMsmq("warden-commander"))
                    .Start();

                Console.WriteLine("Type q to quit\n1 to send StopWarden\n2 to send PauseWarden\n3 to send StartWarden");
                var isRunning = true;
                while (isRunning)
                {
                    var key = Console.ReadKey(true);
                    switch (char.ToLower(key.KeyChar))
                    {
                        case '1':
                            Console.WriteLine("Sending StopWarden!");
                            activator.Bus.Publish(new StopWarden()).Wait();
                            break;
                        case '2':
                            Console.WriteLine("Sending PauseWarden!");
                            activator.Bus.Publish(new PauseWarden()).Wait();
                            break;
                        case '3':
                            Console.WriteLine("Sending StartWarden!");
                            activator.Bus.Publish(new StartWarden()).Wait();
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
