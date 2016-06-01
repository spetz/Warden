using System;
using System.Threading.Tasks;

namespace Warden.Watchers.Process
{
    /// <summary>
    /// Configuration of the ProcessWatcher.
    /// </summary>
    public class ProcessWatcherConfiguration
    {
        /// <summary>
        /// Name of the process.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Flag determining whether the existing but not running process should be treated as valid one.
        /// </summary>
        public bool SkipStateValidation { get; protected set; }

        /// <summary>
        /// Predicate that has to be satisfied in order to return the valid result.
        /// </summary>
        public Func<ProcessInfo, bool> EnsureThat { get; protected set; }

        /// <summary>
        /// Async predicate that has to be satisfied in order to return the valid result.
        /// </summary>
        public Func<ProcessInfo, Task<bool>> EnsureThatAsync { get; protected set; }

        /// <summary>
        /// Custom provider for the IProcessService.
        /// </summary>
        public Func<IProcessService> ProcessServiceProvider { get; protected set; }

        protected internal ProcessWatcherConfiguration(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Process name can not be empty.", nameof(name));

            Name = name;
            ProcessServiceProvider = () => new ProcessService();
        }

        /// <summary>
        /// Factory method for creating a new instance of fluent builder for the ProcessWatcherConfiguration.
        /// </summary>
        /// <param name="name">Name of the process.</param>
        /// <returns>Instance of fluent builder for the ProcessWatcherConfiguration.</returns>
        public static Builder Create(string name) => new Builder(name);

        public abstract class Configurator<T> : WatcherConfigurator<T, ProcessWatcherConfiguration>
            where T : Configurator<T>
        {
            protected Configurator(string name)
            {
                Configuration = new ProcessWatcherConfiguration(name);
            }

            protected Configurator(ProcessWatcherConfiguration configuration) : base(configuration)
            {
            }

            /// <summary>
            /// Skips the validation of the state for existing process.
            /// </summary>
            /// <returns>Instance of fluent builder for the ProcessWatcherConfiguration.</returns>
            public T SkipStateValidation()
            {
                Configuration.SkipStateValidation = true;

                return Configurator;
            }

            /// <summary>
            /// Sets the predicate that has to be satisfied in order to return the valid result.
            /// </summary>
            /// <param name="ensureThat">Lambda expression predicate.</param>
            /// <returns>Instance of fluent builder for the ProcessWatcherConfiguration.</returns>
            public T EnsureThat(Func<ProcessInfo, bool> ensureThat)
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
            /// <returns>Instance of fluent builder for the ProcessWatcherConfiguration.</returns>
            public T EnsureThatAsync(Func<ProcessInfo, Task<bool>> ensureThat)
            {
                if (ensureThat == null)
                    throw new ArgumentException("Ensure that async predicate can not be null.", nameof(ensureThat));

                Configuration.EnsureThatAsync = ensureThat;

                return Configurator;
            }

            /// <summary>
            /// Sets the custom provider for the IProcessService.
            /// </summary>
            /// <param name="processServiceProvider">Custom provider for the IProcessService.</param>
            /// <returns>Lambda expression returning an instance of the IProcessService.</returns>
            /// <returns>Instance of fluent builder for the ProcessWatcherConfiguration.</returns>
            public T WithProcessServiceProvider(Func<IProcessService> processServiceProvider)
            {
                if (processServiceProvider == null)
                {
                    throw new ArgumentNullException(nameof(processServiceProvider),
                        "IProcessService provider can not be null.");
                }

                Configuration.ProcessServiceProvider = processServiceProvider;

                return Configurator;
            }
        }

        /// <summary>
        /// Default ProcessWatcherConfiguration fluent builder used while configuring watcher via lambda expression.
        /// </summary>
        public class Default : Configurator<Default>
        {
            public Default(ProcessWatcherConfiguration configuration) : base(configuration)
            {
                SetInstance(this);
            }
        }

        /// <summary>
        /// Extended ProcessWatcherConfiguration fluent builder used while configuring watcher directly.
        /// </summary>
        public class Builder : Configurator<Builder>
        {
            public Builder(string name) : base(name)
            {
                SetInstance(this);
            }

            /// <summary>
            /// Builds the ProcessWatcherConfiguration and return its instance.
            /// </summary>
            /// <returns>Instance of ProcessWatcherConfiguration.</returns>
            public ProcessWatcherConfiguration Build() => Configuration;

            /// <summary>
            /// Operator overload to provide casting the Builder configurator into Default configurator.
            /// </summary>
            /// <param name="builder">Instance of extended Builder configurator.</param>
            /// <returns>Instance of Default builder configurator.</returns>
            public static explicit operator Default(Builder builder) => new Default(builder.Configuration);
        }
    }
}