using System;
using FluentAssertions;
using Moq;
using Warden.Integrations.SendGrid;
using Machine.Specifications;
using SendGrid;
using It = Machine.Specifications.It;

namespace Warden.Tests.Integrations.SendGrid
{
    public class SendGridIntegration_specs
    {
        protected static SendGridIntegration Integration { get; set; }
        protected static SendGridIntegrationConfiguration Configuration { get; set; }
        protected static Exception Exception { get; set; }
        protected static string Sender = "noreply@email.com";
        protected static string ApiKey = "api-key";
        protected static string Username = "test";
        protected static string Password = "test";
        protected static string[] Receivers = { "test1@email.com", "test2@email.com" };
    }

    [Subject("SendGrid integration initialization")]
    public class when_initializing_without_configuration : SendGridIntegration_specs
    {
        Establish context = () => Configuration = null;

        Because of = () => Exception = Catch.Exception(() => Integration = SendGridIntegration.Create(Configuration));

        It should_fail = () => Exception.Should().BeOfType<ArgumentNullException>();

        It should_have_a_specific_reason =
            () => Exception.Message.Should().Contain("SendGrid Integration configuration has not been provided.");
    }

    [Subject("SendGrid integration initialization")]
    public class when_initializing_with_empty_api_key : SendGridIntegration_specs
    {
        Establish context = () => { };

        Because of = () => Exception = Catch.Exception(() => Configuration = SendGridIntegrationConfiguration
            .Create(string.Empty, Sender)
            .Build());

        It should_fail = () => Exception.Should().BeOfType<ArgumentException>();

        It should_have_a_specific_reason =
            () => Exception.Message.Should().Contain("API key can not be empty.");
    }

    [Subject("SendGrid integration initialization")]
    public class when_initializing_with_empty_username : SendGridIntegration_specs
    {
        Establish context = () => { };

        Because of = () => Exception = Catch.Exception(() => Configuration = SendGridIntegrationConfiguration
            .Create(string.Empty, Password, Sender)
            .Build());

        It should_fail = () => Exception.Should().BeOfType<ArgumentException>();

        It should_have_a_specific_reason =
            () => Exception.Message.Should().Contain("Username can not be empty.");
    }

    [Subject("SendGrid integration initialization")]
    public class when_initializing_with_empty_password : SendGridIntegration_specs
    {
        Establish context = () => { };

        Because of = () => Exception = Catch.Exception(() => Configuration = SendGridIntegrationConfiguration
            .Create(Username, string.Empty, Sender)
            .Build());

        It should_fail = () => Exception.Should().BeOfType<ArgumentException>();

        It should_have_a_specific_reason =
            () => Exception.Message.Should().Contain("Password can not be empty.");
    }

    [Subject("SendGrid integration initialization")]
    public class when_initializing_with_invalid_sender : SendGridIntegration_specs
    {
        Establish context = () => { };

        Because of = () => Exception = Catch.Exception(() => Configuration = SendGridIntegrationConfiguration
            .Create(ApiKey, "invalid")
            .Build());

        It should_fail = () => Exception.Should().BeOfType<ArgumentException>();

        It should_have_a_specific_reason =
            () => Exception.Message.Should().Contain("Invalid email of the message sender.");
    }

    [Subject("SendGrid integration initialization")]
    public class when_initializing_with_invalid_receiver : SendGridIntegration_specs
    {
        static string InvalidReceiver = "invalid";

        Establish context = () => { };

        Because of = () => Exception = Catch.Exception(() => Configuration = SendGridIntegrationConfiguration
            .Create(ApiKey, Sender)
            .WithDefaultReceivers(InvalidReceiver)
            .Build());

        It should_fail = () => Exception.Should().BeOfType<ArgumentException>();

        It should_have_a_specific_reason =
            () => Exception.Message.Should().Contain($"Invalid email(s): {InvalidReceiver}.");
    }

    [Subject("SendGrid integration execution")]
    public class when_invoking_send_email_method_with_valid_configuration_for_api_key : SendGridIntegration_specs
    {
        static Mock<IEmailSender> EmailSenderMock;

