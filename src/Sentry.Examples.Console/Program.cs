using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using NLog;
using Sentry.Core;
using Sentry.Integrations.SendGrid;
using Sentry.Watchers.MongoDb;
using Sentry.Watchers.MsSql;
using Sentry.Watchers.Redis;
using Sentry.Watchers.Web;

namespace Sentry.Examples.Console
{
    class Program
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private static void Main(string[] args)
        {
            var sentry = ConfigureSentry();
            Task.WaitAll(sentry.StartAsync());
        }

        private static ISentry ConfigureSentry()
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
                .WithQuery("select * from users where id = @id", new Dictionary<string, object> {["id"] = 1 })
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
                .WithDefaultSubject("Sentry monitoring")
                .WithDefaultReceivers("my@email.com")
                .Build();

            var sendGrid = SendGridIntegration.Create(sendGridConfiguration);

            var sentryConfiguration = SentryConfiguration
                .Create()
                .SetHooks((hooks, integrations) =>
                {
                    //hooks.OnIterationCompleted(iteration => OnIterationCompleted(iteration));
                    hooks.OnError(exception => Logger.Error(exception));
                })
                .AddWatcher(apiWatcher)
                .AddWatcher(mssqlWatcher)
                .AddWatcher(mongoDbWatcher)
                .AddWatcher(redisWatcher)
                .AddWatcher(websiteWatcher, hooks =>
                {
                    //hooks.OnFailureAsync(result => WebsiteHookOnFailureAsync(result));
                    //hooks.OnSuccessAsync(result => WebsiteHookOnSuccessAsync(result));
                    //hooks.OnCompletedAsync(result => WebsiteHookOnCompletedAsync(result));
                })
                .SetGlobalWatcherHooks((hooks, integrations) =>
                {
                    //hooks.OnStart(check => GlobalHookOnStart(check));
                    //hooks.OnFailure(result => GlobalHookOnFailure(result));
                    //hooks.OnSuccess(result => GlobalHookOnSuccess(result));
                    //hooks.OnCompleted(result => GlobalHookOnCompleted(result));
                    //hooks.OnError(exception => Logger.Error(exception));
                    //hooks.OnFirstFailureAsync(check => integrations
                    //    .SendGrid(sendGridConfiguration)
                    //    .SendEmailAsync($"{check.WatcherCheckResult.WatcherName}: {check.WatcherCheckResult.Description}"));
                    //hooks.OnFirstSuccessAsync(check => integrations
                    //    .SendGrid(sendGridConfiguration)
                    //    .SendEmailAsync($"{check.WatcherCheckResult.WatcherName}: everything is up and running again! :)"));
                })
                .SetAggregatedWatcherHooks(hooks =>
                {
                    hooks.OnFirstFailure(results => Logger.Info($"First failure for {results.Count()} results"));
                    hooks.OnFirstSuccess(results => Logger.Info($"First success for {results.Count()} results"));
                    hooks.OnFirstError(exceptions => Logger.Info($"First error for {exceptions.Count()} exceptions"));
                    hooks.OnCompleted(results => Logger.Info($"Completed for {results.Count()} results"));

                    //hooks.OnFirstFailureAsync(results => sendGrid.SendEmailAsync(GetSentryCheckResultDetails(results)));
                    //hooks.OnFirstSuccessAsync(results => sendGrid.SendEmailAsync(GetSentryCheckResultDetails(results)));
                    //hooks.OnFirstErrorAsync(exceptions => sendGrid.SendEmailAsync(GetExceptionsDetails(exceptions)));
                    //hooks.OnCompletedAsync(results => sendGrid.SendEmailAsync(GetSentryCheckResultDetails(results)));
                })
                .Build();

            return new Sentry(sentryConfiguration);
        }

        private static string GetExceptionsDetails(IEnumerable<Exception> exceptions)
        {
            if (!exceptions.Any())
                return string.Empty;

            return exceptions.Select(x => x.ToString()).Aggregate((x, y) => $"{x}\n{y}");
        }

        private static string GetSentryCheckResultDetails(IEnumerable<ISentryCheckResult> results)
        {
            if (!results.Any())
                return string.Empty;

            Func<ISentryCheckResult, string> details = result => $"{result.WatcherCheckResult.WatcherName} " +
                                                                 $"({(result.WatcherCheckResult.IsValid ? "valid" : "invalid")}): " +
                                                                 $"{result.WatcherCheckResult.Description}\n";

            return results.Select(details).Aggregate((x, y) => $"{x}\n{y}");
        }

        private static async Task WebsiteHookOnStartAsync(IWatcherCheck check)
        {
            Logger.Info($"Invoking the hook OnStartAsync() by watcher: '{check.WatcherName}'.");
            await Task.CompletedTask;
        }

        private static async Task WebsiteHookOnSuccessAsync(ISentryCheckResult check)
        {
            var websiteWatcherCheckResult = (WebWatcherCheckResult)check.WatcherCheckResult;
            Logger.Info($"Invoking the hook OnSuccessAsync() by watcher: '{websiteWatcherCheckResult.WatcherName}'.");
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
