using System;
using Microsoft.AspNet.Builder;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Newtonsoft.Json.Serialization;
using Warden.Web.Core.Mongo;
using Warden.Web.Core.Services;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.SignalR;
using Microsoft.Extensions.Logging;
using Microsoft.Owin.Builder;
using Newtonsoft.Json;
using Owin;
using Warden.Web.Hubs;

namespace Warden.Web
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            services.AddMvcCore().AddJsonFormatters(formatter =>
            {
                formatter.DateFormatString = "yyyy-MM-dd H:mm:ss";
                formatter.Formatting = Formatting.Indented;
                formatter.ContractResolver = new CamelCasePropertyNamesContractResolver();
            });
            services.AddCaching();
            services.AddSession();
            //services.AddSignalR();
            services.AddScoped<IWardenService, WardenService>();
            services.AddScoped<IWatcherService, WatcherService>();
            services.AddScoped<IApiKeyService, ApiKeyService>();
            services.AddScoped<IOrganizationService, OrganizationService>();
            services.AddScoped<IUserService, UserService>();
            services.AddSingleton<IStatsCalculator, StatsCalculator>();
            services.AddSingleton<IEncrypter>(provider => new Encrypter("abcd"));
            services.AddSingleton(provider => new MongoClient("mongodb://localhost:27017"));
            services.AddScoped(provider => provider.GetService<MongoClient>().GetDatabase("Warden"));
            services.AddScoped<ISignalRService, SignalRService>(provider =>
                new SignalRService(GlobalHost.ConnectionManager.GetHubContext<WardenHub>()));
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, IServiceProvider serviceProvider)
        {
            app.UseDeveloperExceptionPage();
            app.UseIISPlatformHandler();
            app.UseStaticFiles();
            app.UseCookieAuthentication(options =>
            {
                options.AutomaticAuthenticate = true;
                options.AutomaticChallenge = true;
                options.CookieName = ".Warden";
                options.LoginPath = "/login";
                options.LogoutPath = "/logout";
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
