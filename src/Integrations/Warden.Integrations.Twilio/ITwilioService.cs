using System.Threading.Tasks;
using Twilio;

namespace Warden.Integrations.Twilio
{
    /// <summary>
    /// Custom Twilio SMS service.
    /// </summary>
    public interface ITwilioService
    {
        /// <summary>
        /// Sends SMS.
        /// </summary>
        /// <param name="sender">Sender phone number.</param>
        /// <param name="receiver">Receiver phone number.</param>
        /// <param name="message">Body of the SMS.</param>
        /// <returns></returns>
        Task SendSmsAsync(string sender, string receiver, string message);
    }


    /// <summary>
    /// Default implementation of the ITwilioService based on Twilio library.
    /// </summary>
    public class TwilioService : ITwilioService
    {
        private readonly TwilioRestClient _twilioRestClient;

        public TwilioService(string accountSid, string authToken)
        {
            _twilioRestClient = new TwilioRestClient(accountSid, authToken);
        }

        public async Task SendSmsAsync(string sender, string receiver, string message)
        {
            await Task.FromResult(_twilioRestClient.SendMessage(sender, receiver, message));
        }
    }
}