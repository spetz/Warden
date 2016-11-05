using System;
using System.Linq;

namespace Warden.Integrations.SendGrid
{
    /// <summary>
    /// Configuration of the SendGridIntegration.
    /// </summary>
    public class SendGridIntegrationConfiguration
    {
        /// <summary>
        /// API key of the SendGrid account.
        /// </summary>
        public string ApiKey { get; protected set; }

        /// <summary>
        /// Sender email address.
        /// </summary>
        public string Sender { get; protected set; }

        /// <summary>
        /// Default receiver(s) email address(es).
        /// </summary>
        public string[] DefaultReceivers { get; protected set; }

        /// <summary>
        /// Default subject of the email message.
        /// </summary>
        public string DefaultSubject { get; protected set; }

        /// <summary>
        /// Default body of the email message.
        /// </summary>
        public string DefaultMessage { get; protected set; }

        /// <summary>
        /// Default template id of the transactional template.
        /// </summary>
        public string DefaultTemplateId { get; protected set; }

        /// <summary>
        /// Default parameters of the transactional template.
        /// </summary>
        public EmailTemplateParameter[] DefaultTemplateParameters { get; protected set; }

        /// <summary>
        /// Flag determining whether the message body should be of HTML type.
        /// </summary>
        public bool UseHtmlBody { get; protected set; }

        /// <summary>
        /// Custom provider for the IEmailSender.
        /// </summary>
        public Func<IEmailSender> EmailSenderProvider { get; protected set; }

        /// <summary>
        /// Factory method for creating a new instance of fluent builder for the SendGridIntegrationConfiguration.
        /// </summary>
        /// <param name="apiKey">API key of the SendGrid account.</param>
        /// <param name="sender">Email address of the message sender.</param>
        /// <returns>Instance of fluent builder for the SendGridIntegrationConfiguration.</returns>
        public static Builder Create(string apiKey, string sender) => new Builder(apiKey, sender);

        protected SendGridIntegrationConfiguration(string apiKey, string sender)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
                throw new ArgumentException("API key can not be empty.", nameof(apiKey));

            ValidateAndSetDefaultParameters(sender);
            ApiKey = apiKey;
        }

        private void ValidateAndSetDefaultParameters(string sender)
        {
            if (string.IsNullOrWhiteSpace(sender))
                throw new ArgumentException("Email message sender can not be empty.", nameof(sender));
            if (!sender.IsEmail())
                throw new ArgumentException("Invalid email of the message sender.", nameof(sender));

            Sender = sender;
            EmailSenderProvider = () => new EmailSender();
            DefaultReceivers = Enumerable.Empty<string>().ToArray();
            DefaultTemplateParameters = Enumerable.Empty<EmailTemplateParameter>().ToArray();
        }

        /// <summary>
        /// Fluent builder for the SendGridIntegrationConfiguration.
        /// </summary>
        public class Builder
        {
            protected readonly SendGridIntegrationConfiguration Configuration;

            public Builder(string apiKey, string sender)
            {
                Configuration = new SendGridIntegrationConfiguration(apiKey, sender);
            }

            /// <summary>
            /// Sets the default subject of the email message.
            /// </summary>
            /// <param name="subject">Default subject of the email message.</param>
            /// <returns>Instance of fluent builder for the SendGridIntegrationConfiguration.</returns>
            public Builder WithDefaultSubject(string subject)
            {
                if (string.IsNullOrWhiteSpace(subject))
                    throw new ArgumentException("Default subject can not be empty", nameof(subject));

                Configuration.DefaultSubject = subject;

                return this;
            }

            /// <summary>
            /// Sets the default body of the email message.
            /// </summary>
            /// <param name="message">Default body of the email message.</param>
            /// <returns>Instance of fluent builder for the SendGridIntegrationConfiguration.</returns>
            public Builder WithDefaultMessage(string message)
            {
                if (string.IsNullOrWhiteSpace(message))
                    throw new ArgumentException("Default message can not be empty.", nameof(message));

                Configuration.DefaultMessage = message;

                return this;
            }

            /// <summary>
            /// Sets the default receiver(s) email address(es).
            /// </summary>
            /// <param name="receivers">Default receiver(s) email address(es).</param>
            /// <returns>Instance of fluent builder for the SendGridIntegrationConfiguration.</returns>
            public Builder WithDefaultReceivers(params string[] receivers)
            {
                if (receivers?.Any() == false)
                    throw new ArgumentException("Default receivers can not be empty.", nameof(receivers));

                receivers.ValidateEmails(nameof(receivers));
                Configuration.DefaultReceivers = receivers;

                return this;
            }

            /// <summary>
            /// Sets the default template id of the transactional template.
            /// </summary>
            /// <param name="templateId">Default template id of the transactional template.</param>
            /// <returns>Instance of fluent builder for the SendGridIntegrationConfiguration.</returns>
            public Builder WithDefaultTemplateId(string templateId)
            {
                if (string.IsNullOrWhiteSpace(templateId))
                    throw new ArgumentException("Default template id can not be empty.", nameof(templateId));

                Configuration.DefaultTemplateId = templateId;

                return this;
            }

            /// <summary>
            /// Sets the default parameters of the transactional template.
            /// </summary>
            /// <param name="parameters">Default parameters of the transactional template.</param>
            /// <returns>Instance of fluent builder for the SendGridIntegrationConfiguration.</returns>
            public Builder WithDefaultTemplateParameters(params EmailTemplateParameter[] parameters)
            {
                if (parameters?.Any() == false)
                    throw new ArgumentException("Default template parameters can not be empty.", nameof(parameters));

                Configuration.DefaultTemplateParameters = parameters;

                return this;
            }

            /// <summary>
            /// Sets the flag determining whether the message body should be of HTML type.
            /// </summary>
            /// <returns>Instance of fluent builder for the SendGridIntegrationConfiguration.</returns>
            public Builder WithHtmlBody()
            {
                Configuration.UseHtmlBody = true;

                return this;
            }

            /// <summary>
            /// Sets the custom provider for the IEmailSender.
            /// </summary>
            /// <param name="emailSenderProvider">Custom provider for the IEmailSender.</param>
            /// <returns>Lambda expression returning an instance of the IEmailSender.</returns>
            /// <returns>Instance of fluent builder for the SendGridIntegrationConfiguration.</returns>
            public Builder WithEmailSenderProvider(Func<IEmailSender> emailSenderProvider)
            {
                if (emailSenderProvider == null)
                {
                    throw new ArgumentNullException(nameof(emailSenderProvider),
                        "Email sender provider can not be null.");
                }

                Configuration.EmailSenderProvider = emailSenderProvider;

                return this;
            }

            /// <summary>
            /// Builds the SendGridIntegrationConfiguration and return its instance.
            /// </summary>
            /// <returns>Instance of SendGridIntegrationConfiguration.</returns>
            public SendGridIntegrationConfiguration Build() => Configuration;
        }
    }
}