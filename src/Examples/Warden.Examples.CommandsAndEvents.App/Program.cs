using System;
using System.Threading.Tasks;
using Rebus.Activation;
using Rebus.Config;
using Rebus.Logging;
using Rebus.Routing.TypeBased;
using Rebus.Transport.Msmq;
using Warden.Commands;
using Warden.Core;
using Warden.Utils;
using Warden.Watchers.Server;
using Warden.Watchers.Web;

namespace Warden.Examples.CommandsAndEvents.App
{
    public class Program
    {
        private static readonly RebusWardenCommandHandler CommandHandler = new RebusWardenCommandHandler();
        private static readonly RebusWardenEventHandler EventHandler = new RebusWardenEventHandler();

        public static void Main(string[] args)
        {
            using (var activator = new BuiltinHandlerActivator())
            {
                Console.Title = "Warden.Examples.CommandsAndEvents.App";
                activator.Register((bus, message) => new GenericCommandHandler(CommandHandler));
                Configure.With(activator)
                    .Logging(l => l.ColoredConsole(minLevel: LogLevel.Debug))
                    .Transport(t => t.UseMsmq("Warden.Examples.CommandsAndEvents.App"))
                    .Routing(r => r.TypeBased()
                        .Map<StopWarden>("Warden.Examples.CommandsAndEvents.Manager")
                        .Map<PauseWarden>("Warden.Examples.CommandsAndEvents.Manager")
                        .Map<StartWarden>("Warden.Examples.CommandsAndEvents.Manager")
                        .Map<KillWarden>("Warden.Examples.CommandsAndEvents.Manager"))
                    .Start();

                activator.Bus.Subscribe<StopWarden>().Wait();
                activator.Bus.Subscribe<PauseWarden>().Wait();
                activator.Bus.Subscribe<StartWarden>().Wait();
                activator.Bus.Subscribe<KillWarden>().Wait();

                var warden = ConfigureWarden();
                Task.WaitAll(warden.StartAsync());

                Console.WriteLine("Stopping the bus...");
            }
        }

        private static IWarden ConfigureWarden()
        {
            var wardenConfiguration = WardenConfiguration
                .Create()
                .AddWebWatcher("http://httpstat.us/200",
                    interval: TimeSpan.FromMilliseconds(1000), group: "websites")
                .AddServerWatcher("www.google.pl", 80)
                .SetCommandHandler(() => CommandHandler)
                .SetEventHandler(() => EventHandler)
                .WithConsoleLogger(minLevel: WardenLoggerLevel.All, useColors: true)
                .Build();

            return WardenInstance.Create(wardenConfiguration);
        }
    }
}
