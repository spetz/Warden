using System;
using System.Linq;
using System.Threading.Tasks;

namespace Warden.Integrations.Twilio
{
    /// <summary>
    /// Integration with the Twilio - SMS service.
    /// </summary>
    public class TwilioIntegration : IIntegration
    {
        private readonly TwilioIntegrationConfiguration _configuration;

        public TwilioIntegration(TwilioIntegrationConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration),
                    "Twilio Integration configuration has not been provided.");
            }

            _configuration = configuration;
        }

        /// <summary>
        /// Sends SMS to the default receiver(s) number(s), using a default message body.
        /// </summary>
        /// <returns></returns>
        public async Task SendSmsAsync()
        {
            await SendSmsAsync(null);
        }

        /// <summary>
        /// Sends SMS to the default receiver(s) number(s).
        /// </summary>
        /// <param name="message">Body of the SMS. If default message has been set, it will override its value.</param>
        /// <returns></returns>
        public async Task SendSmsAsync(string message)
        {
            await SendSmsAsync(message, string.Empty);
        }

        /// <summary>
        /// Sends SMS.
        /// </summary>
        /// <param name="message">Body of the SMS. If default message has been set, it will override its value.</param>
        /// <param name="receivers">Receiver(s) phone number(s) of the SMS. If default receivers have been set, it will merge these 2 lists.</param>
        /// <returns></returns>
        public async Task SendSmsAsync(string message, params string[] receivers)
        {
            var smsBody = string.IsNullOrWhiteSpace(message) ? _configuration.DefaultMessage : message;
            if (string.IsNullOrWhiteSpace(smsBody))
                throw new ArgumentException("SMS body has not been defined.", nameof(smsBody));

            var customReceivers = receivers ?? Enumerable.Empty<string>();
            var smsReceivers = (_configuration.DefaultReceivers.Any()
                ? _configuration.DefaultReceivers.Union(customReceivers)
                : customReceivers).ToList();
            if (!smsReceivers.Any())
                throw new ArgumentException("SMS receiver(s) have not been defined.", nameof(smsReceivers));

            var twilioService = _configuration.TwilioServiceProvider();
            var sendSmsTasks = smsReceivers.Select(receiver =>
                twilioService.SendSmsAsync(_configuration.Sender, receiver, smsBody));
            await Task.WhenAll(sendSmsTasks);
        }

        /// <summary>
        /// Factory method for creating a new instance of TwilioIntegration.
        /// </summary>
        /// <param name="accountSid">SID of the Twilio account.</param>
        /// <param name="authToken">Authentication token of the Twilio account.</param>
        /// <param name="sender">Phone number of the SMS sender.</param>
        /// <param name="configurator">Lambda expression for configuring Twilio integration.</param>
        /// <returns>Instance of TwilioIntegration.</returns>
        public static TwilioIntegration Create(string accountSid, string authToken, string sender,
            Action<TwilioIntegrationConfiguration.Builder> configurator)
        {
            var config = new TwilioIntegrationConfiguration.Builder(accountSid, authToken, sender);
            configurator?.Invoke(config);

            return Create(config.Build());
        }

        /// <summary>
        /// Factory method for creating a new instance of TwilioIntegration.
        /// </summary>
        /// <param name="configuration">Configuration of Twilio integration.</param>
        /// <returns>Instance of TwilioIntegration.</returns>
        public static TwilioIntegration Create(TwilioIntegrationConfiguration configuration)
            => new TwilioIntegration(configuration);
    }
}