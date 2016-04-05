using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using NLog;
using Warden.Configurations;
using Warden.Integrations.SendGrid;
using Warden.Watchers;
using Warden.Watchers.MongoDb;
using Warden.Watchers.MsSql;
using Warden.Watchers.Performance;
using Warden.Watchers.Redis;
using Warden.Watchers.Web;

namespace Warden.Examples.Console
{
    class Program
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private static void Main(string[] args)
        {
            var warden = ConfigureWarden();
            Task.WaitAll(warden.StartAsync());
        }

        private static IWarden ConfigureWarden()
        {
            var websiteWatcherConfiguration = WebWatcherConfiguration
                .Create("http://httpstat.us/200")
                .EnsureThat(x => x.IsValid)
                .Build();
            var websiteWatcher = WebWatcher.Create("Website watcher", websiteWatcherConfiguration);

            var mongoDbWatcherConfiguration = MongoDbWatcherConfiguration
                .Create("mongodb://localhost:27017", "MyDatabase")
                .WithQuery("Users", "{\"name\": \"admin\"}")
                .EnsureThat(users => users.Any(user => user.role == "admin"))
                .Build();
            var mongoDbWatcher = MongoDbWatcher.Create("MongoDB watcher", mongoDbWatcherConfiguration);

            var redisWatcherConfiguration = RedisWatcherConfiguration
                .Create("localhost", 1)
                .WithQuery("get test")
                .EnsureThat(results => results.Any(x => x == "test-value"))
                .Build();
            var redisWatcher = RedisWatcher.Create("Redis watcher", redisWatcherConfiguration);

            var mssqlWatcherConfiguration = MsSqlWatcherConfiguration
                .Create(ConfigurationManager.ConnectionStrings["MyDatabase"].ConnectionString)
                .WithQuery("select * from users where id = @id", new Dictionary<string, object> {["id"] = 1})
                .EnsureThat(users => users.Any(user => user.Name == "admin"))
                .Build();
            var mssqlWatcher = MsSqlWatcher.Create("MSSQL watcher", mssqlWatcherConfiguration);

            var apiWatcherConfiguration = WebWatcherConfiguration
                .Create("http://httpstat.us", HttpRequest.Get("200",
                    headers: new Dictionary<string, string>
                    {
                        ["Accept"] = "application/json"
                    }))
                .Build();
            var apiWatcher = WebWatcher.Create("API watcher", apiWatcherConfiguration);

            var sendGridConfiguration = SendGridIntegrationConfiguration
                .Create("api-key", "my@email.com")
                .WithDefaultSubject("Warden monitoring")
                .WithDefaultReceivers("my@email.com")
                .Build();

            var sendGrid = SendGridIntegration.Create(sendGridConfiguration);

            var wardenConfiguration = WardenConfiguration
                .Create()
                .AddPerformanceWatcher(cfg => cfg.EnsureThat(usage => usage.Cpu < 50 && usage.Ram < 5000),
                hooks => hooks.OnCompleted(result => Logger.Info(result.WatcherCheckResult.Description)))
                .AddWebWatcher("http://my-website.com", hooks =>
                {
                    hooks.OnFailureAsync(result => WebsiteHookOnFailureAsync(result));
                    hooks.OnSuccessAsync(result => WebsiteHookOnSuccessAsync(result));
                    hooks.OnCompletedAsync(result => WebsiteHookOnCompletedAsync(result));
                })
                .AddWebWatcher("http://my-api.com", HttpRequest.Post("users", new {name = "test"},
                    headers: new Dictionary<string, string>
                    {
                        ["Authorization"] = "Token MyBase64EncodedString",
                    }),
                    cfg =>
                    {
                        cfg.EnsureThat(response => response.Headers.Any());
                    })
                .AddMongoDbWatcher("mongodb://localhost:27017", "MyDatabase", cfg =>
                {
                    cfg.WithQuery("Users", "{\"name\": \"admin\"}")
                        .EnsureThat(users => users.Any(user => user.role == "admin"));
                })
                .AddMsSqlWatcher(@"Data Source=.\sqlexpress;Initial Catalog=MyDatabase;Integrated Security=True",
                    cfg =>
                    {
                        cfg.WithQuery("select * from users where id = @id", new Dictionary<string, object> {["id"] = 1})
                            .EnsureThat(users => users.Any(user => user.Name == "admin"));
                    })
                .IntegrateWithSendGrid("api-key", "noreply@system.com", cfg =>
                {
                    cfg.WithDefaultSubject("Monitoring status")
                        .WithDefaultReceivers("admin@system.com");
                })
                .SetGlobalWatcherHooks(hooks =>
                {
                    hooks.OnStart(check => GlobalHookOnStart(check))
                        .OnFailure(result => GlobalHookOnFailure(result))
                        .OnSuccess(result => GlobalHookOnSuccess(result))
                        .OnCompleted(result => GlobalHookOnCompleted(result));
                })
                .SetAggregatedWatcherHooks((hooks, integrations) =>
                {
                    hooks.OnFirstFailureAsync(
                        results => integrations.SendGrid().SendEmailAsync("Monitoring errors have occured."))
                        .OnFirstSuccessAsync(
                            results => integrations.SendGrid().SendEmailAsync("Everything is up and running again!"));
                })
                .SetHooks(hooks =>
                {
                    hooks.OnIterationCompleted(iteration => OnIterationCompleted(iteration))
                         .OnError(exception => Logger.Error(exception));
                })
                .Build();

            return new Warden(wardenConfiguration);
        }

