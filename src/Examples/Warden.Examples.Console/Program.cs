using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Warden.Core;
using Warden.Integrations.HttpApi;
using Warden.Integrations.Slack;
using Warden.Watchers;
using Warden.Watchers.Disk;
using Warden.Watchers.MongoDb;
using Warden.Watchers.MsSql;
using Warden.Watchers.Performance;
using Warden.Watchers.Process;
using Warden.Watchers.Redis;
using Warden.Watchers.Web;
using Warden.Watchers.Port;

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
                .AddProcessWatcher("mongod")
                .AddRedisWatcher("localhost", 1, cfg =>
                {
                    cfg.WithQuery("get test")
                        .EnsureThat(results => results.Any(x => x == "test-value"));
                })
                .AddWebWatcher("http://httpstat.us/200", hooks =>
                {
                    hooks.OnStartAsync(check => WebsiteHookOnStartAsync(check))
                        .OnSuccessAsync(check => WebsiteHookOnSuccessAsync(check))
                        .OnCompletedAsync(check => WebsiteHookOnCompletedAsync(check))
                        .OnFailureAsync(check => WebsiteHookOnFailureAsync(check));
                })
                .AddPortWatcher("www.google.pl", 80)
                .AddWebWatcher("API watcher", "http://httpstat.us/200", HttpRequest.Post("users", new {name = "test"},
                    headers: new Dictionary<string, string>
                    {
                        ["User-Agent"] = "Warden",
                        ["Authorization"] = "Token MyBase64EncodedString"
                    }), cfg => cfg.EnsureThat(response => response.Headers.Any())
                )
                //Set proper API key or credentials.
                //.IntegrateWithSendGrid("api-key", "noreply@system.com", cfg =>
                //{
                //    cfg.WithDefaultSubject("Monitoring status")
                //        .WithDefaultReceivers("admin@system.com");
                //})
                //.SetAggregatedWatcherHooks((hooks, integrations) =>
                //{
                //    hooks.OnFirstFailureAsync(result =>
                //        integrations.SendGrid().SendEmailAsync("Monitoring errors have occured."))
                //        .OnFirstSuccessAsync(results =>
                //            integrations.SendGrid().SendEmailAsync("Everything is up and running again!"));
                //})
                //Set proper URL of the Warden Web API
                .IntegrateWithHttpApi("http://localhost:11223/api",
                    "1pi0Tp9/n2wdLSRsUBAKXwHGPXoFU/58wV8Dc+vIL+k2/fWF14VXPiuK",
                    "0b8351f1-dc93-4137-90b9-e3a7d7e12054")
                //Set proper Slack webhook URL
                //.IntegrateWithSlack("https://hooks.slack.com/services/XXX/YYY/ZZZ")
                .SetGlobalWatcherHooks(hooks =>
                {
                    hooks.OnStart(check => GlobalHookOnStart(check))
                        .OnFailure(result => GlobalHookOnFailure(result))
                        .OnSuccess(result => GlobalHookOnSuccess(result))
                        .OnCompleted(result => GlobalHookOnCompleted(result));
                })
                .SetHooks((hooks, integrations) =>
                {
                    hooks.OnIterationCompleted(iteration => OnIterationCompleted(iteration))
                        //.OnIterationCompletedAsync(iteration =>
                        //    integrations.Slack().SendMessageAsync($"Iteration {iteration.Ordinal} has completed."))
                        .OnIterationCompletedAsync(iteration => integrations.HttpApi()
                            .PostIterationToWardenPanelAsync(iteration))
                        .OnError(exception => System.Console.WriteLine(exception));
                })
                .Build();

            return WardenInstance.Create(wardenConfiguration);
        }

        private static async Task WebsiteHookOnStartAsync(IWatcherCheck check)
        {
            System.Console.WriteLine($"Invoking the hook OnStartAsync() by watcher: '{check.WatcherName}'.");
            await Task.FromResult(true);
        }

        private static async Task WebsiteHookOnSuccessAsync(IWardenCheckResult check)
        {
            var webWatcherCheckResult = (WebWatcherCheckResult)check.WatcherCheckResult;
            System.Console.WriteLine("Invoking the hook OnSuccessAsync() " +
                                     $"by watcher: '{webWatcherCheckResult.WatcherName}'.");
            await Task.FromResult(true);
        }

        private static async Task WebsiteHookOnCompletedAsync(IWardenCheckResult check)
        {
            System.Console.WriteLine("Invoking the hook OnCompletedAsync() " +
                                     $"by watcher: '{check.WatcherCheckResult.WatcherName}'.");
            await Task.FromResult(true);
        }

        private static async Task WebsiteHookOnFailureAsync(IWardenCheckResult check)
        {
            System.Console.WriteLine("Invoking the hook OnFailureAsync() " +
                                     $"by watcher: '{check.WatcherCheckResult.WatcherName}'.");
            await Task.FromResult(true);
        }

        private static void GlobalHookOnStart(IWatcherCheck check)
        {
            System.Console.WriteLine("Invoking the global hook OnStart() " +
                                     $"by watcher: '{check.WatcherName}'.");
        }

        private static void GlobalHookOnSuccess(IWardenCheckResult check)
        {
            System.Console.WriteLine("Invoking the global hook OnSuccess() " +
                                     $"by watcher: '{check.WatcherCheckResult.WatcherName}'.");
        }

        private static void GlobalHookOnCompleted(IWardenCheckResult check)
        {
            System.Console.WriteLine("Invoking the global hook OnCompleted() " +
                                     $"by watcher: '{check.WatcherCheckResult.WatcherName}'.");
        }

        private static void GlobalHookOnFailure(IWardenCheckResult check)
        {
            System.Console.WriteLine("Invoking the global hook OnFailure() " +
                                     $"by watcher: '{check.WatcherCheckResult.WatcherName}'.");
        }

        private static void OnIterationCompleted(IWardenIteration wardenIteration)
        {
            var newLine = Environment.NewLine;
            System.Console.WriteLine($"{wardenIteration.WardenName} iteration {wardenIteration.Ordinal} has completed.");
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
