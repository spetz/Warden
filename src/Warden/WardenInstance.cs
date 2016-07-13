using System;
using System.Runtime.InteropServices;
using Warden.Core;

namespace Warden
{
    /// <summary>
    /// Builder of the Warden instance.
    /// </summary>
    public static class WardenInstance
    {
        private const string UnixComputerNameEnvironmentVariable = "HOSTNAME";
        private const string WindowsComputerNameEnvironmentVariable = "COMPUTERNAME";

        public static string DefaultName = GetDefaultName();

        private static string GetDefaultName()
        {
            var environment = string.Empty;
#if NET461
            environment = Environment.GetEnvironmentVariable(WindowsComputerNameEnvironmentVariable);
#else
            environment = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? Environment.GetEnvironmentVariable(WindowsComputerNameEnvironmentVariable)
                : Environment.GetEnvironmentVariable(UnixComputerNameEnvironmentVariable);
#endif

            return $"Warden @{environment}";
        }

        /// <summary>
        /// Factory method for creating a new Warden instance with provided configuration and default name of "Warden @{COMPUTER NAME}".
        /// </summary>
        /// <param name="configuration">Configuration of Warden.</param>
        /// <returns>Instance of IWarden.</returns>
        public static IWarden Create(WardenConfiguration configuration) => Create(DefaultName, configuration);

        /// <summary>
        /// Factory method for creating a new Warden instance with provided configuration.
        /// </summary>
        /// <param name="name">Name of the Warden.</param>
        /// <param name="configuration">Configuration of Warden.</param>
        /// <returns>Instance of IWarden.</returns>
        public static IWarden Create(string name, WardenConfiguration configuration) => new Warden(name, configuration);

        /// <summary>
        /// Factory method for creating a new Warden instance with default name of "Warden @{COMPUTER NAME}",
        /// for which the configuration can be provided via the lambda expression.
        /// </summary>
        /// <param name="configurator">Lambda expression to build the configuration of Warden.</param>
        /// <returns>Instance of IWarden.</returns>
        public static IWarden Create(Action<WardenConfiguration.Builder> configurator) => Create(DefaultName, configurator);

        /// <summary>
        /// Factory method for creating a new Warden instance, for which the configuration can be provided via the lambda expression.
        /// </summary>
        /// <param name="name">Name of the Warden.</param>
        /// <param name="configurator">Lambda expression to build the configuration of Warden.</param>
        /// <returns>Instance of IWarden.</returns>
        public static IWarden Create(string name, Action<WardenConfiguration.Builder> configurator)
        {
            var config = new WardenConfiguration.Builder();
            configurator?.Invoke(config);

            return Create(name, config.Build());
        }
    }
}