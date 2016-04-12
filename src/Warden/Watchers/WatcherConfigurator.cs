namespace Warden.Watchers
{
    /// <summary>
    /// Base class for defining the configuration builders for the watchers.
    /// </summary>
    /// <typeparam name="TConfigurator">Type of fluent builder configurator.</typeparam>
    /// <typeparam name="TConfiguration">Type of fluent builder configuration.</typeparam>
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