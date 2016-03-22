namespace Sentry.Core
{

    public abstract class WatcherConfigurator<TConfigurator, TConfiguration>
    {
        protected TConfigurator Configurator { get; set; }
        protected TConfiguration Configuration { get; set; }

        protected WatcherConfigurator()
        {
        }

        protected WatcherConfigurator(TConfiguration configuration)
        {
            Configuration = configuration;
        }

        protected void SetInstance(TConfigurator configurator)
        {
            Configurator = configurator;
        }
    }
}