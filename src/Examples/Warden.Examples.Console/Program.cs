using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Warden.Configurations;
using Warden.Integrations.SendGrid;
using Warden.Watchers;
using Warden.Watchers.Disk;
using Warden.Watchers.MongoDb;
using Warden.Watchers.MsSql;
using Warden.Watchers.Performance;
using Warden.Watchers.Redis;
using Warden.Watchers.Web;

namespace Warden.Examples.Console
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var warden = ConfigureWarden();
            Task.WaitAll(warden.StartAsync());
        }

        private static IWarden ConfigureWarden()
        {
            var wardenConfiguration = WardenConfiguration
                .Create()
                .AddDiskWatcher(cfg =>
                {
                    cfg.WithFilesToCheck(@"D:\Test\File1.txt", @"D:\Test\File2.txt")
                        .WithPartitionsToCheck("D", @"E:\")
                        .WithDirectoriesToCheck(@"D:\Test");
                })
                .AddMongoDbWatcher("mongodb://localhost:27017", "MyDatabase", cfg =>
                {
                    cfg.WithQuery("Users", "{\"name\": \"admin\"}")
                        .EnsureThat(users => users.Any(user => user.role == "admin"));
                })
                .AddMsSqlWatcher(@"Data Source=.\sqlexpress;Initial Catalog=MyDatabase;Integrated Security=True", cfg =>
                {
                    cfg.WithQuery("select * from users where id = @id", new Dictionary<string, object> {["id"] = 1})
                        .EnsureThat(users => users.Any(user => user.Name == "admin"));
                })
                .AddPerformanceWatcher(cfg => cfg.EnsureThat(usage => usage.Cpu < 50 && usage.Ram < 5000),
                    hooks =>
                        hooks.OnCompleted(result => System.Console.WriteLine(result.WatcherCheckResult.Description)))
                .AddRedisWatcher("localhost", 1, cfg =>
                {
                    cfg.WithQuery("get test")
                        .EnsureThat(results => results.Any(x => x == "test-value"));
                })
                .AddWebWatcher("http://httpstat.us/200")
                .AddWebWatcher("http://httpstat.us/200", HttpRequest.Post("users", new {name = "test"},
                    headers: new Dictionary<string, string>
                    {
                        ["User-Agent"] = "Warden",
                        ["Authorization"] = "Token MyBase64EncodedString"
                    }), cfg => cfg.EnsureThat(response => response.Headers.Any())
                )
                .IntegrateWithSendGrid("api-key", "noreply@system.com", cfg =>
                {
                    cfg.WithDefaultSubject("Monitoring status")
                        .WithDefaultReceivers("admin@system.com");
                })
                .SetAggregatedWatcherHooks((hooks, integrations) =>
                {
                    hooks.OnFirstFailureAsync(result =>
                            integrations.SendGrid().SendEmailAsync("Monitoring errors have occured."))
                         .OnFirstSuccessAsync(results =>
                            integrations.SendGrid().SendEmailAsync("Everything is up and running again!"));
                })
                .SetGlobalWatcherHooks(hooks =>
                {
                    hooks.OnStart(check => GlobalHookOnStart(check))
                        .OnFailure(result => GlobalHookOnFailure(result))
                        .OnSuccess(result => GlobalHookOnSuccess(result))
                        .OnCompleted(result => GlobalHookOnCompleted(result));
                })
                .SetHooks(hooks =>
                {
                    hooks.OnIterationCompleted(iteration => OnIterationCompleted(iteration))
                         .OnError(exception => System.Console.WriteLine(exception));
                })
                .Build();

            return Warden.Create(wardenConfiguration);
        }

        private static void GlobalHookOnStart(IWatcherCheck check)
        {
            System.Console.WriteLine($"Invoking the global hook OnStart() by watcher: '{check.WatcherName}'.");
        }

        private static void GlobalHookOnSuccess(IWardenCheckResult check)
        {
            System.Console.WriteLine(
                $"Invoking the global hook OnSuccess() by watcher: '{check.WatcherCheckResult.WatcherName}'.");
        }

        private static void GlobalHookOnCompleted(IWardenCheckResult check)
        {
            System.Console.WriteLine(
                $"Invoking the global hook OnCompleted() by watcher: '{check.WatcherCheckResult.WatcherName}'.");
        }

        private static void GlobalHookOnFailure(IWardenCheckResult check)
        {
            System.Console.WriteLine(
                $"Invoking the global hook OnFailure() by watcher: '{check.WatcherCheckResult.WatcherName}'.");
        }

        private static void GlobalHookOnStart(IWardenCheckResult check)
        {
            System.Console.WriteLine(
                $"Invoking the global hook OnFailure() by watcher: '{check.WatcherCheckResult.WatcherName}'.");
        }

        private static void OnIterationCompleted(IWardenIteration wardenIteration)
        {
            var newLine = Environment.NewLine;
            System.Console.WriteLine($"Warden iteration {wardenIteration.Ordinal} has completed.");
            foreach (var result in wardenIteration.Results)
            {
                System.Console.WriteLine($"Watcher: '{result.WatcherCheckResult.WatcherName}'{newLine}" +
                                         $"Description: {result.WatcherCheckResult.Description}{newLine}" +
                                         $"Is valid: {result.IsValid}{newLine}" +
                                         $"Started at: {result.StartedAt}{newLine}" +
                                         $"Completed at: {result.CompletedAt}{newLine}" +
                                         $"Execution time: {result.ExecutionTime}{newLine}");
            }
        }
    }
}
