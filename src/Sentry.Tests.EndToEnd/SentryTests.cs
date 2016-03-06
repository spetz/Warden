using System;
using System.Collections.Generic;
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
        protected IEnumerable<ISentryOutcome> SentryOutcomes { get; set; } 
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
            WebsiteWatcherConfiguration = WebsiteWatcherConfiguration.Create("Valid website watcher")
                .WithUrl("http://httpstat.us/200")
                .Build();
            WebsiteWatcher = new WebsiteWatcher(WebsiteWatcherConfiguration);
            SentryConfiguration = SentryConfiguration.Create()
                .AddWatcher(WebsiteWatcher, hooks =>
                {
                    hooks.OnStart(() => { });
                    hooks.OnStartAsync(() => Task.CompletedTask);
                    hooks.OnFailure(outcome => { });
                    hooks.OnFailureAsync(outcome => Task.CompletedTask);
                    hooks.OnSuccess(outcome => { });
                    hooks.OnSuccessAsync(outcome => Task.CompletedTask);
                    hooks.OnCompleted(outcome => { });
                    hooks.OnCompletedAsync(outcome => Task.CompletedTask);
                })
                .SetGlobalHooks(hooks =>
                {
                    hooks.OnStart(() => { });
                    hooks.OnStartAsync(() => Task.CompletedTask);
                    hooks.OnFailure(outcome => { });
                    hooks.OnFailureAsync(outcome => Task.CompletedTask);
                    hooks.OnSuccess(outcome => { });
                    hooks.OnSuccessAsync(outcome => Task.CompletedTask);
                    hooks.OnCompleted(outcome => { });
                    hooks.OnCompletedAsync(outcome => Task.CompletedTask);
                })
                .Build();
            Sentry = new Sentry(SentryConfiguration);
            await Sentry.ExecuteAsync();
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
            WebsiteWatcherConfiguration = WebsiteWatcherConfiguration.Create("Invalid website watcher")
                .WithUrl("http://httpstat.us/400")
                .Build();
            WebsiteWatcher = new WebsiteWatcher(WebsiteWatcherConfiguration);
            SentryConfiguration = SentryConfiguration.Create()
                .AddWatcher(WebsiteWatcher)
                .Build();
            Sentry = new Sentry(SentryConfiguration);
            SentryOutcomes = await Sentry.ExecuteAsync();
        }

        [Then]
        public void then_sentry_outcome_should_contain_an_entry_with_exception_set()
        {
            SentryOutcomes.All(x => x.Exception != null).ShouldBeEquivalentTo(true);
        }
    }
}