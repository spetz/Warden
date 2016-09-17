using System;
using System.Threading.Tasks;
using Rebus.Activation;
using Rebus.Config;
using Rebus.Logging;
using Rebus.Routing.TypeBased;
using Rebus.Transport.Msmq;
using Warden.Core;
using Warden.Manager;
using Warden.Manager.Commands;
using Warden.Utils;
using Warden.Watchers.Server;
using Warden.Watchers.Web;

namespace Warden.Examples.CommandsAndEvents.App
{
    public class Program
    {
        private static readonly RebusWardenCommandSource CommandSource = new RebusWardenCommandSource();
        private static RebusWardenEventHandler _eventHandler;
        private static readonly string CommandsRoute = "Warden.Examples.CommandsAndEvents.Manager";

        public static void Main(string[] args)
        {
            using (var activator = new BuiltinHandlerActivator())
            {
                Console.Title = "Warden.Examples.CommandsAndEvents.App";
                activator.Register((bus, message) => new GenericCommandHandler(CommandSource));
                Configure.With(activator)
                    .Logging(l => l.ColoredConsole(minLevel: LogLevel.Debug))
                    .Transport(t => t.UseMsmq("Warden.Examples.CommandsAndEvents.App"))
                    .Routing(r => r.TypeBased()
                        .Map<PingWarden>(CommandsRoute)
                        .Map<StopWarden>(CommandsRoute)
                        .Map<PauseWarden>(CommandsRoute)
                        .Map<StartWarden>(CommandsRoute)
                        .Map<KillWarden>(CommandsRoute))
                    .Start();

                _eventHandler = new RebusWardenEventHandler(activator.Bus);
                activator.Bus.Subscribe<PingWarden>().Wait();
                activator.Bus.Subscribe<StopWarden>().Wait();
                activator.Bus.Subscribe<PauseWarden>().Wait();
                activator.Bus.Subscribe<StartWarden>().Wait();
                activator.Bus.Subscribe<KillWarden>().Wait();

                var wardenManager = ConfigureWardenManager();
                Task.WaitAll(wardenManager.StartAsync());

                Console.WriteLine("Stopping the bus...");
            }
        }

        private static IWardenManager ConfigureWardenManager()
        {
            var wardenConfiguration = WardenConfiguration
                .Create()
                .AddWebWatcher("http://httpstat.us/200",
                    interval: TimeSpan.FromMilliseconds(1000), group: "websites")
                .AddServerWatcher("www.google.pl", 80)
                .WithConsoleLogger(minLevel: WardenLoggerLevel.All, useColors: true)
                .Build();

            var warden = WardenInstance.Create(wardenConfiguration);

            return WardenManager.Create(warden, cfg =>
            {
                cfg.SetCommandSource(() => CommandSource)
                    .SetEventHandler(() => _eventHandler)
                    .WithConsoleLogger(minLevel: WardenLoggerLevel.All, useColors: true);
            });
        }
    }
}
