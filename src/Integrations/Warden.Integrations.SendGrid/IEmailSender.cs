using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Warden.Integrations.SendGrid
{
    /// <summary>
    /// Custom SendGrid email sender.
    /// </summary>
    public interface IEmailSender
    {
        /// <summary>
        /// Sends the SendGrid message with usage of the API key.
        /// </summary>
        /// <param name="apiKey">API key of the SendGrid account.</param>
        /// <param name="message">SendGrid message.</param>
        /// <returns></returns>
        Task SendMessageAsync(string apiKey, SendGridEmailMessage message);
    }

    /// <summary>
    /// Default implementation of the IEmailSender based on HttpClient.
    /// </summary>
    public class EmailSender : IEmailSender
    {
        private readonly HttpClient _httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://api.sendgrid.com/v3/")
        };

        public async Task SendMessageAsync(string apiKey, SendGridEmailMessage message)
        {
            _httpClient.DefaultRequestHeaders.Remove("Authorization");
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

            var payload = JsonConvert.SerializeObject(message);
            var content = new StringContent(payload, Encoding.UTF8, "application/json");
            await _httpClient.PostAsync("mail/send", content);
        }
    }
}