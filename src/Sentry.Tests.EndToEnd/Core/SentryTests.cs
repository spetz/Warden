using System.Linq;
using Machine.Specifications;
using Sentry.Core;
using Sentry.Watchers.Web;
using It = Machine.Specifications.It;

namespace Sentry.Tests.EndToEnd.Core
{
    public class Sentry_specs
    {
        protected static ISentry Sentry { get; set; }
        protected static SentryConfiguration SentryConfiguration { get; set; }
        protected static WebWatcher WebWatcher { get; set; }
        protected static WebWatcherConfiguration WatcherConfiguration { get; set; }
        protected static ISentryIteration SentryIteration { get; set; } 
    }

    [Subject("Sentry exeuction with watcher")]
    public class when_running_one_iteration_without_any_hooks_setup : Sentry_specs
    {
        Establish context = () =>
        {
            WatcherConfiguration = WebWatcherConfiguration
                .Create("http://httpstat.us/200")
                .Build();
            WebWatcher = WebWatcher.Create("Valid web watcher",WatcherConfiguration);
            SentryConfiguration = SentryConfiguration
                .Create()
                .AddWatcher(WebWatcher)
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
            WatcherConfiguration = WebWatcherConfiguration
                .Create("http://httpstat.us/400")
                .Build();
            WebWatcher = WebWatcher.Create("Invalid web watcher", WatcherConfiguration);
            SentryConfiguration = SentryConfiguration
                .Create()
                .SetHooks(hooks =>
                {
                    hooks.OnIterationCompleted(iteration => UpdateSentryIteration(iteration));
                })
                .AddWatcher(WebWatcher)
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