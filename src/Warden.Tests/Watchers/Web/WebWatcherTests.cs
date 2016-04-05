using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using Warden.Watchers;
using Warden.Watchers.Web;
using It = Machine.Specifications.It;

namespace Warden.Tests.Watchers.Web
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
        It should_have_a_specific_reason = () => Exception.Message.ShouldContain("Web Watcher configuration has not been provided.");
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
        static Mock<IHttpService> HttpServiceMock;
        static IHttpResponse Response;

        Establish context = () =>
        {
            HttpServiceMock = new Mock<IHttpService>();
            Response = HttpResponse.Valid(HttpStatusCode.OK, "Ok", new Dictionary<string, string>(), string.Empty);
            HttpServiceMock.Setup(x =>
                x.ExecuteAsync(Moq.It.IsAny<string>(), Moq.It.IsAny<IHttpRequest>(), Moq.It.IsAny<TimeSpan?>()))
                .ReturnsAsync(Response);

            Configuration = WebWatcherConfiguration
                .Create("http://website.com", HttpRequest.Get())
                .WithHttpServiceProvider(() => HttpServiceMock.Object)
                .Build();
            Watcher = WebWatcher.Create("Web watcher", Configuration);
        };

        Because of = async () =>
        {
            CheckResult = await Watcher.ExecuteAsync().Await().AsTask;
            WebCheckResult = CheckResult as WebWatcherCheckResult;
        };

        It should_invoke_http_service_execute_async_method_only_once = () =>
            HttpServiceMock.Verify(x => x.ExecuteAsync(Moq.It.IsAny<string>(),
                Moq.It.IsAny<IHttpRequest>(), Moq.It.IsAny<TimeSpan?>()), Times.Once);

        It should_have_valid_check_result = () => CheckResult.IsValid.ShouldBeTrue();
        It should_have_check_result_of_type_web = () => WebCheckResult.ShouldNotBeNull();

        It should_have_set_values_in_web_check_result = () =>
        {
            WebCheckResult.WatcherName.ShouldNotBeEmpty();
            WebCheckResult.WatcherType.ShouldNotBeNull();
            WebCheckResult.Uri.ShouldNotBeNull();
            WebCheckResult.Request.ShouldNotBeNull();
            WebCheckResult.Response.ShouldNotBeNull();
        };
    }

    [Subject("Web watcher execution")]
    public class when_invoking_ensure_predicate_that_is_valid : WebWatcher_specs
    {
        static Mock<IHttpService> HttpServiceMock;
        static IHttpResponse Response;

        Establish context = () =>
        {
            Response = HttpResponse.Valid(HttpStatusCode.OK, "Ok", new Dictionary<string, string>(), string.Empty);
            HttpServiceMock = new Mock<IHttpService>();
            HttpServiceMock.Setup(x =>
                x.ExecuteAsync(Moq.It.IsAny<string>(), Moq.It.IsAny<IHttpRequest>(), Moq.It.IsAny<TimeSpan?>()))
                .ReturnsAsync(Response);

            Configuration = WebWatcherConfiguration
                .Create("http://website.com", HttpRequest.Get())
                .EnsureThat(response => response.ReasonPhrase == "Ok")
                .WithHttpServiceProvider(() => HttpServiceMock.Object)
                .Build();
            Watcher = WebWatcher.Create("Web watcher", Configuration);
        };

        Because of = async () =>
        {
            CheckResult = await Watcher.ExecuteAsync().Await().AsTask;
            WebCheckResult = CheckResult as WebWatcherCheckResult;
        };

        It should_invoke_http_service_execute_async_method_only_once = () =>
            HttpServiceMock.Verify(x => x.ExecuteAsync(Moq.It.IsAny<string>(),
                Moq.It.IsAny<IHttpRequest>(), Moq.It.IsAny<TimeSpan?>()), Times.Once);

        It should_have_valid_check_result = () => CheckResult.IsValid.ShouldBeTrue();
        It should_have_check_result_of_type_web = () => WebCheckResult.ShouldNotBeNull();

        It should_have_set_values_in_web_check_result = () =>
        {
            WebCheckResult.WatcherName.ShouldNotBeEmpty();
            WebCheckResult.WatcherType.ShouldNotBeNull();
            WebCheckResult.Uri.ShouldNotBeNull();
            WebCheckResult.Request.ShouldNotBeNull();
            WebCheckResult.Response.ShouldNotBeNull();
        };
    }

    [Subject("Web watcher execution")]
    public class when_invoking_ensure_async_predicate_that_is_valid : WebWatcher_specs
    {
        static Mock<IHttpService> HttpServiceMock;
        static IHttpResponse Response;

        Establish context = () =>
        {
            Response = HttpResponse.Valid(HttpStatusCode.OK, "Ok", new Dictionary<string, string>(), string.Empty);
            HttpServiceMock = new Mock<IHttpService>();
            HttpServiceMock.Setup(x =>
                x.ExecuteAsync(Moq.It.IsAny<string>(), Moq.It.IsAny<IHttpRequest>(), Moq.It.IsAny<TimeSpan?>()))
                .ReturnsAsync(Response);

            Configuration = WebWatcherConfiguration
                .Create("http://website.com", HttpRequest.Get())
                .EnsureThatAsync(response => Task.Factory.StartNew(() => response.ReasonPhrase == "Ok"))
                .WithHttpServiceProvider(() => HttpServiceMock.Object)
                .Build();
            Watcher = WebWatcher.Create("Web watcher", Configuration);
        };

        Because of = async () =>
        {
            CheckResult = await Watcher.ExecuteAsync().Await().AsTask;
            WebCheckResult = CheckResult as WebWatcherCheckResult;
        };

        It should_invoke_http_service_execute_async_method_only_once = () =>
            HttpServiceMock.Verify(x => x.ExecuteAsync(Moq.It.IsAny<string>(),
                Moq.It.IsAny<IHttpRequest>(), Moq.It.IsAny<TimeSpan?>()), Times.Once);

        It should_have_valid_check_result = () => CheckResult.IsValid.ShouldBeTrue();
        It should_have_check_result_of_type_web = () => WebCheckResult.ShouldNotBeNull();

        It should_have_set_values_in_web_check_result = () =>
        {
            WebCheckResult.WatcherName.ShouldNotBeEmpty();
            WebCheckResult.WatcherType.ShouldNotBeNull();
            WebCheckResult.Uri.ShouldNotBeNull();
            WebCheckResult.Request.ShouldNotBeNull();
            WebCheckResult.Response.ShouldNotBeNull();
        };
    }

    [Subject("Web watcher execution")]
    public class when_invoking_ensure_predicate_that_is_invalid : WebWatcher_specs
    {
        static Mock<IHttpService> HttpServiceMock;
        static IHttpResponse Response;

        Establish context = () =>
        {
            Response = HttpResponse.Valid(HttpStatusCode.OK, "Ok", new Dictionary<string, string>(), string.Empty);
            HttpServiceMock = new Mock<IHttpService>();
            HttpServiceMock.Setup(x =>
                x.ExecuteAsync(Moq.It.IsAny<string>(), Moq.It.IsAny<IHttpRequest>(), Moq.It.IsAny<TimeSpan?>()))
                .ReturnsAsync(Response);

            Configuration = WebWatcherConfiguration
                .Create("http://website.com", HttpRequest.Get())
                .EnsureThat(response => response.ReasonPhrase == "Not ok")
                .WithHttpServiceProvider(() => HttpServiceMock.Object)
                .Build();
            Watcher = WebWatcher.Create("Web watcher", Configuration);
        };

        Because of = async () =>
        {
            CheckResult = await Watcher.ExecuteAsync().Await().AsTask;
            WebCheckResult = CheckResult as WebWatcherCheckResult;
        };

        It should_invoke_http_service_execute_async_method_only_once = () =>
            HttpServiceMock.Verify(x => x.ExecuteAsync(Moq.It.IsAny<string>(),
                Moq.It.IsAny<IHttpRequest>(), Moq.It.IsAny<TimeSpan?>()), Times.Once);

        It should_have_invalid_check_result = () => CheckResult.IsValid.ShouldBeFalse();
        It should_have_check_result_of_type_web = () => WebCheckResult.ShouldNotBeNull();

        It should_have_set_values_in_web_check_result = () =>
        {
            WebCheckResult.WatcherName.ShouldNotBeEmpty();
            WebCheckResult.WatcherType.ShouldNotBeNull();
            WebCheckResult.Uri.ShouldNotBeNull();
            WebCheckResult.Request.ShouldNotBeNull();
            WebCheckResult.Response.ShouldNotBeNull();
        };
    }

    [Subject("Web watcher execution")]
    public class when_invoking_ensure_async_predicate_that_is_invalid : WebWatcher_specs
    {
        static Mock<IHttpService> HttpServiceMock;
        static IHttpResponse Response;

        Establish context = () =>
        {
            Response = HttpResponse.Valid(HttpStatusCode.OK, "Ok", new Dictionary<string, string>(), string.Empty);
            HttpServiceMock = new Mock<IHttpService>();
            HttpServiceMock.Setup(x =>
                x.ExecuteAsync(Moq.It.IsAny<string>(), Moq.It.IsAny<IHttpRequest>(), Moq.It.IsAny<TimeSpan?>()))
                .ReturnsAsync(Response);

            Configuration = WebWatcherConfiguration
                .Create("http://website.com", HttpRequest.Get())
                .EnsureThatAsync(response => Task.Factory.StartNew(() => response.ReasonPhrase == "Not ok"))
                .WithHttpServiceProvider(() => HttpServiceMock.Object)
                .Build();
            Watcher = WebWatcher.Create("Web watcher", Configuration);
        };

        Because of = async () =>
        {
            CheckResult = await Watcher.ExecuteAsync().Await().AsTask;
            WebCheckResult = CheckResult as WebWatcherCheckResult;
        };

        It should_invoke_http_service_execute_async_method_only_once = () =>
            HttpServiceMock.Verify(x => x.ExecuteAsync(Moq.It.IsAny<string>(),
                Moq.It.IsAny<IHttpRequest>(), Moq.It.IsAny<TimeSpan?>()), Times.Once);

        It should_have_invalid_check_result = () => CheckResult.IsValid.ShouldBeFalse();
        It should_have_check_result_of_type_web = () => WebCheckResult.ShouldNotBeNull();

        It should_have_set_values_in_web_check_result = () =>
        {
            WebCheckResult.WatcherName.ShouldNotBeEmpty();
            WebCheckResult.WatcherType.ShouldNotBeNull();
            WebCheckResult.Uri.ShouldNotBeNull();
            WebCheckResult.Request.ShouldNotBeNull();
            WebCheckResult.Response.ShouldNotBeNull();
        };
    }
}