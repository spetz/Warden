using Topshelf;

namespace Sentry.Examples.WindowsService
{
    class Program
    {
        private static void Main(string[] args)
        {
            HostFactory.Run(x =>
            {
                x.Service<SentryService>(service =>
                {
                    service.ConstructUsing(name => new SentryService());
                    service.WhenStarted(async sentry => await sentry.StartAsync());
                    service.WhenPaused(async sentry => await sentry.PauseAsync());
                    service.WhenContinued(async sentry => await sentry.StartAsync());
                    service.WhenStopped(async sentry => await sentry.StopAsync());
                });
                x.RunAsLocalSystem();
                x.SetDescription("Sentry");
                x.SetDisplayName("Sentry");
                x.SetServiceName("Sentry");
            });
        }
    }
}
