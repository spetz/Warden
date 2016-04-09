using System.Net;
using System.Threading.Tasks;
using SendGrid;

namespace Warden.Integrations.SendGrid
{
    /// <summary>
    /// Custom SendGrid email sender.
    /// </summary>
    public interface IEmailSender
    {
        /// <summary>
        /// Sends the SendGrid message with usage of the credentials.
        /// </summary>
        /// <param name="username">Username of the SendGrid account.</param>
        /// <param name="password">Password of the SendGrid account.</param>
        /// <param name="message">SendGrid message.</param>
        /// <returns></returns>
        Task SendMessageAsync(string username, string password, SendGridMessage message);

        /// <summary>
        /// Sends the SendGrid message with usage of the API key.
        /// </summary>
        /// <param name="apiKey">API key of the SendGrid account.</param>
        /// <param name="message">SendGrid message.</param>
        /// <returns></returns>
        Task SendMessageAsync(string apiKey, SendGridMessage message);
    }

    /// <summary>
    /// Default implementation of the IEmailSender based on SendGrid.Web.
    /// </summary>
    public class EmailSender : IEmailSender
    {
        public async Task SendMessageAsync(string username, string password, SendGridMessage message)
        {
            var transportWeb = new Web(new NetworkCredential(username, password));
            await transportWeb.DeliverAsync(message);
        }

        public async Task SendMessageAsync(string apiKey, SendGridMessage message)
        {
            var transportWeb = new Web(apiKey);
            await transportWeb.DeliverAsync(message);
        }
    }
}