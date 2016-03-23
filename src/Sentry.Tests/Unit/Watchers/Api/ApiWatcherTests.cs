using System;
using Machine.Specifications;
using Moq;
using Sentry.Watchers.Api;
using It = Machine.Specifications.It;

namespace Sentry.Tests.Unit.Watchers.Api
{
    public class ApiWatcher_specs
    {
        protected static ApiWatcher Watcher { get; set; }
        protected static ApiWatcherConfiguration Configuration { get; set; }
        protected static IWatcherCheckResult CheckResult { get; set; }
        protected static ApiWatcherCheckResult WebsiteCheckResult { get; set; }
        protected static Exception Exception { get; set; }
    }

    [Subject("API watcher initialization")]
    public class when_initializing_without_configuration : ApiWatcher_specs
    {
        Establish context = () => Configuration = null;

        Because of = () => Exception = Catch.Exception(() => Watcher = ApiWatcher.Create("test", Configuration));

        It should_fail = () => Exception.ShouldBeOfExactType<ArgumentNullException>();
        It should_have_a_specific_reason = () => Exception.Message.ShouldContain("API watcher configuration has not been provided.");
    }

    [Subject("API watcher initialization")]
    public class when_initializing_with_invalid_url_in_configuration : ApiWatcher_specs
    {
        Establish context = () => {};

        Because of = () => Exception = Catch.Exception(() =>
        {
            Configuration = ApiWatcherConfiguration
                .Create("invalid url", HttpRequest.Get())
                .Build();
        });

        It should_fail = () => Exception.ShouldBeOfExactType<UriFormatException>();
    }

    [Subject("API watcher execution")]
    public class when_invoking_execute_async_method_with_valid_url : ApiWatcher_specs
    {
        static Mock<IHttpClient> HttpClientMock { get; set; }

        Establish context = () =>
        {
            HttpClientMock = new Mock<IHttpClient>();
            Configuration = ApiWatcherConfiguration
                .Create("http://website.com", HttpRequest.Get())
                .WithHttpClientProvider(() => HttpClientMock.Object)
                .Build();
            Watcher = ApiWatcher.Create("API watcher", Configuration);
        };

        Because of = async () => await Watcher.ExecuteAsync().Await().AsTask;

        It should_invoke_get_async_method_only_once = () => HttpClientMock.Verify(x => x.GetAsync(Moq.It.IsAny<string>()), Times.Once);
    }
}