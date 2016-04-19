using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Http;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Newtonsoft.Json.Serialization;
using Warden.Web.Core.Mongo;
using Warden.Web.Core.Services;

namespace Warden.Web
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            services.AddMvcCore().AddJsonFormatters(formatter =>
                formatter.ContractResolver = new CamelCasePropertyNamesContractResolver());
            services.AddScoped<IWardenIterationService, WardenIterationService>();
            services.AddSingleton<IEncrypter>(provider => new Encrypter("abcd"));
            services.AddSingleton(provider => new MongoClient("mongodb://localhost:27017"));
            services.AddScoped(provider => provider.GetService<MongoClient>().GetDatabase("Warden"));
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseIISPlatformHandler();
            app.UseMvcWithDefaultRoute();
            app.UseStaticFiles();
            app.UseCookieAuthentication((cookieOptions) =>
            {
                cookieOptions.CookieName = ".Warden";
                cookieOptions.LoginPath = new PathString("/login");
                cookieOptions.LoginPath = new PathString("/logout");
            });
            MongoConfigurator.Initialize();
        }

        public static void Main(string[] args) => WebApplication.Run<Startup>(args);
    }
}
