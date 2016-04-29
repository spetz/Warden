using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Config;
using NLog.Extensions.Logging;
using System.Reflection;

namespace Warden.Web.Extensions
{
    public static class AspNetExtensions
    {
        private const string NLogPathKey = "NLOG_PATH";
        private class NLogConfigurationProvider : ConfigurationProvider
        {
            internal NLogConfigurationProvider(string key, string path)
            {
                Data[key] = path;
            }
            public override void Set(string key, string value) { }
        }

        public static IConfigurationBuilder AddNLogConfig(this IConfigurationBuilder configurationBuilder, string configFileRelativePath)
        {
            var fullPathToConfigFile = Path.Combine(configurationBuilder.GetBasePath(), configFileRelativePath);
            var provider = new NLogConfigurationProvider(NLogPathKey, fullPathToConfigFile);
            configurationBuilder.Add(provider);
            return configurationBuilder;
        }

        public static ILoggerFactory AddNLog(this ILoggerFactory factory, IConfigurationRoot configuration)
        {
            //ignore this
            LogManager.AddHiddenAssembly(typeof(AspNetExtensions).GetTypeInfo().Assembly);

            using (var provider = new NLogLoggerProvider())
            {
                factory.AddProvider(provider);
            }

            var configFilePath = configuration[NLogPathKey];
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