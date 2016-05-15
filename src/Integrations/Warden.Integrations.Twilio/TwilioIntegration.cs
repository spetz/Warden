using System;
using System.Threading.Tasks;

namespace Warden.Integrations.Twilio
{
    /// <summary>
    /// Integration with the Twilio - SMS service.
    /// </summary>
    public class TwilioIntegration
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
        /// Sends SMS to the default receiver number, using a default sender number and default message body.
        /// </summary>
        /// <returns></returns>
        public async Task SendSmsAsync()
        {
            await SendSmsAsync(null);
        }

        /// <summary>
        /// Sends SMS to the default receiver number, using a default sender number.
        /// </summary>
        /// <param name="message">Body of the SMS. If default message has been set, it will override its value.</param>
        /// <returns></returns>
        public async Task SendSmsAsync(string message)
        {
            await SendSmsAsync(null, message);
        }

        /// <summary>
        /// Sends SMS to the default receiver number.
        /// </summary>
        /// <param name="receiver">Receiver phone number of the SMS. If default receiver has been set, it will override its value.</param>
        /// <param name="message">Body of the SMS. If default message has been set, it will override its value.</param>
        /// <returns></returns>
        public async Task SendSmsAsync(string receiver, string message)
        {
            await SendSmsAsync(null, receiver, message);
        }

        /// <summary>
        /// Sends email message.
        /// </summary>
        /// <param name="sender">Sender phone number of the SMS. If default sender has been set, it will override its value.</param>
        /// <param name="receiver">Receiver phone number of the SMS. If default receiver has been set, it will override its value.</param>
        /// <param name="message">Body of the SMS. If default message has been set, it will override its value.</param>
        /// <returns></returns>
        public async Task SendSmsAsync(string sender, string receiver, string message)
        {
            var twilioService = _configuration.TwilioServiceProvider();

            await twilioService.SendSmsAsync(sender, receiver, message);
        }
    }
}