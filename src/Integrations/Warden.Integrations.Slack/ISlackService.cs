using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Warden.Integrations.Slack
{
    /// <summary>
    /// Custom Slack client for integrating with the Webhook.
    /// </summary>
    public interface ISlackService
    {
        /// <summary>
        /// Executes the HTTP POST request.
        /// </summary>
        /// <param name="message">Message text.</param>
        /// <param name="channel">Optional name of channel to which the message will be sent.</param>
        /// <param name="username">Optional username that will send the message.</param>
        /// <param name="timeout">Optional timeout for the request.</param>
        /// <param name="failFast">Optional flag determining whether an exception should be thrown if received reponse is invalid (false by default).</param>
        /// <returns></returns>
        Task SendMessageAsync(string message, string channel = null, string username = null, 
            TimeSpan? timeout = null, bool failFast = false);
    }

    /// <summary>
    /// Default implementation of the ISlackService based on the HttpClient.
    /// </summary>
    public class SlackService : ISlackService
    {
        private readonly Uri _webhookUrl;
        private readonly HttpClient _httpClient = new HttpClient();

        public SlackService(Uri webhookUrl)
        {
            _webhookUrl = webhookUrl;
        }

        public async Task SendMessageAsync(string message, string channel = null, string username = null,
            TimeSpan? timeout = null, bool failFast = false)
        {
            SetTimeout(timeout);
            try
            {
                var payload = new
                {
                    text = message,
                    channel,
                    username,
                };
                var serializedPayload = JsonConvert.SerializeObject(payload);
                var response = await _httpClient.PostAsync(_webhookUrl, new StringContent(
                    serializedPayload, Encoding.UTF8, "application/json"));

                if (response.IsSuccessStatusCode)
                    return;
                if (!failFast)
                    return;

                throw new IntegrationException("Received invalid HTTP response from Slack API " +
                                               $"with status code: {response.StatusCode}. Reason phrase: {response.ReasonPhrase}");
            }
            catch (Exception exception)
            {
                if (!failFast)
                    return;

                throw new IntegrationException("There was an error while executing the SendMessageAsync(): " +
                                               $"{exception}", exception);
            }
        }

        private void SetTimeout(TimeSpan? timeout)
        {
            if (timeout > TimeSpan.Zero)
                _httpClient.Timeout = timeout.Value;
        }
    }
}