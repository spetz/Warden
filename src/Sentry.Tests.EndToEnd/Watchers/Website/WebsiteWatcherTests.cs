using System;
using Machine.Specifications;
using Sentry.Core;
using Sentry.Watchers.Website;
using It = Machine.Specifications.It;

namespace Sentry.Tests.EndToEnd.Watchers.Website
{
    public class WebsiteWatcher_specs
    {
        protected static WebsiteWatcher Watcher { get; set; }
        protected static WebsiteWatcherConfiguration Configuration { get; set; }
        protected static IWatcherCheckResult CheckResult { get; set; }
        protected static WebsiteWatcherCheckResult WebsiteCheckResult { get; set; }
        protected static Exception Exception { get; set; }
    }

    [Subject("Website watcher execution")]
    public class when_trying_to_access_invalid_url : WebsiteWatcher_specs
    {
        Establish context = () =>
        {
            Configuration = WebsiteWatcherConfiguration.
                Create("http://www.testwebsitethatdoesnotexist.com")
                .Build();
            Watcher = WebsiteWatcher.Create("Invalid website watcher", Configuration);
        };

        Because of = () => Exception = Catch.Exception(() => Watcher.ExecuteAsync().Await());

        It should_fail = () => Exception.ShouldBeOfExactType<WatcherException>();
        It should_have_a_specific_reason = () => Exception.Message.ShouldContain("There was an error while trying to access the URL");
    }

    [Subject("Website watcher execution")]
    public class when_website_returns_invalid_status_code : WebsiteWatcher_specs
    {
        Establish context = () =>
        {
            Configuration = WebsiteWatcherConfiguration
                .Create("http://httpstat.us/400")
                .Build();
            Watcher = WebsiteWatcher.Create("Valid website watcher", Configuration);
        };

        Because of = async () => CheckResult = await Watcher.ExecuteAsync().Await().AsTask;

        It should_have_invalid_check_result = () => CheckResult.IsValid.ShouldBeFalse();
    }

    [Subject("Website watcher execution")]
    public class when_website_returns_valid_status_code : WebsiteWatcher_specs
    {
        Establish context = () =>
        {
            Configuration = WebsiteWatcherConfiguration
                .Create("http://httpstat.us/200")
                .Build();
            Watcher = WebsiteWatcher.Create("Valid website watcher", Configuration);
        };

        Because of = async () =>
        {
            CheckResult = await Watcher.ExecuteAsync().Await().AsTask;
            WebsiteCheckResult = CheckResult as WebsiteWatcherCheckResult;
        };

        It should_have_valid_check_result = () => CheckResult.IsValid.ShouldBeTrue();
        It should_have_check_result_of_type_website = () => CheckResult.ShouldBeAssignableTo<WebsiteWatcherCheckResult>();
        It should_have_check_result_with_valid_uri = () => WebsiteCheckResult.Uri.ShouldNotBeNull();
        It should_have_check_result_with_request_headers = () => WebsiteCheckResult.RequestHeaders.ShouldNotBeNull();
        It should_have_check_result_with_response = () => WebsiteCheckResult.Response.ShouldNotBeNull();
    }
}