using System;
using System.Threading.Tasks;
using FluentAssertions;
using Sentry.Core;

namespace Sentry.Tests
{
    [Specification]
    public class Sentry_specs : SpecificationBase
    {
        protected ISentry Sentry { get; set; }
        protected SentryConfiguration SentryConfiguration { get; set; }
    }

    [Specification]
    public class when_sentry_is_being_initialized_without_configuration : Sentry_specs
    {
        protected override async Task EstablishContext()
        {
            await base.EstablishContext();
            ExceptionExpected = true;
        }

        protected override async Task BecauseOf()
        {
            await base.BecauseOf();
            SentryConfiguration = null;
            Sentry = new Sentry(SentryConfiguration);
        }

        [Then]
        public void then_exception_should_be_thrown()
        {
            ExceptionThrown.Should().BeAssignableTo<ArgumentNullException>();
            ExceptionThrown.Message.Should().StartWithEquivalent("Sentry configuration has not been provided.");
        }
    }

    [Specification]
    public class when_sentry_is_being_initialized_with_configuration : Sentry_specs
    {
        protected override async Task EstablishContext()
        {
            await base.EstablishContext();
        }

        protected override async Task BecauseOf()
        {
            await base.BecauseOf();
            SentryConfiguration = SentryConfiguration.Empty;
            Sentry = new Sentry(SentryConfiguration);
        }

        [Then]
        public void then_new_sentry_instance_should_be_created()
        {
            Sentry.Should().NotBeNull();
        }
    }
}