        Establish context = () =>
        {
            EmailSenderMock = new Mock<IEmailSender>();
            Configuration = SendGridIntegrationConfiguration
                .Create(ApiKey, Sender)
                .WithDefaultReceivers(Receivers)
                .WithEmailSenderProvider(() => EmailSenderMock.Object)
                .Build();
            Integration = SendGridIntegration.Create(Configuration);
        };

        Because of = async () =>
        {
            await Integration.SendEmailAsync().Await().AsTask;
        };

        It should_invoke_send_message_async_method_only_once = () => EmailSenderMock.Verify(x =>
            x.SendMessageAsync(Moq.It.IsAny<string>(), Moq.It.IsAny<SendGridMessage>()), Times.Once);
    }

    [Subject("SendGrid integration execution")]
    public class when_invoking_send_email_method_with_valid_configuration_for_credentials : SendGridIntegration_specs
    {
        static Mock<IEmailSender> EmailSenderMock;

        Establish context = () =>
        {
            EmailSenderMock = new Mock<IEmailSender>();
            Configuration = SendGridIntegrationConfiguration
                .Create(Username, Password, Sender)
                .WithDefaultReceivers(Receivers)
                .WithEmailSenderProvider(() => EmailSenderMock.Object)
                .Build();
            Integration = SendGridIntegration.Create(Configuration);
        };

        Because of = async () =>
        {
            await Integration.SendEmailAsync().Await().AsTask;
        };

        It should_invoke_send_message_async_method_only_once = () => EmailSenderMock.Verify(x =>
            x.SendMessageAsync(Moq.It.IsAny<string>(), Moq.It.IsAny<string>(),
                Moq.It.IsAny<SendGridMessage>()), Times.Once);
    }

    [Subject("SendGrid integration execution")]
    public class when_invoking_send_email_method_with_valid_configuration_for_api_key_but_without_receivers : SendGridIntegration_specs
    {
        static Mock<IEmailSender> EmailSenderMock;

        Establish context = () =>
        {
            EmailSenderMock = new Mock<IEmailSender>();
            Configuration = SendGridIntegrationConfiguration
                .Create(ApiKey, Sender)
                .WithEmailSenderProvider(() => EmailSenderMock.Object)
                .Build();
            Integration = SendGridIntegration.Create(Configuration);
        };

        Because of = () => Exception = Catch.Exception(() => Integration.SendEmailAsync().Await().AsTask);

        It should_fail = () => Exception.Should().BeOfType<ArgumentException>();

        It should_have_a_specific_reason =
            () => Exception.Message.Should().Contain("Email message receivers have not been defined.");

        It should_not_invoke_send_message_async_method = () => EmailSenderMock.Verify(x =>
            x.SendMessageAsync(Moq.It.IsAny<string>(), Moq.It.IsAny<SendGridMessage>()), Times.Never);
    }

    [Subject("SendGrid integration execution")]
    public class when_invoking_send_email_method_with_valid_configuration_for_credentials_without_receivers :
        SendGridIntegration_specs
    {
        static Mock<IEmailSender> EmailSenderMock;

        Establish context = () =>
        {
            EmailSenderMock = new Mock<IEmailSender>();
            Configuration = SendGridIntegrationConfiguration
                .Create(Username, Password, Sender)
                .WithEmailSenderProvider(() => EmailSenderMock.Object)
                .Build();
            Integration = SendGridIntegration.Create(Configuration);
        };

        Because of = () => Exception = Catch.Exception(() => Integration.SendEmailAsync().Await().AsTask);

        It should_fail = () => Exception.Should().BeOfType<ArgumentException>();

        It should_have_a_specific_reason =
            () => Exception.Message.Should().Contain("Email message receivers have not been defined.");

        It should_not_invoke_send_message_async_method = () => EmailSenderMock.Verify(x =>
            x.SendMessageAsync(Moq.It.IsAny<string>(), Moq.It.IsAny<string>(),
                Moq.It.IsAny<SendGridMessage>()), Times.Never);
    }
}