using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Warden.Watchers.Disk
{
    /// <summary>
    /// Configuration of the DiskWatcher.
    /// </summary>
    public class DiskWatcherConfiguration
    {
        /// <summary>
        /// Collection of partition names that should be checked.
        /// </summary>
        public IEnumerable<string> PartitionsToCheck { get; protected set; }

        /// <summary>
        /// Collection of directory paths that should be checked.
        /// </summary>
        public IEnumerable<string> DirectoriesToCheck { get; protected set; }

        /// <summary>
        /// Collection of file paths that should be checked.
        /// </summary>
        public IEnumerable<string> FilesToCheck { get; protected set; }

        /// <summary>
        /// Predicate that has to be satisfied in order to return the valid result.
        /// </summary>
        public Func<DiskCheck, bool> EnsureThat { get; protected set; }

        /// <summary>
        /// Async predicate that has to be satisfied in order to return the valid result.
        /// </summary>
        public Func<DiskCheck, Task<bool>> EnsureThatAsync { get; protected set; }

        /// <summary>
        /// Custom provider for the IDiskChecker.
        /// </summary>
        public Func<IDiskChecker> DiskCheckerProvider { get; protected set; }

        protected internal DiskWatcherConfiguration()
        {
            DiskCheckerProvider = () => new DiskChecker();
            PartitionsToCheck = Enumerable.Empty<string>();
            DirectoriesToCheck = Enumerable.Empty<string>();
            FilesToCheck = Enumerable.Empty<string>();
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

            public T WithPartitionsToCheck(params string[] partitions)
            {
                if (partitions == null || partitions.Any() == false || partitions.Any(string.IsNullOrWhiteSpace))
                    throw new ArgumentException("Partitions to check can not be empty.", nameof(partitions));

                Configuration.PartitionsToCheck = partitions;

                return Configurator;
            }

            public T WithDirectoriesToCheck(params string[] directories)
            {
                if (directories == null || directories.Any() == false || directories.Any(string.IsNullOrWhiteSpace))
                    throw new ArgumentException("Directories to check can not be empty.", nameof(directories));

                Configuration.DirectoriesToCheck = directories;

                return Configurator;
            }

            public T WithFilesToCheck(params string[] files)
            {
                if (files == null || files.Any() == false || files.Any(string.IsNullOrWhiteSpace))
                    throw new ArgumentException("Files to check can not be empty.", nameof(files));

                Configuration.FilesToCheck = files;

                return Configurator;
            }

            /// <summary>
            /// Sets the predicate that has to be satisfied in order to return the valid result.
            /// </summary>
            /// <param name="ensureThat">Lambda expression predicate.</param>
            /// <returns>Instance of fluent builder for the DiskWatcherConfiguration.</returns>
            public T EnsureThat(Func<DiskCheck, bool> ensureThat)
            {
                if (ensureThat == null)
                    throw new ArgumentException("Ensure that predicate can not be null.", nameof(ensureThat));

                Configuration.EnsureThat = ensureThat;

                return Configurator;
            }

            /// <summary>
            /// Sets the async predicate that has to be satisfied in order to return the valid result.
            /// </summary>
            /// <param name="ensureThat">Lambda expression predicate.</param>
            /// <returns>Instance of fluent builder for the DiskWatcherConfiguration.</returns>
            public T EnsureThatAsync(Func<DiskCheck, Task<bool>> ensureThat)
            {
                if (ensureThat == null)
                    throw new ArgumentException("Ensure that async predicate can not be null.", nameof(ensureThat));

                Configuration.EnsureThatAsync = ensureThat;

                return Configurator;
            }

            /// <summary>
            /// Sets the custom provider for the IDiskChecker.
            /// </summary>
            /// <param name="diskCheckerProvider">Custom provider for the IDiskChecker.</param>
            /// <returns>Lambda expression returning an instance of the IDiskChecker.</returns>
            /// <returns>Instance of fluent builder for the DiskWatcherConfiguration.</returns>
            public T WithDiskCheckerProvider(Func<IDiskChecker> diskCheckerProvider)
            {
                if (diskCheckerProvider == null)
                    throw new ArgumentNullException(nameof(diskCheckerProvider), "Disk checker provider can not be null.");

                Configuration.DiskCheckerProvider = diskCheckerProvider;

                return Configurator;
            }
        }

        /// <summary>
        /// Default DiskWatcherConfiguration fluent builder used while configuring watcher via lambda expression.
        /// </summary>
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