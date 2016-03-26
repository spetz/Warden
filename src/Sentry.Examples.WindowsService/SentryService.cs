using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using NLog;
using Sentry.Core;
using Sentry.Watchers.MongoDb;
using Sentry.Watchers.MsSql;
using Sentry.Watchers.Web;

namespace Sentry.Examples.WindowsService
{
    public class SentryService
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static readonly ISentry Sentry = ConfigureSentry();

        public async Task StartAsync()
        {
            Logger.Info("Sentry service has been started.");
            await Sentry.StartAsync();
        }

        public async Task PauseAsync()
        {
            await Sentry.PauseAsync();
            Logger.Info("Sentry service has been paused.");
        }

        public async Task StopAsync()
        {
            await Sentry.StopAsync();
            Logger.Info("Sentry service has been stopped.");
        }

        private static ISentry ConfigureSentry()
        {
            var websiteWatcherConfiguration = WebWatcherConfiguration
                .Create("http://httpstat.us/200")
                .EnsureThat(x => x.IsValid)
                .Build();
            var websiteWatcher = WebWatcher.Create("Website watcher", websiteWatcherConfiguration);

            var mongoDbWatcherConfiguration = MongoDbWatcherConfiguration
                .Create("MyDatabase", "mongodb://localhost:27017")
                .WithQuery("Users", "{\"name\": \"admin\"}")
                .EnsureThat(users => users.Any(user => user.role == "admin"))
                .Build();
            var mongoDbWatcher = MongoDbWatcher.Create("MongoDB watcher", mongoDbWatcherConfiguration);

            var mssqlWatcherConfiguration = MsSqlWatcherConfiguration
                .Create(ConfigurationManager.ConnectionStrings["MyDatabase"].ConnectionString)
                .WithQuery("select * from users where id = @id", new Dictionary<string, object> {["id"] = 1})
                .EnsureThat(users => users.Any(user => user.Name == "admin"))
                .Build();
            var mssqlWatcher = MsSqlWatcher.Create("Database watcher", mssqlWatcherConfiguration);

            var apiWatcherConfiguration = WebWatcherConfiguration
                .Create("http://httpstat.us", HttpRequest.Get("200",
                    headers: new Dictionary<string, string>
                    {
                        ["Accept"] = "application/json"
                    }))
                .Build();
            var apiWatcher = WebWatcher.Create("API watcher", apiWatcherConfiguration);

            var sentryConfiguration = SentryConfiguration
                .Create()
                .SetHooks(hooks =>
                {
                    hooks.OnError(exception => Logger.Error(exception));
                    hooks.OnIterationCompleted(iteration => OnIterationCompleted(iteration));
                })
                .AddWatcher(apiWatcher)
                .AddWatcher(mssqlWatcher)
                .AddWatcher(mongoDbWatcher)
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

            return new Sentry(sentryConfiguration);
        }

        private static async Task WebsiteHookOnStartAsync(IWatcherCheck check)
        {
            Logger.Info($"Invoking the hook OnStartAsync() by watcher: '{check.WatcherName}'.");
            await Task.CompletedTask;
        }

        private static async Task WebsiteHookOnSuccessAsync(ISentryCheckResult check)
        {
            var webWatcherCheckResult = (WebWatcherCheckResult) check.WatcherCheckResult;
            Logger.Info($"Invoking the hook OnSuccessAsync() by watcher: '{webWatcherCheckResult.WatcherName}'.");
            await Task.CompletedTask;
        }

        private static async Task WebsiteHookOnCompletedAsync(ISentryCheckResult check)
        {
            Logger.Info($"Invoking the hook OnCompletedAsync() by watcher: '{check.WatcherCheckResult.WatcherName}'.");
            await Task.CompletedTask;
        }

        private static async Task WebsiteHookOnFailureAsync(ISentryCheckResult check)
        {
            Logger.Info($"Invoking the hook OnFailureAsync() by watcher: '{check.WatcherCheckResult.WatcherName}'.");
            await Task.CompletedTask;
        }

        private static void GlobalHookOnStart(IWatcherCheck check)
        {
            Logger.Info($"Invoking the global hook OnStart() by watcher: '{check.WatcherName}'.");
        }

        private static void GlobalHookOnSuccess(ISentryCheckResult check)
        {
            Logger.Info($"Invoking the global hook OnSuccess() by watcher: '{check.WatcherCheckResult.WatcherName}'.");
        }

        private static void GlobalHookOnCompleted(ISentryCheckResult check)
        {
            Logger.Info($"Invoking the global hook OnCompleted() by watcher: '{check.WatcherCheckResult.WatcherName}'.");
        }

        private static void GlobalHookOnFailure(ISentryCheckResult check)
        {
            Logger.Info($"Invoking the global hook OnFailure() by watcher: '{check.WatcherCheckResult.WatcherName}'.");
        }

        private static void OnIterationCompleted(ISentryIteration sentryIteration)
        {
            var newLine = Environment.NewLine;
            Logger.Info($"Sentry iteration {sentryIteration.Ordinal} has completed.");
            foreach (var result in sentryIteration.Results)
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