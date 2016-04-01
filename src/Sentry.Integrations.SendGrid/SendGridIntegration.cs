using System;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using SendGrid;

namespace Sentry.Integrations.SendGrid
{
    public class SendGridIntegration : IIntegration
    {
        private readonly SendGridIntegrationConfiguration _configuration;

        public SendGridIntegration(SendGridIntegrationConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync()
        {
            await SendEmailAsync(Enumerable.Empty<string>().ToArray());
        }

        public async Task SendEmailAsync(params string[] receivers)
        {
            await SendEmailAsync(null, null, null, receivers);
        }

        public async Task SendEmailAsync(string message, params string[] receivers)
        {
            await SendEmailAsync(null, null, message, receivers);
        }

        public async Task SendEmailAsync(string subject, string message, params string[] receivers)
        {
            await SendEmailAsync(null, subject, message, receivers);
        }

        public async Task SendEmailAsync(string sender = null, string subject = null,
            string message = null, params string[] receivers)
        {
            var customReceivers = receivers ?? Enumerable.Empty<string>();
            var emailReceivers = (_configuration.DefaultReceivers.Any()
                ? _configuration.DefaultReceivers.Union(customReceivers)
                : customReceivers).ToList();
            if (!emailReceivers.Any())
                throw new ArgumentException("No receivers defined.");

            var emailMessage = new SendGridMessage
            {
                From = new MailAddress(_configuration.DefaultSender.Or(sender)),
                Subject = _configuration.DefaultSubject.Or(subject)
            };
            //Space is required if template is being used
            var messageText = _configuration.DefaultMessage + message;
            emailMessage.Text = string.IsNullOrWhiteSpace(messageText) ? " " : messageText;
            emailMessage.AddTo(emailReceivers);

            var transportWeb = string.IsNullOrWhiteSpace(_configuration.ApiKey)
                ? new Web(new NetworkCredential(_configuration.Username, _configuration.Password))
                : new Web(_configuration.ApiKey);

            await transportWeb.DeliverAsync(emailMessage);
        }

        public static SendGridIntegration Create(string sender,
            Action<SendGridIntegrationConfiguration.Builder> configurator)
        {
            var config = new SendGridIntegrationConfiguration.Builder(sender);
            configurator?.Invoke(config);

            return Create(config.Build());
        }

        public static SendGridIntegration Create(SendGridIntegrationConfiguration configuration)
            => new SendGridIntegration(configuration);
    }
}