        private static string GetExceptionsDetails(IEnumerable<Exception> exceptions)
        {
            if (!exceptions.Any())
                return string.Empty;

            return exceptions.Select(x => x.ToString()).Aggregate((x, y) => $"{x}\n{y}");
        }

        private static string GetWardenCheckResultDetails(IEnumerable<IWardenCheckResult> results)
        {
            if (!results.Any())
                return string.Empty;

            Func<IWardenCheckResult, string> details = result => $"{result.WatcherCheckResult.WatcherName} " +
                                                                 $"({(result.WatcherCheckResult.IsValid ? "valid" : "invalid")}): " +
                                                                 $"{result.WatcherCheckResult.Description}\n";

            return results.Select(details).Aggregate((x, y) => $"{x}\n{y}");
        }

        private static async Task WebsiteHookOnStartAsync(IWatcherCheck check)
        {
            Logger.Info($"Invoking the hook OnStartAsync() by watcher: '{check.WatcherName}'.");
            await Task.CompletedTask;
        }

        private static async Task WebsiteHookOnSuccessAsync(IWardenCheckResult check)
        {
            var websiteWatcherCheckResult = (WebWatcherCheckResult)check.WatcherCheckResult;
            Logger.Info($"Invoking the hook OnSuccessAsync() by watcher: '{websiteWatcherCheckResult.WatcherName}'.");
            await Task.CompletedTask;
        }

        private static async Task WebsiteHookOnCompletedAsync(IWardenCheckResult check)
        {
            Logger.Info($"Invoking the hook OnCompletedAsync() by watcher: '{check.WatcherCheckResult.WatcherName}'.");
            await Task.CompletedTask;
        }

        private static async Task WebsiteHookOnFailureAsync(IWardenCheckResult check)
        {
            Logger.Info($"Invoking the hook OnFailureAsync() by watcher: '{check.WatcherCheckResult.WatcherName}'.");
            await Task.CompletedTask;
        }

        private static void GlobalHookOnStart(IWatcherCheck check)
        {
            Logger.Info($"Invoking the global hook OnStart() by watcher: '{check.WatcherName}'.");
        }

        private static void GlobalHookOnSuccess(IWardenCheckResult check)
        {
            Logger.Info($"Invoking the global hook OnSuccess() by watcher: '{check.WatcherCheckResult.WatcherName}'.");
        }

        private static void GlobalHookOnCompleted(IWardenCheckResult check)
        {
            Logger.Info($"Invoking the global hook OnCompleted() by watcher: '{check.WatcherCheckResult.WatcherName}'.");
        }

        private static void GlobalHookOnFailure(IWardenCheckResult check)
        {
            Logger.Info($"Invoking the global hook OnFailure() by watcher: '{check.WatcherCheckResult.WatcherName}'.");
        }

        private static void OnIterationCompleted(IWardenIteration wardenIteration)
        {
            var newLine = Environment.NewLine;
            Logger.Info($"Warden iteration {wardenIteration.Ordinal} has completed.");
            foreach (var result in wardenIteration.Results)
            {
                Logger.Info($"Watcher: '{result.WatcherCheckResult.WatcherName}'{newLine}" +
                            $"Description: {result.WatcherCheckResult.Description}{newLine}" +
                            $"Is valid: {result.IsValid}{newLine}" +
                            $"Started at: {result.StartedAt}{newLine}" +
                            $"Completed at: {result.CompletedAt}{newLine}" +
                            $"Execution time: {result.ExecutionTime}{newLine}");
            }
        }
    }
}
