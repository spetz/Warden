using System;
using System.Linq;

namespace Warden.Integrations.Twilio
{
    /// <summary>
    /// Configuration of the TwilioIntegration.
    /// </summary>
    public class TwilioIntegrationConfiguration
    {
        /// <summary>
        /// SID of the Twilio account.
        /// </summary>
        public string AccountSid { get; protected set; }

        /// <summary>
        /// Authentication token of the Twilio account.
        /// </summary>
        public string AuthToken { get; protected set; }

        /// <summary>
        /// Phone number of the SMS sender.
        /// </summary>
        public string Sender { get; protected set; }

        /// <summary>
        /// Default receiver(s) number(s) of the SMS.
        /// </summary>
        public string[] DefaultReceivers { get; protected set; }

        /// <summary>
        /// Default body of the SMS.
        /// </summary>
        public string DefaultMessage { get; protected set; }

        /// <summary>
        /// Custom provider for the ITwilioService.
        /// </summary>
        public Func<ITwilioService> TwilioServiceProvider { get; protected set; }

        /// <summary>
        /// Factory method for creating a new instance of fluent builder for the TwilioIntegrationConfiguration.
        /// </summary>
        /// <param name="accountSid">SID of the Twilio account.</param>
        /// <param name="authToken">Authentication token of the Twilio account.</param>
        /// <param name="sender">Phone number of the SMS sender.</param>
        /// <returns>Instance of fluent builder for the TwilioIntegrationConfiguration.</returns>
        public static Builder Create(string accountSid, string authToken, string sender) => new Builder(accountSid, authToken, sender);

        protected TwilioIntegrationConfiguration(string accountSid, string authToken, string sender)
        {
            if (string.IsNullOrWhiteSpace(accountSid))
                throw new ArgumentException("Account SID can not be empty.", nameof(accountSid));
            if (string.IsNullOrWhiteSpace(authToken))
                throw new ArgumentException("Authentication token can not be empty.", nameof(authToken));
            if (string.IsNullOrWhiteSpace(sender))
                throw new ArgumentException("SMS sender can not be empty.", nameof(sender));

            AccountSid = accountSid;
            AuthToken = authToken;
            Sender = sender;
            DefaultReceivers = Enumerable.Empty<string>().ToArray();
        }

        /// <summary>
        /// Fluent builder for the TwilioIntegrationConfiguration.
        /// </summary>
        public class Builder
        {
            protected readonly TwilioIntegrationConfiguration Configuration;

            /// <summary>
            /// Constructor of fluent builder for the TwilioIntegrationConfiguration.
            /// </summary>
            /// <param name="accountSid">SID of the Twilio account.</param>
            /// <param name="authToken">Authentication token of the Twilio account.</param>
            /// <param name="sender">Phone number of the SMS sender.</param>
            /// <returns>Instance of fluent builder for the TwilioIntegrationConfiguration.</returns>
            public Builder(string accountSid, string authToken, string sender)
            {
                Configuration = new TwilioIntegrationConfiguration(accountSid, authToken, sender);
            }

            /// <summary>
            /// Sets the default body of the SMS.
            /// </summary>
            /// <param name="message">Default body of the SMS.</param>
            /// <returns>Instance of fluent builder for the TwilioIntegrationConfiguration.</returns>
            public Builder WithDefaultMessage(string message)
            {
                if (string.IsNullOrWhiteSpace(message))
                    throw new ArgumentException("Default message can not be empty.", nameof(message));

                Configuration.DefaultMessage = message;

                return this;
            }

            /// <summary>
            /// Sets the default receiver(s) number(s).
            /// </summary>
            /// <param name="receivers">Default receiver(s) number(s).</param>
            /// <returns>Instance of fluent builder for the TwilioIntegrationConfiguration.</returns>
            public Builder WithDefaultReceivers(params string[] receivers)
            {
                if (receivers == null || !receivers.Any())
                    throw new ArgumentException("Default receivers can not be empty.", nameof(receivers));
                if (receivers.Any(string.IsNullOrWhiteSpace))
                    throw new ArgumentException("Receiver(s) can not have empty number(s).", nameof(receivers));

                Configuration.DefaultReceivers = receivers;

                return this;
            }

            /// <summary>
            /// Sets the custom provider for the ITwilioService.
            /// </summary>
            /// <param name="twilioServiceProvider">Custom provider for the ITwilioService.</param>
            /// <returns>Lambda expression returning an instance of the ITwilioService.</returns>
            /// <returns>Instance of fluent builder for the TwilioIntegrationConfiguration.</returns>
            public Builder WithTwilioServiceProvider(Func<ITwilioService> twilioServiceProvider)
            {
                if (twilioServiceProvider == null)
                {
                    throw new ArgumentNullException(nameof(twilioServiceProvider),
                        "Twilio service provider can not be null.");
                }

                Configuration.TwilioServiceProvider = twilioServiceProvider;

                return this;
            }

            /// <summary>
            /// Builds the TwilioIntegrationConfiguration and return its instance.
            /// </summary>
            /// <returns>Instance of TwilioIntegrationConfiguration.</returns>
            public TwilioIntegrationConfiguration Build() => Configuration;
        }
    }
}