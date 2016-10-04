using System.Linq;
using Topshelf;

namespace Warden.Examples.WindowsService
{
    public class Program
    {
        private const string ServiceName = "Warden";

        public static void Main(string[] args)
        {
            args = args.Where(a => a != ServiceName).ToArray();
            HostFactory.Run(x =>
            {
                x.ApplyCommandLine(string.Join(" ", args));
                x.Service<WardenService>(service =>
                {
                    service.ConstructUsing(name => new WardenService());
                    service.WhenStarted(async warden => await warden.StartAsync());
                    service.WhenPaused(async warden => await warden.PauseAsync());
                    service.WhenContinued(async warden => await warden.StartAsync());
                    service.WhenStopped(async warden => await warden.StopAsync());
                });
                x.RunAsLocalSystem();
                x.SetDescription(ServiceName);
                x.SetDisplayName(ServiceName);
                x.SetServiceName(ServiceName);
            });
        }
    }
}
