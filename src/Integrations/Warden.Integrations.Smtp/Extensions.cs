using System;
using Warden.Core;
using Warden.Integrations;

namespace Warden.Integrations.Smtp
{
    
    public static class Extensions
    {

        public static WardenConfiguration.Builder IntegrateWithSmtp(
            this WardenConfiguration.Builder builder,
            string host,
            int port,
            bool enableSsl,
            Action<SmtpIntegrationConfiguration.Builder> configurator = null)
        {
            builder.AddIntegration(SmtpIntegration.Create(host, port, enableSsl, configurator));

            return builder;
        }

        
        public static WardenConfiguration.Builder IntegrateWithSmtp(
            this WardenConfiguration.Builder builder,
            SmtpIntegrationConfiguration configuration)
        {
            builder.AddIntegration(SmtpIntegration.Create(configuration));

            return builder;
        }

        public static SmtpIntegration Smtp(this IIntegrator integrator)
            => integrator.Resolve<SmtpIntegration>();
    }
}