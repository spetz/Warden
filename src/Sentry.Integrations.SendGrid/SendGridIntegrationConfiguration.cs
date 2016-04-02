using System;
using System.Linq;

namespace Sentry.Integrations.SendGrid
{
    public class SendGridIntegrationConfiguration
    {
        public string Username { get; protected set; }
        public string Password { get; protected set; }
        public string ApiKey { get; protected set; }
        public string DefaultSender { get; protected set; }
        public string[] DefaultReceivers { get; protected set; }
        public string DefaultSubject { get; protected set; }
        public string DefaultMessage { get; protected set; }
        public string DefaultTemplateId { get; protected set; }
        public EmailTemplateParameter[] DefaultTemplateParameters { get; protected set; }

        protected SendGridIntegrationConfiguration(string defaultSender = null)
        {
            DefaultSender = defaultSender;
        }

        public static Builder Create(string sender) => new Builder(sender);

        public class Builder
        {
            protected readonly SendGridIntegrationConfiguration Configuration;

            public Builder(string sender)
            {
                Configuration = new SendGridIntegrationConfiguration(sender);
            }

            public Builder WithDefaultSubject(string subject)
            {
                if(string.IsNullOrWhiteSpace(subject))
                    throw new ArgumentException("Default subject can not be empty", nameof(subject));

                Configuration.DefaultSubject = subject;

                return this;
            }

            public Builder WithDefaultMessage(string message)
            {
                if (string.IsNullOrWhiteSpace(message))
                    throw new ArgumentException("Default message can not be empty", nameof(message));

                Configuration.DefaultMessage = message;

                return this;
            }

            public Builder WithDefaultReceivers(params string[] receivers)
            {
                if (receivers?.Any() == false)
                    throw new ArgumentException("Default receivers can not be empty", nameof(receivers));

                Configuration.DefaultReceivers = receivers ?? Enumerable.Empty<string>().ToArray();

                return this;
            }

            public Builder WithDefaultTemplateId(string templateId)
            {
                if (string.IsNullOrWhiteSpace(templateId))
                    throw new ArgumentException("Default template id can not be empty", nameof(templateId));

                Configuration.DefaultTemplateId = templateId;

                return this;
            }

            public Builder WithDefaultTemplateParameters(params EmailTemplateParameter[] parameters)
            {
                if (parameters?.Any() == false)
                    throw new ArgumentException("Default template parameters can not be empty", nameof(parameters));

                Configuration.DefaultTemplateParameters = parameters;

                return this;
            }

            public Builder WithCredentials(string username, string password)
            {
                if (string.IsNullOrWhiteSpace(username))
                    throw new ArgumentException("Username can not be empty", nameof(username));
                if (string.IsNullOrWhiteSpace(password))
                    throw new ArgumentException("Password can not be empty", nameof(password));

                Configuration.Username = username;
                Configuration.Password = password;

                return this;
            }

            public Builder WithApiKey(string apiKey)
            {
                if (string.IsNullOrWhiteSpace(apiKey))
                    throw new ArgumentException("Api key can not be empty", nameof(apiKey));

                Configuration.ApiKey = apiKey;

                return this;
            }

            public SendGridIntegrationConfiguration Build() => Configuration;
        }
    }
}