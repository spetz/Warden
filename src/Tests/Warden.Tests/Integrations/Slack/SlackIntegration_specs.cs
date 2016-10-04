using System;
using FluentAssertions;
using Moq;
using Warden.Integrations.Slack;
using Machine.Specifications;
using It = Machine.Specifications.It;

namespace Warden.Tests.Integrations.Slack
{
    public class SlackIntegration_specs
    {
        protected static SlackIntegration Integration { get; set; }
        protected static SlackIntegrationConfiguration Configuration { get; set; }
        protected static Exception Exception { get; set; }
        protected static string WebhookUrl = "https://slack.com";
    }

    [Subject("Slack integration initialization")]
    public class when_initializing_without_configuration : SlackIntegration_specs
    {
        Establish context = () => Configuration = null;

        Because of = () => Exception = Catch.Exception(() => Integration = SlackIntegration.Create(Configuration));

        It should_fail = () => Exception.Should().BeOfType<ArgumentNullException>();

        It should_have_a_specific_reason =
            () => Exception.Message.Should().Contain("Slack Integration configuration has not been provided.");
    }

    [Subject("Slack integration initialization")]
    public class when_initializing_with_invalid_webhook_url : SlackIntegration_specs
    {
        Establish context = () => { };

        Because of = () => Exception = Catch.Exception(() => Configuration = SlackIntegrationConfiguration
            .Create("Invalid")
            .Build());

        It should_fail = () => Exception.Should().BeOfType<UriFormatException>();
    }

    [Subject("Slack integration execution")]
    public class when_invoking_send_message_async_method_with_valid_configuration : SlackIntegration_specs
    {
        static Mock<ISlackService> SlackServiceMock;

        Establish context = () =>
        {
            SlackServiceMock = new Mock<ISlackService>();
            Configuration = SlackIntegrationConfiguration
                .Create(WebhookUrl)
                .WithSlackServiceProvider(() => SlackServiceMock.Object)
                .Build();
            Integration = SlackIntegration.Create(Configuration);
        };

        Because of = async () => await Integration.SendMessageAsync().Await().AsTask;

        It should_invoke_send_message_async_method_only_once = () => SlackServiceMock.Verify(x =>
            x.SendMessageAsync(Moq.It.IsAny<string>(), Moq.It.IsAny<string>(), Moq.It.IsAny<string>(),
                Moq.It.IsAny<string>(), Moq.It.IsAny<TimeSpan?>(), Moq.It.IsAny<bool>()), Times.Once);
    }
}