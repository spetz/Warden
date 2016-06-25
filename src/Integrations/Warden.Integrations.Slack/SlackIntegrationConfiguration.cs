using System;

namespace Warden.Integrations.Slack
{
    /// <summary>
    /// Configuration of the SlackIntegration.
    /// </summary>
    public class SlackIntegrationConfiguration
    {
        /// <summary>
        /// Full URL of the Slack webhook integration.
        /// </summary>
        public Uri WebhookUrl { get; protected set; }

        /// <summary>
        /// Default message text.
        /// </summary>
        public string DefaultMessage { get; protected set; }

        /// <summary>
        /// Default name of channel to which the message will be sent.
        /// </summary>
        public string DefaultChannel { get; protected set; }

        /// <summary>
        /// Default username that will send the message.
        /// </summary>
        public string DefaultUsername { get; protected set; }

        /// <summary>
        /// Optional timeout of the HTTP request to the Slack API.
        /// </summary>
        public TimeSpan? Timeout { get; protected set; }

        /// <summary>
        /// Flag determining whether an exception should be thrown if SendMessageAsync() returns invalid reponse (false by default).
        /// </summary>
        public bool FailFast { get; protected set; }

        /// <summary>
        /// Custom provider for the ISlackService.
        /// </summary>
        public Func<ISlackService> SlackServiceProvider { get; protected set; }

        /// <summary>
        /// Factory method for creating a new instance of fluent builder for the SlackIntegrationConfiguration.
        /// </summary>
        /// <param name="webhookUrl">Full URL of the Slack webhook integration.</param>
        /// <returns>Instance of fluent builder for the SlackIntegrationConfiguration.</returns>
        public static Builder Create(string webhookUrl) => new Builder(webhookUrl);

        protected SlackIntegrationConfiguration(string webhookUrl)
        {
            if (string.IsNullOrWhiteSpace(webhookUrl))
                throw new ArgumentException("Webhook URL can not be empty.", nameof(webhookUrl));

            WebhookUrl = new Uri(webhookUrl);
            SlackServiceProvider = () => new SlackService(WebhookUrl);
        }

        /// <summary>
        /// Fluent builder for the SlackIntegrationConfiguration.
        /// </summary>
        public class Builder
        {
            protected readonly SlackIntegrationConfiguration Configuration;

            /// <summary>
            /// Constructor of fluent builder for the SlackIntegrationConfiguration.
            /// </summary>
            /// <param name="webhookUrl">Full URL of the Slack webhook integration.</param>
            /// <returns>Instance of fluent builder for the SlackIntegrationConfiguration.</returns>
            public Builder(string webhookUrl)
            {
                Configuration = new SlackIntegrationConfiguration(webhookUrl);
            }

            /// <summary>
            /// Sets the default message text.
            /// </summary>
            /// <param name="message">Default message text.</param>
            /// <returns>Instance of fluent builder for the SlackIntegrationConfiguration.</returns>
            public Builder WithDefaultMessage(string message)
            {
                if (string.IsNullOrWhiteSpace(message))
                    throw new ArgumentException("Default message can not be empty.", nameof(message));

                Configuration.DefaultMessage = message;

                return this;
            }

            /// <summary>
            /// Sets the default channel to which the message will be sent.
            /// </summary>
            /// <param name="channel">Default name of channel to which the message will be sent.</param>
            /// <returns>Instance of fluent builder for the SlackIntegrationConfiguration.</returns>
            public Builder WithDefaultChannel(string channel)
            {
                if (string.IsNullOrWhiteSpace(channel))
                    throw new ArgumentException("Default channel can not be empty.", nameof(channel));

                Configuration.DefaultChannel = channel;

                return this;
            }

            /// <summary>
            /// Sets the default username that will send the message.
            /// </summary>
            /// <param name="username">Default username that will send the message.</param>
            /// <returns>Instance of fluent builder for the SlackIntegrationConfiguration.</returns>
            public Builder WithDefaultUsername(string username)
            {
                if (string.IsNullOrWhiteSpace(username))
                    throw new ArgumentException("Default username can not be empty.", nameof(username));

                Configuration.DefaultUsername = username;

                return this;
            }

            /// <summary>
            /// Timeout of the HTTP request to the Slack API.
            /// </summary>
            /// <param name="timeout">Timeout.</param>
            /// <returns>Instance of fluent builder for the SlackIntegrationConfiguration.</returns>
            public Builder WithTimeout(TimeSpan timeout)
            {
                if (timeout == null)
                    throw new ArgumentNullException(nameof(timeout), "Timeout can not be null.");

                if (timeout == TimeSpan.Zero)
                    throw new ArgumentException("Timeout can not be equal to zero.", nameof(timeout));

                Configuration.Timeout = timeout;

                return this;
            }

            /// <summary>
            /// Flag determining whether an exception should be thrown if SendMessageAsync() returns invalid reponse (false by default).
            /// </summary>
            /// <returns>Instance of fluent builder for the SlackIntegrationConfiguration.</returns>
            public Builder FailFast()
            {
                Configuration.FailFast = true;

                return this;
            }

            /// <summary>
            /// Sets the custom provider for the ISlackService.
            /// </summary>
            /// <param name="slackServiceProvider">Custom provider for the ISlackService.</param>
            /// <returns>Lambda expression returning an instance of the ISlackService.</returns>
            /// <returns>Instance of fluent builder for the SlackIntegrationConfiguration.</returns>
            public Builder WithSlackServiceProvider(Func<ISlackService> slackServiceProvider)
            {
                if (slackServiceProvider == null)
                {
                    throw new ArgumentNullException(nameof(slackServiceProvider),
                        "Slack service provider can not be null.");
                }

                Configuration.SlackServiceProvider = slackServiceProvider;

                return this;
            }

            /// <summary>
            /// Builds the SlackIntegrationConfiguration and return its instance.
            /// </summary>
            /// <returns>Instance of SlackIntegrationConfiguration.</returns>
            public SlackIntegrationConfiguration Build() => Configuration;
        }
    }
}