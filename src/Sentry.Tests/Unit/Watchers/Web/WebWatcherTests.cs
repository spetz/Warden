using System;
using Machine.Specifications;
using Moq;
using Sentry.Watchers.Web;
using It = Machine.Specifications.It;

namespace Sentry.Tests.Unit.Watchers.Web
{
    public class WebWatcher_specs
    {
        protected static WebWatcher Watcher { get; set; }
        protected static WebWatcherConfiguration Configuration { get; set; }
        protected static IWatcherCheckResult CheckResult { get; set; }
        protected static WebWatcherCheckResult WebCheckResult { get; set; }
        protected static Exception Exception { get; set; }
    }

    [Subject("Web watcher initialization")]
    public class when_initializing_without_configuration : WebWatcher_specs
    {
        Establish context = () => Configuration = null;

        Because of = () => Exception = Catch.Exception(() => Watcher = WebWatcher.Create("test", Configuration));

        It should_fail = () => Exception.ShouldBeOfExactType<ArgumentNullException>();
        It should_have_a_specific_reason = () => Exception.Message.ShouldContain("Web watcher configuration has not been provided.");
    }

    [Subject("Web watcher initialization")]
    public class when_initializing_with_invalid_url_in_configuration : WebWatcher_specs
    {
        Establish context = () => {};

        Because of = () => Exception = Catch.Exception(() =>
        {
            Configuration = WebWatcherConfiguration
                .Create("invalid url")
                .Build();
        });

        It should_fail = () => Exception.ShouldBeOfExactType<UriFormatException>();
    }

    [Subject("Web watcher execution")]
    public class when_invoking_execute_async_method_with_valid_url_for_get_request : WebWatcher_specs
    {
        static Mock<IHttpService> HttpServiceMock { get; set; }
        static Mock<IHttpResponse> HttpResponseMock { get; set; }

        Establish context = () =>
        {
            HttpServiceMock = new Mock<IHttpService>();
            HttpResponseMock = new Mock<IHttpResponse>();
            HttpServiceMock.Setup(x =>
                x.ExecuteAsync(Moq.It.IsAny<string>(), Moq.It.IsAny<IHttpRequest>(), Moq.It.IsAny<TimeSpan?>()))
                .ReturnsAsync(HttpResponseMock.Object);

            Configuration = WebWatcherConfiguration
                .Create("http://website.com", HttpRequest.Get())
                .WithHttpServiceProvider(() => HttpServiceMock.Object)
                .Build();
            Watcher = WebWatcher.Create("Web watcher", Configuration);
        };

        Because of = async () => await Watcher.ExecuteAsync().Await().AsTask;

        It should_invoke_http_service_execute_async_method_only_once = () =>
            HttpServiceMock.Verify(x => x.ExecuteAsync(Moq.It.IsAny<string>(),
                Moq.It.IsAny<IHttpRequest>(), Moq.It.IsAny<TimeSpan?>()), Times.Once);
    }
}