using System;
using Microsoft.AspNet.Builder;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Newtonsoft.Json.Serialization;
using Warden.Web.Core.Mongo;
using Warden.Web.Core.Services;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.OptionsModel;
using Microsoft.Owin.Builder;
using Newtonsoft.Json;
using Owin;
using Warden.Web.Core.Settings;
using Warden.Web.Framework;
using Warden.Web.Hubs;

namespace Warden.Web
{
    public class Startup
    {
        private IConfiguration Configuration { get; }

        public Startup(IHostingEnvironment env)
        {
            Configuration = new ConfigurationBuilder()
                .AddJsonFile("config.json")
                .AddJsonFile($"config.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables()
                .Build();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            var settings = Configuration.GetSection("general").Get<GeneralSettings>();
            services.AddOptions();
            services.Configure<FeatureSettings>(Configuration.GetSection("feature"));
            services.Configure<GeneralSettings>(Configuration.GetSection("general"));
            services.AddMvc();
            services.AddMvcCore().AddJsonFormatters(formatter =>
            {
                formatter.DateFormatString = settings.JsonFormatDate;
                formatter.Formatting = Formatting.Indented;
                formatter.ContractResolver = new CamelCasePropertyNamesContractResolver();
            });
            services.AddCaching();
            services.AddSession();
            services.AddScoped<IWardenService, WardenService>();
            services.AddScoped<IWatcherService, WatcherService>();
            services.AddScoped<IApiKeyService, ApiKeyService>();
            services.AddScoped<IOrganizationService, OrganizationService>();
            services.AddScoped<IUserService, UserService>();
            services.AddSingleton<IStatsCalculator, StatsCalculator>();
            services.AddSingleton(provider => Configuration.GetSection("feature").Get<FeatureSettings>());
            services.AddSingleton(provider => Configuration.GetSection("general").Get<GeneralSettings>());
            services.AddSingleton<IEncrypter>(provider => new Encrypter(settings.EncrypterKey));
            services.AddSingleton(provider => new MongoClient(settings.ConnectionString));
            services.AddScoped(provider => provider.GetService<MongoClient>().GetDatabase(settings.Database));
            services.AddScoped<ISignalRService, SignalRService>(provider =>
                new SignalRService(GlobalHost.ConnectionManager.GetHubContext<WardenHub>()));
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, 
            IServiceProvider serviceProvider, GeneralSettings settings)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseIISPlatformHandler();
            app.UseStaticFiles();
            app.UseCookieAuthentication(options =>
            {
                options.AutomaticAuthenticate = true;
                options.AutomaticChallenge = true;
                options.CookieName = settings.AuthCookieName;
                options.LoginPath = settings.LoginPath;
                options.LogoutPath = settings.LogoutPath;
            });
            app.UseSession();
            app.UseMvcWithDefaultRoute();
            MapSignalR(app, serviceProvider);
            MongoConfigurator.Initialize();
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

        public static void Main(string[] args) => WebApplication.Run<Startup>(args);
    }
}
