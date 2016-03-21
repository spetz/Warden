using System;
using Machine.Specifications;
using Sentry.Watchers.Website;

namespace Sentry.Tests.Unit.Watchers
{
    public class WebsiteWatcher_specs
    {
        protected static WebsiteWatcher Watcher { get; set; }
        protected static WebsiteWatcherConfiguration Configuration { get; set; }
        protected static Exception Exception { get; set; }
    }

    [Subject("Website watcher initialization")]
    public class when_initializing_without_configuration : WebsiteWatcher_specs
    {
        Establish context = () => Configuration = null;

        Because of = () => Exception = Catch.Exception(() => Watcher = WebsiteWatcher.Create("test", Configuration));

        It should_fail = () => Exception.ShouldBeOfExactType<ArgumentNullException>();
        It should_have_a_specific_reason = () => Exception.Message.ShouldContain("WebsiteWatcher configuration has not been provided.");
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
}