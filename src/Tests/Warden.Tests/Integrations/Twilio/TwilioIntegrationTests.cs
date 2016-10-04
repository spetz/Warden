using System;
using FluentAssertions;
using Moq;
using Warden.Integrations.Twilio;
using Machine.Specifications;
using It = Machine.Specifications.It;

namespace Warden.Tests.Integrations.Twilio
{
    public class TwilioIntegration_specs
    {
        protected static TwilioIntegration Integration { get; set; }
        protected static TwilioIntegrationConfiguration Configuration { get; set; }
        protected static Exception Exception { get; set; }
        protected static string AccountSid = "sid";
        protected static string AuthToken = "token";
        protected static string Sender = "+1123456789";
        protected static string[] Receivers = {"+1123456780", "+1123456781"};
    }

    [Subject("Twilio integration initialization")]
    public class when_initializing_without_configuration : TwilioIntegration_specs
    {
        Establish context = () => Configuration = null;

        Because of = () => Exception = Catch.Exception(() => Integration = TwilioIntegration.Create(Configuration));

        It should_fail = () => Exception.Should().BeOfType<ArgumentNullException>();

        It should_have_a_specific_reason =
            () => Exception.Message.Should().Contain("Twilio Integration configuration has not been provided.");
    }

    [Subject("Twilio integration initialization")]
    public class when_initializing_with_empty_account_sid : TwilioIntegration_specs
    {
        Establish context = () => { };

        Because of = () => Exception = Catch.Exception(() => Configuration = TwilioIntegrationConfiguration
            .Create(string.Empty, AuthToken, Sender)
            .Build());

        It should_fail = () => Exception.Should().BeOfType<ArgumentException>();

        It should_have_a_specific_reason =
            () => Exception.Message.Should().Contain("Account SID can not be empty.");
    }

    [Subject("Twilio integration initialization")]
    public class when_initializing_with_empty_auth_token : TwilioIntegration_specs
    {
        Establish context = () => { };

        Because of = () => Exception = Catch.Exception(() => Configuration = TwilioIntegrationConfiguration
            .Create(AccountSid, string.Empty, Sender)
            .Build());

        It should_fail = () => Exception.Should().BeOfType<ArgumentException>();

        It should_have_a_specific_reason =
            () => Exception.Message.Should().Contain("Authentication token can not be empty.");
    }

    [Subject("Twilio integration initialization")]
    public class when_initializing_with_empty_sender : TwilioIntegration_specs
    {
        Establish context = () => { };

        Because of = () => Exception = Catch.Exception(() => Configuration = TwilioIntegrationConfiguration
            .Create(AccountSid, AuthToken, string.Empty)
            .Build());

        It should_fail = () => Exception.Should().BeOfType<ArgumentException>();

        It should_have_a_specific_reason =
            () => Exception.Message.Should().Contain("SMS sender can not be empty.");
    }

    [Subject("Twilio integration execution")]
    public class when_invoking_send_sms_method_with_empty_body : TwilioIntegration_specs
    {
        Establish context = () =>
        {
            Configuration = TwilioIntegrationConfiguration
                .Create(AccountSid, AuthToken, Sender)
                .WithDefaultMessage("Test")
                .Build();
            Integration = TwilioIntegration.Create(Configuration);
        };

        Because of = () => Exception = Catch.Exception(() => Integration.SendSmsAsync().Await().AsTask);

        It should_fail = () => Exception.Should().BeOfType<ArgumentException>();

        It should_have_a_specific_reason =
            () => Exception.Message.Should().Contain("SMS receiver(s) have not been defined.");
    }

    [Subject("Twilio integration execution")]
    public class when_invoking_send_sms_method_without_receivers : TwilioIntegration_specs
    {
        Establish context = () =>
        {
            Configuration = TwilioIntegrationConfiguration
                .Create(AccountSid, AuthToken, Sender)
                .Build();
            Integration = TwilioIntegration.Create(Configuration);
        };

        Because of = () => Exception = Catch.Exception(() => Integration.SendSmsAsync().Await().AsTask);

        It should_fail = () => Exception.Should().BeOfType<ArgumentException>();

        It should_have_a_specific_reason =
            () => Exception.Message.Should().Contain("SMS body has not been defined.");
    }

    [Subject("Twilio integration execution")]
    public class when_invoking_send_sms_method_with_valid_configuration : TwilioIntegration_specs
    {
        static Mock<ITwilioService> TwilioServiceMock;

        Establish context = () =>
        {
            TwilioServiceMock = new Mock<ITwilioService>();
            Configuration = TwilioIntegrationConfiguration
                .Create(AccountSid, AuthToken, Sender)
                .WithDefaultMessage("Test")
                .WithDefaultReceivers(Receivers)
                .WithTwilioServiceProvider(() => TwilioServiceMock.Object)
                .Build();
            Integration = TwilioIntegration.Create(Configuration);
        };

        Because of = async () =>
        {
            await Integration.SendSmsAsync().Await().AsTask;
        };

        It should_invoke_send_message_async_method_twice = () => TwilioServiceMock.Verify(x =>
            x.SendSmsAsync(Moq.It.IsAny<string>(), Moq.It.IsAny<string>(), Moq.It.IsAny<string>()), Times.Exactly(Receivers.Length));
    }
}