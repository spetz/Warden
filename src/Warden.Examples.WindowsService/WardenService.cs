using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using NLog;
using Warden.Core;
using Warden.Watchers.MongoDb;
using Warden.Watchers.MsSql;
using Warden.Watchers.Redis;
using Warden.Watchers.Web;

namespace Warden.Examples.WindowsService
{
    public class WardenService
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static readonly IWarden Warden = ConfigureWarden();

        public async Task StartAsync()
        {
            Logger.Info("Warden service has been started.");
            await Warden.StartAsync();
        }

        public async Task PauseAsync()
        {
            await Warden.PauseAsync();
            Logger.Info("Warden service has been paused.");
        }

        public async Task StopAsync()
        {
            await Warden.StopAsync();
            Logger.Info("Warden service has been stopped.");
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

            var wardenConfiguration = WardenConfiguration
                .Create()
                .SetHooks(hooks =>
                {
                    hooks.OnError(exception => Logger.Error(exception));
                    hooks.OnIterationCompleted(iteration => OnIterationCompleted(iteration));
                })
                .AddWatcher(apiWatcher)
                .AddWatcher(mssqlWatcher)
                .AddWatcher(mongoDbWatcher)
                .AddWatcher(redisWatcher)
                .AddWatcher(websiteWatcher, hooks =>
                {
                    hooks.OnStartAsync(check => WebsiteHookOnStartAsync(check));
                    hooks.OnFailureAsync(result => WebsiteHookOnFailureAsync(result));
                    hooks.OnSuccessAsync(result => WebsiteHookOnSuccessAsync(result));
                    hooks.OnCompletedAsync(result => WebsiteHookOnCompletedAsync(result));
                })
                .SetGlobalWatcherHooks(hooks =>
                {
                    hooks.OnStart(check => GlobalHookOnStart(check));
                    hooks.OnFailure(result => GlobalHookOnFailure(result));
                    hooks.OnSuccess(result => GlobalHookOnSuccess(result));
                    hooks.OnCompleted(result => GlobalHookOnCompleted(result));
                    hooks.OnError(exception => Logger.Error(exception));
                })
                .Build();

            return new Warden(wardenConfiguration);
        }

        private static async Task WebsiteHookOnStartAsync(IWatcherCheck check)
        {
            Logger.Info($"Invoking the hook OnStartAsync() by watcher: '{check.WatcherName}'.");
            await Task.CompletedTask;
        }

        private static async Task WebsiteHookOnSuccessAsync(IWardenCheckResult check)
        {
            var webWatcherCheckResult = (WebWatcherCheckResult) check.WatcherCheckResult;
            Logger.Info($"Invoking the hook OnSuccessAsync() by watcher: '{webWatcherCheckResult.WatcherName}'.");
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