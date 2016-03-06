using System;
using System.Threading.Tasks;
using FluentAssertions;
using Sentry.Watchers.Website;

namespace Sentry.Tests
{
    [Specification]
    public class WebsiteWatcher_specs : SpecificationBase
    {
        protected WebsiteWatcher WebsiteWatcher { get; set; }
        protected WebsiteWatcherConfiguration WebsiteWatcherConfiguration { get; set; }
    }

    [Specification]
    public class when_website_watcher_is_being_initialized_without_configuration : WebsiteWatcher_specs
    {
        protected override async Task EstablishContext()
        {
            await base.EstablishContext();
            ExceptionExpected = true;
        }

        protected override async Task BecauseOf()
        {
            await base.BecauseOf();
            WebsiteWatcherConfiguration = null;
            WebsiteWatcher = new WebsiteWatcher(WebsiteWatcherConfiguration);
        }

        [Then]
        public void then_exception_should_be_thrown()
        {
            ExceptionThrown.Should().BeAssignableTo<ArgumentNullException>();
            ExceptionThrown.Message.Should().StartWithEquivalent("WebsiteWatcher configuration has not been provided.");
        }
    }

    [Specification]
    public class when_website_watcher_is_being_initialized_with_configuration : WebsiteWatcher_specs
    {
        protected override async Task EstablishContext()
        {
            await base.EstablishContext();
        }

        protected override async Task BecauseOf()
        {
            await base.BecauseOf();
            WebsiteWatcherConfiguration = WebsiteWatcherConfiguration.Empty;
            WebsiteWatcher = new WebsiteWatcher(WebsiteWatcherConfiguration);
        }

        [Then]
        public void then_new_website_watcher_instance_should_be_created()
        {
            WebsiteWatcher.Should().NotBeNull();
        }
    }

    [Specification]
    public class WebsiteWatcherConfiguration_specs : SpecificationBase
    {
        protected WebsiteWatcherConfiguration WebsiteWatcherConfiguration { get; set; }
    }

    [Specification]
    public class when_website_watcher_configuration_is_being_initialized_with_invalid_url : WebsiteWatcherConfiguration_specs
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
                .WithUrl("test")
                .Build();
        }

        [Then]
        public void then_exception_should_be_thrown()
        {
            ExceptionThrown.Should().BeAssignableTo<UriFormatException>();
        }
    }
}