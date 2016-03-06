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
            WebsiteWatcherConfiguration = WebsiteWatcherConfiguration.Configure()
                .WithUrl("http://httpstat.us/200")
                .Build();
            WebsiteWatcher = new WebsiteWatcher(WebsiteWatcherConfiguration);
            SentryConfiguration = SentryConfiguration.Configure()
                .AddWatcher(WebsiteWatcher, hooks =>
                {
                    hooks.OnFailure(ex => { });
                    hooks.OnFailureAsync(ex => Task.FromResult(0));
                    hooks.OnSuccess(() => { });
                    hooks.OnSuccessAsync(() => Task.FromResult(0));
                    hooks.OnCompleted(() => { });
                    hooks.OnCompletedAsync(() => Task.FromResult(0));
                })
                .SetGlobalHooks(hooks =>
                {
                    hooks.OnFailure(ex => { });
                    hooks.OnFailureAsync(ex => Task.FromResult(0));
                    hooks.OnSuccess(() => { });
                    hooks.OnSuccessAsync(() => Task.FromResult(0));
                    hooks.OnCompleted(() => { });
                    hooks.OnCompletedAsync(() => Task.FromResult(0));
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
    public class when_any_watcher_has_an_invalid_resource_uri_and_main_method_is_executed : Sentry_specs
    {
        protected override async Task EstablishContext()
        {
            await base.EstablishContext();
            ExceptionExpected = true;
        }

        protected override async Task BecauseOf()
        {
            await base.BecauseOf();
            WebsiteWatcherConfiguration = WebsiteWatcherConfiguration.Configure()
                .WithUrl("http://httpstat.us/400")
                .Build();
            WebsiteWatcher = new WebsiteWatcher(WebsiteWatcherConfiguration);
            SentryConfiguration = SentryConfiguration.Configure()
                .AddWatcher(WebsiteWatcher)
                .Build();
            Sentry = new Sentry(SentryConfiguration);
            await Sentry.ExecuteAsync();
        }

        [Then]
        public void then_exception_should_be_thrown()
        {
            ExceptionThrown.Should().BeAssignableTo<SentryException>();
            ExceptionThrown.Message.Should().StartWithEquivalent("There was an error while executing Sentry caused by watcher");
        }
    }
}