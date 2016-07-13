using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Config;
using NLog.Extensions.Logging;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Warden.Web.Extensions
{
    public static class AspNetExtensions
    {
        public static ILoggerFactory AddNLog(this ILoggerFactory factory, IConfigurationRoot configuration)
        {
            //ignore this
            LogManager.AddHiddenAssembly(typeof(AspNetExtensions).GetTypeInfo().Assembly);

            using (var provider = new NLogLoggerProvider())
            {
                factory.AddProvider(provider);
            }

            var configFilePath = "nlog.config";
            if (string.IsNullOrEmpty(configFilePath))
                throw new NLogConfigurationException("Not found NLog config path. Did you add NLog config?");
            ConfigureNLog(configFilePath);

            return factory;
        }

        private static void ConfigureNLog(string fileName)
        {
            LogManager.Configuration = new XmlLoggingConfiguration(fileName, true);
        }
    }
}