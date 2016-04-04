using System;
using Machine.Specifications;
using Warden.Core;
using Warden.Watchers.Web;
using It = Machine.Specifications.It;

namespace Warden.Tests.EndToEnd.Watchers.Web
{
    public class WebWatcherSpecs
    {
        protected static WebWatcher Watcher { get; set; }
        protected static WebWatcherConfiguration Configuration { get; set; }
        protected static IWatcherCheckResult CheckResult { get; set; }
        protected static WebWatcherCheckResult WebCheckResult { get; set; }
        protected static Exception Exception { get; set; }
    }

    [Subject("Web watcher execution")]
    public class when_trying_to_access_invalid_url : WebWatcherSpecs
    {
        Establish context = () =>
        {
            Configuration = WebWatcherConfiguration.
                Create("http://www.testwebsitethatdoesnotexist.com")
                .Build();
            Watcher = WebWatcher.Create("Invalid web watcher", Configuration);
        };

        Because of = () => Exception = Catch.Exception(() => Watcher.ExecuteAsync().Await());

        It should_fail = () => Exception.ShouldBeOfExactType<WatcherException>();
        It should_have_a_specific_reason = () => Exception.Message.ShouldContain("There was an error while trying to access the Web endpoint");
    }

    [Subject("Web watcher execution")]
    public class when_website_returns_invalid_status_code : WebWatcherSpecs
    {
        Establish context = () =>
        {
            Configuration = WebWatcherConfiguration
                .Create("http://httpstat.us/400")
                .Build();
            Watcher = WebWatcher.Create("Valid web watcher", Configuration);
        };

        Because of = async () => CheckResult = await Watcher.ExecuteAsync().Await().AsTask;

        It should_have_invalid_check_result = () => CheckResult.IsValid.ShouldBeFalse();
    }

    [Subject("Web watcher execution")]
    public class when_website_returns_valid_status_code : WebWatcherSpecs
    {
        Establish context = () =>
        {
            Configuration = WebWatcherConfiguration
                .Create("http://httpstat.us/200")
                .Build();
            Watcher = WebWatcher.Create("Valid web watcher", Configuration);
        };

        Because of = async () =>
        {
            CheckResult = await Watcher.ExecuteAsync().Await().AsTask;
            WebCheckResult = CheckResult as WebWatcherCheckResult;
        };

        It should_have_valid_check_result = () => CheckResult.IsValid.ShouldBeTrue();
        It should_have_check_result_of_type_web = () => CheckResult.ShouldBeAssignableTo<WebWatcherCheckResult>();
        It should_have_check_result_with_valid_uri = () => WebCheckResult.Uri.ShouldNotBeNull();
        It should_have_check_result_with_request_headers = () => WebCheckResult.Response.Headers.ShouldNotBeNull();
        It should_have_check_result_with_response = () => WebCheckResult.Response.ShouldNotBeNull();
    }
}