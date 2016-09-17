using System;
using Warden.Manager.Commands;
using Warden.Manager.Events;
using Warden.Utils;

namespace Warden.Manager
{
    /// <summary>
    /// Configuration of the Warden Manager.
    /// </summary>
    public class WardenManagerConfiguration
    {
        /// <summary>
        /// Custom provider for the IWardenCommandHandler.
        /// </summary>
        public Func<IWardenCommandSource> WardenCommandSourceProvider { get; protected set; }

        /// <summary>
        /// Custom provider for the IWardenEventHandler.
        /// </summary>
        public Func<IWardenEventHandler> WardenEventHandlerProvider { get; protected set; }

        /// <summary>
        /// Custom provider for the IWardenLogger.
        /// </summary>
        public Func<IWardenLogger> WardenLoggerProvider { get; protected set; }

        protected internal WardenManagerConfiguration()
        {
            WardenCommandSourceProvider = () => new EmptyWardenCommandSource();
            WardenEventHandlerProvider = () => new EmptyWardenEventHandler();
            WardenLoggerProvider = () => new EmptyWardenLogger();
        }

        /// <summary>
        /// Fluent builder for the WardenManagerConfiguration.
        /// </summary>
        public class Builder
        {
            private readonly WardenManagerConfiguration _configuration = new WardenManagerConfiguration();

            /// <summary>
            /// Sets the custom provider for IWardenCommandSource.
            /// </summary>
            /// <param name="wardenCommandSourceProvider">Custom provider for IWardenCommandSource.</param>
            /// <returns>Instance of fluent builder for the WardenManagerConfiguration.</returns>
            public Builder SetCommandSource(Func<IWardenCommandSource> wardenCommandSourceProvider)
            {
                if (wardenCommandSourceProvider == null)
                {
                    throw new ArgumentNullException(nameof(wardenCommandSourceProvider),
                        "Warden command source can not be null.");

                }

                _configuration.WardenCommandSourceProvider = wardenCommandSourceProvider;

                return this;
            }

            /// <summary>
            /// Sets the custom provider for IWardenEventHandler.
            /// </summary>
            /// <param name="wardenEventHandlerProvider">Custom provider for IWardenEventHandler.</param>
            /// <returns>Instance of fluent builder for the WardenManagerConfiguration.</returns>
            public Builder SetEventHandler(Func<IWardenEventHandler> wardenEventHandlerProvider)
            {
                if (wardenEventHandlerProvider == null)
                {
                    throw new ArgumentNullException(nameof(wardenEventHandlerProvider),
                        "Warden event handler can not be null.");

                }

                _configuration.WardenEventHandlerProvider = wardenEventHandlerProvider;

                return this;
            }

            /// <summary>
            /// Sets the custom provider for IWardenLogger.
            /// </summary>
            /// <param name="wardenLoggerProvider">Custom provider for IWardenLogger.</param>
            /// <returns>Instance of fluent builder for the WardenManagerConfiguration.</returns>
            public Builder SetLogger(Func<IWardenLogger> wardenLoggerProvider)
            {
                if (wardenLoggerProvider == null)
                    throw new ArgumentNullException(nameof(wardenLoggerProvider), "Warden manager logger can not be null.");

                _configuration.WardenLoggerProvider = wardenLoggerProvider;

                return this;
            }

            /// <summary>
            /// Sets the Console Logger for Warden Manager.
            /// <param name="minLevel">Minimal level of the messages that will be logged (all by default).</param>
            ///  /// <param name="useColors">Flag determining whether to use colors for different levels (true by default).</param>
            /// </summary>
            /// <returns>Instance of fluent builder for the WardenManagerConfiguration.</returns>
            public Builder WithConsoleLogger(WardenLoggerLevel minLevel = WardenLoggerLevel.All, bool useColors = true)
            {
                _configuration.WardenLoggerProvider = () => new ConsoleWardenLogger(minLevel, useColors);

                return this;
            }

            /// <summary>
            /// Builds the WardenManagerConfiguration and return its instance.
            /// </summary>
            /// <returns>Instance of WardenManagerConfiguration.</returns>
            public WardenManagerConfiguration Build() => _configuration;
        }
    }
}