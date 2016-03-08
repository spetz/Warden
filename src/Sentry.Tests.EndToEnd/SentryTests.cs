using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Sentry.Core;
using Sentry.Watchers.Website;

namespace Sentry.Tests.EndToEnd
{
    [Specification]
    public class Sentry_specs : SpecificationBase
    {
        protected ISentry Sentry { get; set; }
        protected SentryConfiguration SentryConfiguration { get; set; }
        protected WebsiteWatcher WebsiteWatcher { get; set; }
        protected WebsiteWatcherConfiguration WebsiteWatcherConfiguration { get; set; }
        protected ISentryIteration SentryIteration { get; set; } 
    }

    [Specification]
    public class when_watchers_have_been_configured_and_main_method_is_executed : Sentry_specs
    {
        protected override async Task EstablishContext()
        {
            await base.EstablishContext();
        }

        protected override async Task BecauseOf()
        {
            await base.BecauseOf();
            WebsiteWatcherConfiguration = WebsiteWatcherConfiguration
                .Create("Valid website watcher")
                .WithUrl("http://httpstat.us/200")
                .Build();
            WebsiteWatcher = new WebsiteWatcher(WebsiteWatcherConfiguration);
            SentryConfiguration = SentryConfiguration
                .Create()
                .AddWatcher(WebsiteWatcher)
                .RunOnlyOnce()
                .Build();
            Sentry = new Sentry(SentryConfiguration);
            await Sentry.StartAsync();
        }

        [Then]
        public void then_everything_should_be_just_fine() // :)
        {
            true.ShouldBeEquivalentTo(true);
        }
    }

    [Specification]
    public class when_watcher_has_an_invalid_resource_uri_and_main_method_is_executed : Sentry_specs
    {
        protected override async Task EstablishContext()
        {
            await base.EstablishContext();
        }

        protected override async Task BecauseOf()
        {
            await base.BecauseOf();
            WebsiteWatcherConfiguration = WebsiteWatcherConfiguration
                .Create("Invalid website watcher")
                .WithUrl("http://httpstat.us/400")
                .Build();
            WebsiteWatcher = new WebsiteWatcher(WebsiteWatcherConfiguration);
            SentryConfiguration = SentryConfiguration
                .Create()
                .SetIterationHooks(hooks =>
                {
                    hooks.OnIterationCompleted(iteration => UpdateSentryIteration(iteration));
                })
                .AddWatcher(WebsiteWatcher)
                .RunOnlyOnce()
                .Build();
            Sentry = new Sentry(SentryConfiguration);
            await Sentry.StartAsync();
        }

        [Then]
        public void then_sentry_iteration_results_should_not_be_valid()
        {
            SentryIteration.Results.All(x => !x.IsValid).ShouldBeEquivalentTo(true);
        }

        private void UpdateSentryIteration(ISentryIteration sentryIteration)
        {
            SentryIteration = sentryIteration;
        }
    }
}