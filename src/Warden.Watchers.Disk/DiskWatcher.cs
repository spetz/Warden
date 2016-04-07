using System;
using System.Threading.Tasks;

namespace Warden.Watchers.Disk
{
    /// <summary>
    /// DiskWatcher designed for disk & file monitoring.
    /// </summary>
    public class DiskWatcher : IWatcher
    {
        private readonly DiskWatcherConfiguration _configuration;
        public string Name { get; }
        public const string DefaultName = "Disk Watcher";

        protected DiskWatcher(string name, DiskWatcherConfiguration configuration)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Watcher name can not be empty.");

            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration),
                    "Disk Watcher configuration has not been provided.");
            }

            Name = name;
            _configuration = configuration;
        }

        public async Task<IWatcherCheckResult> ExecuteAsync()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Factory method for creating a new instance of DiskWatcher with default name of Disk Watcher.
        /// </summary>
        /// <param name="configurator">Optional lambda expression for configuring the DiskWatcher.</param>
        /// <returns>Instance of DiskWatcher.</returns>
        public static DiskWatcher Create(Action<DiskWatcherConfiguration.Default> configurator = null)
        {
            var config = new DiskWatcherConfiguration.Builder();
            configurator?.Invoke((DiskWatcherConfiguration.Default) config);

            return Create(DefaultName, config.Build());
        }

        /// <summary>
        /// Factory method for creating a new instance of DiskWatcher.
        /// </summary>
        /// <param name="name">Name of the DiskWatcher.</param>
        /// <param name="configurator">Optional lambda expression for configuring the DiskWatcher.</param>
        /// <returns>Instance of DiskWatcher.</returns>
        public static DiskWatcher Create(string name,
            Action<DiskWatcherConfiguration.Default> configurator = null)
        {
            var config = new DiskWatcherConfiguration.Builder();
            configurator?.Invoke((DiskWatcherConfiguration.Default) config);

            return Create(name, config.Build());
        }

        /// <summary>
        /// Factory method for creating a new instance of DiskWatcher with default name of Disk DiskWatcher.
        /// </summary>
        /// <param name="configuration">Configuration of DiskWatcher.</param>
        /// <returns>Instance of DiskWatcher.</returns>
        public static DiskWatcher Create(DiskWatcherConfiguration configuration)
            => Create(DefaultName, configuration);

        /// <summary>
        /// Factory method for creating a new instance of DiskWatcher.
        /// </summary>
        /// <param name="name">Name of the DiskWatcher.</param>
        /// <param name="configuration">Configuration of DiskWatcher.</param>
        /// <returns>Instance of DiskWatcher.</returns>
        public static DiskWatcher Create(string name, DiskWatcherConfiguration configuration)
            => new DiskWatcher(name, configuration);
    }
}