using System.Threading.Tasks;
using FluentAssertions;
using Sentry.Core;
using Sentry.Watchers.Website;

namespace Sentry.Tests.EndToEnd
{
    [Specification]
    public class WebsiteWatcher_specs : SpecificationBase
    {
        protected WebsiteWatcher WebsiteWatcher { get; set; }
        protected WebsiteWatcherConfiguration WebsiteWatcherConfiguration { get; set; }
        protected IWatcherCheckResult WebsiteWatcherCheckResult { get; set; }
    }

    [Specification]
    public class when_trying_to_access_invalid_url : WebsiteWatcher_specs
    {
        protected override async Task EstablishContext()
        {
            await base.EstablishContext();
            ExceptionExpected = true;
        }

        protected override async Task BecauseOf()
        {
            await base.BecauseOf();
            WebsiteWatcherConfiguration = WebsiteWatcherConfiguration.Create("Invalid website watcher")
                .WithUrl("http://www.testwebsitethatdoesnotexist.com")
                .Build();
            WebsiteWatcher = new WebsiteWatcher(WebsiteWatcherConfiguration);
            WebsiteWatcherCheckResult = await WebsiteWatcher.ExecuteAsync();
        }

        [Then]
        public void then_exception_should_be_thrown()
        {
            ExceptionThrown.Should().BeAssignableTo<WatcherException>();
            ExceptionThrown.Message.Should().StartWithEquivalent("There was an error while trying to access URL");
        }
    }

    [Specification]
    public class when_website_returns_invalid_status_code : WebsiteWatcher_specs
    {
        protected override async Task EstablishContext()
        {
            await base.EstablishContext();
            ExceptionExpected = true;
        }

        protected override async Task BecauseOf()
        {
            await base.BecauseOf();
            WebsiteWatcherConfiguration = WebsiteWatcherConfiguration.Create("Valid website watcher")
                .WithUrl("http://httpstat.us/400")
                .Build();
            WebsiteWatcher = new WebsiteWatcher(WebsiteWatcherConfiguration);
            WebsiteWatcherCheckResult = await WebsiteWatcher.ExecuteAsync();
        }

        [Then]
        public void then_exception_should_be_thrown()
        {
            ExceptionThrown.Should().BeAssignableTo<WatcherException>();
            ExceptionThrown.Message.Should().StartWithEquivalent("The server has returned an invalid response while trying to access URL");
        }
    }

    [Specification]
    public class when_website_returns_valid_status_code : WebsiteWatcher_specs
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
            WebsiteWatcherCheckResult = await WebsiteWatcher.ExecuteAsync();
        }

        [Then]           
        public void then_website_watcher_check_result_should_be_returned()
        {
            WebsiteWatcherCheckResult.Should().BeAssignableTo<WebsiteWatcherCheckResult>();
            var result = (WebsiteWatcherCheckResult) WebsiteWatcherCheckResult;
            result.Uri.Should().NotBeNull();
            result.RequestHeaders.Should().NotBeNull();
            result.Response.Should().NotBeNull();
        }
    }
}