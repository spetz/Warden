using System;
using Machine.Specifications;
using Moq;
using Sentry.Watchers.Website;
using It = Machine.Specifications.It;

namespace Sentry.Tests.Unit.Watchers.Website
{
    public class WebsiteWatcher_specs
    {
        protected static WebsiteWatcher Watcher { get; set; }
        protected static WebsiteWatcherConfiguration Configuration { get; set; }
        protected static IWatcherCheckResult CheckResult { get; set; }
        protected static WebsiteWatcherCheckResult WebsiteCheckResult { get; set; }
        protected static Exception Exception { get; set; }
    }

    [Subject("Website watcher initialization")]
    public class when_initializing_without_configuration : WebsiteWatcher_specs
    {
        Establish context = () => Configuration = null;

        Because of = () => Exception = Catch.Exception(() => Watcher = WebsiteWatcher.Create("test", Configuration));

        It should_fail = () => Exception.ShouldBeOfExactType<ArgumentNullException>();
        It should_have_a_specific_reason = () => Exception.Message.ShouldContain("Website Watcher configuration has not been provided.");
    }

    [Subject("Website watcher initialization")]
    public class when_initializing_with_invalid_url_in_configuration : WebsiteWatcher_specs
    {
        Establish context = () => {};

        Because of = () => Exception = Catch.Exception(() =>
        {
            Configuration = WebsiteWatcherConfiguration
                .Create("invalid url")
                .Build();
        });

        It should_fail = () => Exception.ShouldBeOfExactType<UriFormatException>();
    }

    [Subject("Website watcher execution")]
    public class when_invoking_execute_async_method_with_valid_url : WebsiteWatcher_specs
    {
        static Mock<IHttpClient> HttpClientMock { get; set; }

        Establish context = () =>
        {
            HttpClientMock = new Mock<IHttpClient>();
            Configuration = WebsiteWatcherConfiguration
                .Create("http://website.com")
                .WithHttpClientProvider(() => HttpClientMock.Object)
                .Build();
            Watcher = WebsiteWatcher.Create("Website watcher", Configuration);
        };

        Because of = async () => await Watcher.ExecuteAsync().Await().AsTask;

        It should_invoke_get_async_method_only_once = () => HttpClientMock.Verify(x => x.GetAsync(Moq.It.IsAny<string>()), Times.Once);
    }
}