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
        Task PostMessageAsync(string text, string username = null, string channel = null);
    }

    /// <summary>
    /// Default implementation of the ISlackService based on the HttpClient.
    /// </summary>
    public class SlackService : ISlackService
    {
        private readonly Uri _uri;
        private readonly HttpClient _httpClient = new HttpClient();

        public SlackService(string webhookUrl)
        {
            _uri = new Uri(webhookUrl);
        }

        public async Task PostMessageAsync(string text, string username = null, string channel = null)
        {
            var payload = new
            {
                channel,
                username,
                text
            };
            var serializedPayload = JsonConvert.SerializeObject(payload);
            var response = await _httpClient.PostAsync(_uri, new StringContent(
                serializedPayload, Encoding.UTF8, "application/json"));
        }
    }
}