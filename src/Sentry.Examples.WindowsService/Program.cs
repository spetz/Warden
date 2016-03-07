using Topshelf;

namespace Sentry.Examples.WindowsService
{
    class Program
    {
        private static void Main(string[] args)
        {
            HostFactory.Run(x =>
            {
                x.Service<SentryService>(s =>
                {
                    s.ConstructUsing(name => new SentryService());
                    s.WhenStarted(async tc => await tc.StartAsync());
                    s.WhenStopped(async tc => await tc.StopAsync());
                });
                x.RunAsLocalSystem();
                x.SetDescription("Sentry");
                x.SetDisplayName("Sentry");
                x.SetServiceName("Sentry");
            });
        }
    }
}
