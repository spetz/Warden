using System.Linq;
using Machine.Specifications;
using Sentry.Core;
using Sentry.Watchers.Website;
using It = Machine.Specifications.It;

namespace Sentry.Tests.EndToEnd.Core
{
    public class Sentry_specs
    {
        protected static ISentry Sentry { get; set; }
        protected static SentryConfiguration SentryConfiguration { get; set; }
        protected static WebsiteWatcher WebsiteWatcher { get; set; }
        protected static WebsiteWatcherConfiguration WebsiteWatcherConfiguration { get; set; }
        protected static ISentryIteration SentryIteration { get; set; } 
    }

    [Subject("Sentry exeuction with watcher")]
    public class when_running_one_iteration_without_any_hooks_setup : Sentry_specs
    {
        Establish context = () =>
        {
            WebsiteWatcherConfiguration = WebsiteWatcherConfiguration
                .Create("http://httpstat.us/200")
                .Build();
            WebsiteWatcher = WebsiteWatcher.Create("Valid website watcher",WebsiteWatcherConfiguration);
            SentryConfiguration = SentryConfiguration
                .Create()
                .AddWatcher(WebsiteWatcher)
                .RunOnlyOnce()
                .Build();
            Sentry = new Sentry(SentryConfiguration);
        };

        Because of = async () => await Sentry.StartAsync().Await().AsTask;

        It should_be_just_fine = () => true.ShouldBeTrue(); // :)
    }

    [Subject("Sentry exeuction with invalid watcher")]
    public class when_running_one_iteration_with_on_completed_hooks_setup : Sentry_specs
    {
        Establish context = () =>
        {
            WebsiteWatcherConfiguration = WebsiteWatcherConfiguration
                .Create("http://httpstat.us/400")
                .Build();
            WebsiteWatcher = WebsiteWatcher.Create("Invalid website watcher", WebsiteWatcherConfiguration);
            SentryConfiguration = SentryConfiguration
                .Create()
                .SetHooks(hooks =>
                {
                    hooks.OnIterationCompleted(iteration => UpdateSentryIteration(iteration));
                })
                .AddWatcher(WebsiteWatcher)
                .RunOnlyOnce()
                .Build();
            Sentry = new Sentry(SentryConfiguration);
        };

        Because of = async () => await Sentry.StartAsync().Await().AsTask;

        It should_return_the_iteration_with_invalid_results = () => SentryIteration.Results.All(x => !x.IsValid).ShouldBeTrue();

        private static void UpdateSentryIteration(ISentryIteration sentryIteration)
        {
            SentryIteration = sentryIteration;
        }
    }
}