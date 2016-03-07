using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NLog;
using Sentry.Core;
using Sentry.Watchers.Website;

namespace Sentry.Examples.WindowsService
{
    public class SentryService
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public async Task StartAsync()
        {
            Logger.Info("Sentry service has been started.");
            var sentry = ConfigureSentry();
            while (true)
            {
                try
                {
                    var sentryCheckResults = await sentry.ExecuteAsync();
                    FormatSentryResult(sentryCheckResults.ToList());
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                }

                await Task.Delay(5000);
            }
        }

        public void Stop()
        {
            Logger.Info("Sentry service has been stopped.");
        }

        private static ISentry ConfigureSentry()
        {
            var websiteWatcherConfiguration = WebsiteWatcherConfiguration.Create("httpstat.us website watcher")
                .WithUrl("http://httpstat.us/200")
                .Build();
            var websiteWatcher = new WebsiteWatcher(websiteWatcherConfiguration);
            var sentryConfiguration = SentryConfiguration.Create()
                .AddWatcher(websiteWatcher, hooks =>
                {
                    hooks.OnStartAsync(WebsiteHookOnStartAsync);
                    hooks.OnFailureAsync(WebsiteHookOnFailureAsync);
                    hooks.OnSuccessAsync(WebsiteHookOnSuccessAsync);
                    hooks.OnCompletedAsync(WebsiteHookOnCompletedAsync);
                })
                .SetGlobalHooks(hooks =>
                {
                    hooks.OnStart(GlobalHookOnStart);
                    hooks.OnFailure(GlobalHookOnFailure);
                    hooks.OnSuccess(GlobalHookOnSuccess);
                    hooks.OnCompleted(GlobalHookOnCompleted);
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
            var websiteWatcherCheckResult = (WebsiteWatcherCheckResult) check.WatcherCheckResult;
            Logger.Info($"Invoking the hook OnSuccessAsync() by watcher: '{websiteWatcherCheckResult.WatcherName}'.");
            await Task.CompletedTask;
        }

        private static async Task WebsiteHookOnCompletedAsync(ISentryCheckResult check)
        {
            var websiteWatcherCheckResult = (WebsiteWatcherCheckResult) check.WatcherCheckResult;
            Logger.Info($"Invoking the hook OnCompletedAsync() by watcher: '{websiteWatcherCheckResult.WatcherName}'.");
            await Task.CompletedTask;
        }

        private static async Task WebsiteHookOnFailureAsync(ISentryCheckResult check)
        {
            var websiteWatcherCheckResult = (WebsiteWatcherCheckResult) check.WatcherCheckResult;
            Logger.Info($"Invoking the hook OnFailureAsync() by watcher: '{websiteWatcherCheckResult.WatcherName}'.");
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

        private static void FormatSentryResult(ICollection<ISentryCheckResult> sentryCheckResults)
        {
            var newLine = Environment.NewLine;
            Logger.Info($"Received {sentryCheckResults.Count} Sentry check results.");
            foreach (var result in sentryCheckResults)
            {
                Logger.Info($"Watcher: '{result.WatcherCheckResult.WatcherName}'{newLine}" +
                            $"Description: {result.WatcherCheckResult.Description}{newLine}" +
                            $"Is valid: {result.IsValid}{newLine}" +
                            $"Execution time: {result.ExecutionTime}{newLine}");
            }
        }
    }
}