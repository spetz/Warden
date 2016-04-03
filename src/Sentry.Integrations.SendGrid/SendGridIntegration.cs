using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SendGrid;

namespace Sentry.Integrations.SendGrid
{
    public class SendGridIntegration : IIntegration
    {
        private static readonly Regex EmailRegex =
            new Regex(
                @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
                @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$",
                RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant);

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
            var emailMessage = CreateMessage(sender, subject, receivers);
            var body = _configuration.DefaultMessage + emailMessage;
            if (_configuration.UseHtmlBody)
                emailMessage.Html = body;
            else
                emailMessage.Text = body;

            await SendMessageAsync(emailMessage);
        }

        public async Task SendTemplatedEmailAsync()
        {
            await SendTemplatedEmailAsync(Enumerable.Empty<string>().ToArray());
        }

        public async Task SendTemplatedEmailAsync(params string[] receivers)
        {
            await SendTemplatedEmailAsync(null, null, null, null, receivers);
        }

        public async Task SendTemplatedEmailAsync(string templateId)
        {
            await SendTemplatedEmailAsync(null, null, templateId, null, Enumerable.Empty<string>().ToArray());
        }

        public async Task SendTemplatedEmailAsync(IEnumerable<EmailTemplateParameter> parameters)
        {
            await SendTemplatedEmailAsync(null, null, null, parameters);
        }

        public async Task SendTemplatedEmailAsync(string templateId, IEnumerable<EmailTemplateParameter> parameters)
        {
            await SendTemplatedEmailAsync(null, null, templateId, parameters, Enumerable.Empty<string>().ToArray());
        }

        public async Task SendTemplatedEmailAsync(string templateId, params string[] receivers)
        {
            await SendTemplatedEmailAsync(null, null, templateId, null, receivers);
        }

        public async Task SendTemplatedEmailAsync(string templateId, IEnumerable<EmailTemplateParameter> parameters,
            params string[] receivers)
        {
            await SendTemplatedEmailAsync(null, null, templateId, parameters, receivers);
        }

        public async Task SendTemplatedEmailAsync(string subject, string templateId,
            IEnumerable<EmailTemplateParameter> parameters = null, params string[] receivers)
        {
            await SendTemplatedEmailAsync(null, subject, templateId, parameters, receivers);
        }

        public async Task SendTemplatedEmailAsync(string sender = null, string subject = null,
            string templateId = null, IEnumerable<EmailTemplateParameter> parameters = null,
            params string[] receivers)
        {
            var emailMessage = CreateMessage(sender, subject, receivers);
            var emailTemplateId = _configuration.DefaultTemplateId.Or(templateId);
            //Space and some empty html tag are required if the template is being used
            emailMessage.Text = " ";
            emailMessage.Html = "<span></span>";
            emailMessage.EnableTemplateEngine(emailTemplateId);
            var customParameters = parameters ?? Enumerable.Empty<EmailTemplateParameter>();
            var templateParameters = (_configuration.DefaultTemplateParameters.Any()
                ? _configuration.DefaultTemplateParameters.Union(customParameters)
                : customParameters).ToList();

            foreach (var parameter in templateParameters)
            {
                emailMessage.AddSubstitution(parameter.ReplacementTag, parameter.Values.ToList());
            }

            await SendMessageAsync(emailMessage);
        }

        private SendGridMessage CreateMessage(string sender = null, string subject = null, params string[] receivers)
        {
            var emailSender = _configuration.DefaultSender.Or(sender);
            if (string.IsNullOrWhiteSpace(emailSender))
                throw new ArgumentException("Email message sender has not been defined.", nameof(emailSender));
            if(!EmailRegex.IsMatch(sender))
                throw new ArgumentException("Invalid email of the message sender.", nameof(emailSender));

            var customReceivers = receivers ?? Enumerable.Empty<string>();
            var emailReceivers = (_configuration.DefaultReceivers.Any()
                ? _configuration.DefaultReceivers.Union(customReceivers)
                : customReceivers).ToList();
            if (!emailReceivers.Any())
                throw new ArgumentException("Email message receivers have not been defined.", nameof(emailReceivers));
            var invalidEmailReceivers = emailReceivers.Where(x => !EmailRegex.IsMatch(x));
            if (invalidEmailReceivers.Any())
            {
                throw new ArgumentException("Invalid email(s) of the message receiver(s): " +
                                            $"{string.Join(",", invalidEmailReceivers)}", nameof(emailSender));
            }

            var message = new SendGridMessage
            {
                From = new MailAddress(emailSender),
                Subject = _configuration.DefaultSubject.Or(subject)
            };
            message.AddTo(emailReceivers);

            return message;
        }

        private async Task SendMessageAsync(SendGridMessage message)
        {
            var transportWeb = string.IsNullOrWhiteSpace(_configuration.ApiKey)
                ? new Web(new NetworkCredential(_configuration.Username, _configuration.Password))
                : new Web(_configuration.ApiKey);

            await transportWeb.DeliverAsync(message);
        }

        /// <summary>
        /// Factory method for creating a new instance of SendGridIntegration.
        /// </summary>
        /// <param name="sender">Email address of the message sender.</param>
        /// <param name="configurator">Lambda expression for configuring the SendGridIntegration.</param>
        /// <returns>Instance of SendGridIntegration.</returns>
        public static SendGridIntegration Create(string sender, Action<SendGridIntegrationConfiguration.Builder> configurator)
        {
            var config = new SendGridIntegrationConfiguration.Builder(sender);
            configurator?.Invoke(config);

            return Create(config.Build());
        }

        /// <summary>
        /// Factory method for creating a new instance of SendGridIntegration.
        /// </summary>
        /// <param name="configuration">Configuration of SendGridIntegration.</param>
        /// <returns>Instance of SendGridIntegration.</returns>
        public static SendGridIntegration Create(SendGridIntegrationConfiguration configuration)
            => new SendGridIntegration(configuration);
    }
}
