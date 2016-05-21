using System.Threading.Tasks;
using Twilio;

namespace Warden.Integrations.Twilio
{
    public interface ITwilioService
    {
        Task SendSmsAsync(string sender, string receiver, string message);
    }

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