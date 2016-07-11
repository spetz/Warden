using System;
using Warden.Core;

namespace Warden.Integrations.MsSql
{
    public static class Extensions
    {
        /// <summary>
        /// Extension method for adding the MS SQL integration to the the WardenConfiguration.
        /// </summary>
        /// <param name="builder">Instance of the Warden configuration builder.</param>
        /// <param name="connectionString">Connection string of the MS SQL server.</param>
        /// <param name="configurator">Optional lambda expression for configuring the MsSqlIntegration.</param>
        /// <returns>Instance of fluent builder for the WardenConfiguration.</returns>
        public static WardenConfiguration.Builder IntegrateWithMsSql(
            this WardenConfiguration.Builder builder,
            string connectionString,
            Action<MsSqlIntegrationConfiguration.Builder> configurator = null)
        {
            builder.AddIntegration(MsSqlIntegration.Create(connectionString, configurator));

            return builder;
        }

        /// <summary>
        /// Extension method for adding the MS SQL integration to the the WardenConfiguration.
        /// </summary>
        /// <param name="builder">Instance of the Warden configuration builder.</param>
        /// <param name="configuration">Configuration of MsSqlIntegration.</param>
        /// <returns>Instance of fluent builder for the WardenConfiguration.</returns>
        public static WardenConfiguration.Builder IntegrateWithMsSql(
            this WardenConfiguration.Builder builder,
            MsSqlIntegrationConfiguration configuration)
        {
            builder.AddIntegration(MsSqlIntegration.Create(configuration));

            return builder;
        }

        /// <summary>
        /// Extension method for resolving the MS SQL integration from the IIntegrator.
        /// </summary>
        /// <param name="integrator">Instance of the IIntegrator.</param>
        /// <returns>Instance of fluent builder for the WardenConfiguration.</returns>
        public static MsSqlIntegration MsSql(this IIntegrator integrator)
            => integrator.Resolve<MsSqlIntegration>();
    }
}