using Topshelf;

namespace Warden.Examples.WindowsService
{
    class Program
    {
        private static void Main(string[] args)
        {
            HostFactory.Run(x =>
            {
                x.Service<WardenService>(service =>
                {
                    service.ConstructUsing(name => new WardenService());
                    service.WhenStarted(async warden => await warden.StartAsync());
                    service.WhenPaused(async warden => await warden.PauseAsync());
                    service.WhenContinued(async warden => await warden.StartAsync());
                    service.WhenStopped(async warden => await warden.StopAsync());
                });
                x.RunAsLocalSystem();
                x.SetDescription("Warden");
                x.SetDisplayName("Warden");
                x.SetServiceName("Warden");
            });
        }
    }
}
