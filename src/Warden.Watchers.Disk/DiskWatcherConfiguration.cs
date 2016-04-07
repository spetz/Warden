namespace Warden.Watchers.Disk
{
    /// <summary>
    /// Configuration of the DiskWatcher.
    /// </summary>
    public class DiskWatcherConfiguration
    {
        protected internal DiskWatcherConfiguration()
        {
        }

        /// <summary>
        /// Factory method for creating a new instance of fluent builder for the DiskWatcherConfiguration.
        /// </summary>
        public static Builder Create() => new Builder();

        /// <summary>
        /// Fluent builder for the DiskWatcherConfiguration.
        /// </summary>
        public abstract class Configurator<T> : WatcherConfigurator<T, DiskWatcherConfiguration>
            where T : Configurator<T>
        {
            protected Configurator()
            {
                Configuration = new DiskWatcherConfiguration();
            }

            protected Configurator(DiskWatcherConfiguration configuration) : base(configuration)
            {
            }
        }

        public class Default : Configurator<Default>
        {
            public Default(DiskWatcherConfiguration configuration) : base(configuration)
            {
                SetInstance(this);
            }
        }

        /// <summary>
        /// Extended DiskWatcherConfiguration fluent builder used while configuring watcher directly.
        /// </summary>
        public class Builder : Configurator<Builder>
        {
            public Builder() : base()
            {
                SetInstance(this);
            }

            /// <summary>
            /// Builds the DiskWatcherConfiguration and return its instance.
            /// </summary>
            /// <returns>Instance of DiskWatcherConfiguration.</returns>
            public DiskWatcherConfiguration Build() => Configuration;

            /// <summary>
            /// Operator overload to provide casting the Builder configurator into Default configurator.
            /// </summary>
            /// <param name="builder">Instance of extended Builder configurator.</param>
            /// <returns>Instance of Default builder configurator.</returns>
            public static explicit operator Default(Builder builder) => new Default(builder.Configuration);
        }
    }
}