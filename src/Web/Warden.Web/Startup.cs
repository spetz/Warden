using Microsoft.AspNet.Builder;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Newtonsoft.Json.Serialization;
using Warden.Web.Core.Mongo;
using Warden.Web.Core.Services;
using Microsoft.AspNet.Hosting;
using Microsoft.Extensions.Logging;

namespace Warden.Web
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            services.AddMvcCore().AddJsonFormatters(formatter =>
                formatter.ContractResolver = new CamelCasePropertyNamesContractResolver());
            services.AddCaching();
            services.AddSession();
            services.AddSignalR();
            services.AddScoped<IWardenIterationService, WardenIterationService>();
            services.AddScoped<IApiKeyService, ApiKeyService>();
            services.AddScoped<IOrganizationService, OrganizationService>();
            services.AddScoped<IUserService, UserService>();
            services.AddSingleton<IEncrypter>(provider => new Encrypter("abcd"));
            services.AddSingleton(provider => new MongoClient("mongodb://localhost:27017"));
            services.AddScoped(provider => provider.GetService<MongoClient>().GetDatabase("Warden"));
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
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
            app.UseSignalR();
            app.UseMvcWithDefaultRoute();
            MongoConfigurator.Initialize();
        }

        public static void Main(string[] args) => WebApplication.Run<Startup>(args);
    }
}
