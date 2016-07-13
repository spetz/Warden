using System;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Newtonsoft.Json.Serialization;
using Warden.Web.Core.Mongo;
using Warden.Web.Core.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNet.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.PlatformAbstractions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.Server.IISIntegration;
using Microsoft.Owin.Builder;
using Newtonsoft.Json;
using NLog;
using NLog.Extensions.Logging;
using Owin;
using Warden.Web.Core.Factories;
using Warden.Web.Core.Settings;
using Warden.Web.Extensions;
using Warden.Web.Framework;
using Warden.Web.Framework.Filters;
using Warden.Web.Hubs;

namespace Warden.Web
{
    public class Startup
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private IConfigurationRoot Configuration { get; }

        public Startup(IHostingEnvironment env)
        {
            Configuration = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("config.json")
                .AddJsonFile($"config.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            env.ConfigureNLog("nlog.config");
        }

        public void ConfigureServices(IServiceCollection services)
        {
            GeneralSettings settings = GetConfigurationValue<GeneralSettings>("general");

            services.AddOptions();

            services.Configure<FeatureSettings>(Configuration.GetSection("feature"));
            services.Configure<GeneralSettings>(Configuration.GetSection("general"));
            services.Configure<EmailSettings>(Configuration.GetSection("email"));
            services.Configure<AccountSettings>(Configuration.GetSection("account"));
            services.AddMvc(options =>
            {
                options.Filters.Add(new ExceptionFilter());
            });
            services.AddMvcCore().AddJsonFormatters(formatter =>
            {
                formatter.DateFormatString = settings.JsonFormatDate;
                formatter.Formatting = Formatting.Indented;
                formatter.ContractResolver = new CamelCasePropertyNamesContractResolver();
            });
            services.AddMemoryCache();
            services.AddSession();
            services.AddScoped<IWardenService, WardenService>();
            services.AddScoped<IWatcherService, WatcherService>();
            services.AddScoped<IApiKeyService, ApiKeyService>();
            services.AddScoped<IOrganizationService, OrganizationService>();
            services.AddScoped<IUserService, UserService>();
            services.AddSingleton<IStatsCalculator, StatsCalculator>();
            services.AddSingleton<IEmailSender, SendGridEmailSender>();
            services.AddSingleton<ISecuredOperationFactory, SecuredOperationFactory>();
            services.AddSingleton(provider => GetConfigurationValue<FeatureSettings>("feature"));
            services.AddSingleton(provider => GetConfigurationValue<GeneralSettings>("general"));
            services.AddSingleton(provider => GetConfigurationValue<EmailSettings>("email"));
            services.AddSingleton(provider => GetConfigurationValue<AccountSettings>("account"));
            services.AddSingleton<IEncrypter>(provider => new Encrypter(settings.EncrypterKey));
            services.AddSingleton(provider => new MongoClient(settings.ConnectionString));
            services.AddScoped(provider => provider.GetService<MongoClient>().GetDatabase(settings.Database));
            services.AddScoped<ISignalRService, SignalRService>(provider =>
                new SignalRService(GlobalHost.ConnectionManager.GetHubContext<WardenHub>()));
        }

        private T GetConfigurationValue<T>(string section) where T : new()
        {
            T val = new T();
            Configuration.GetSection(section).Bind(val);
            return val;
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, 
            ILoggerFactory loggerFactory, IServiceProvider serviceProvider, GeneralSettings settings)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            loggerFactory.AddNLog(Configuration);
            app.UseStaticFiles();
            app.UseCookieAuthentication(new CookieAuthenticationOptions()
            {
                AutomaticAuthenticate = true,
                AutomaticChallenge = true,
                CookieName = settings.AuthCookieName,
                LoginPath = settings.LoginPath,
                LogoutPath = settings.LogoutPath
            });
            app.UseSession();
            app.UseMvcWithDefaultRoute();
            app.UseDeveloperExceptionPage();
            MapSignalR(app, serviceProvider);
            MongoConfigurator.Initialize();
            Logger.Info("Application has started.");
        }

        //Issues with Signalr 3 groups - using the version 2 for now.
        private void MapSignalR(IApplicationBuilder app, IServiceProvider serviceProvider)
        {
            app.UseOwin(addToPipeline =>
            {
                addToPipeline(next =>
                {
                    var appBuilder = new AppBuilder();
                    appBuilder.Properties["builder.DefaultApp"] = next;

                    appBuilder.MapSignalR();

                    return appBuilder.Build();
                });
            });

            GlobalHost.DependencyResolver.Register(typeof(WardenHub), () =>
                new WardenHub(serviceProvider.GetService<IUserService>(),
                    serviceProvider.GetService<IOrganizationService>()));

            //SignalR camelCase JSON resolver does not seem to be working.
        }

        public static void Main(string[] args)
        {
            var host = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseStartup<Startup>()
                .Build();

            host.Run();
        }
    }
}
