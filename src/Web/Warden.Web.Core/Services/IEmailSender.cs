using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using NLog;
using SendGrid;
using Warden.Web.Core.Extensions;
using Warden.Web.Core.Settings;

namespace Warden.Web.Core.Services
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string sender, string receiver, string subject,
            string message);

        Task SendTemplatedEmailAsync(string templateName, string sender,
            string receiver, IDictionary<string, IEnumerable<string>> parameters = null);

        Task SendAccountCreatedEmailAsync(string receiver);
    }

    public class SendGridEmailSender : IEmailSender
    {
        private readonly EmailSettings _emailSettings;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private const string AccountCreatedTemplateName = "account-created";

        public SendGridEmailSender(EmailSettings emailSettings)
        {
            _emailSettings = emailSettings;
        }

        public async Task SendEmailAsync(string sender, string receiver, string subject, string message)
        {
            if (IsSenderDisabled())
                return;

            var emailMessage = CreateMessage(receiver, sender, subject, message);
            await SendMessageAsync(emailMessage);
            Logger.Trace($"Email message has been sent -> sender: {sender}, receiver: {receiver}");
        }

        public async Task SendTemplatedEmailAsync(string templateName, string sender, string receiver,
            IDictionary<string, IEnumerable<string>> parameters = null)
        {
            if (IsSenderDisabled())
                return;

            var template = _emailSettings.Templates?.FirstOrDefault(x => x.Name.EqualsCaseInvariant(templateName));
            if (template == null)
            {
                Logger.Error($"Email template: {templateName} has not been found. Message will not be sent.");

                return;
            }

            var emailMessage = CreateMessage(receiver, sender, template.Subject);
            emailMessage.Html = "<span></span>";
            emailMessage.EnableTemplateEngine(template.Id);
            if (parameters != null)
            {
                foreach (var parameter in parameters)
                {
                    emailMessage.AddSubstitution(parameter.Key, parameter.Value.ToList());
                }
            }
            await SendMessageAsync(emailMessage);
            Logger.Trace($"Templated email message has been sent -> template: {template.Name}, " +
                         $"sender: {sender}, receiver: {receiver}");
        }

        public async Task SendAccountCreatedEmailAsync(string receiver)
        {
            await SendTemplatedEmailAsync(AccountCreatedTemplateName, _emailSettings.NoReplyAccount, receiver);
        }

        private bool IsSenderDisabled()
        {
            var cannotSendMessage = !_emailSettings.Enabled;
            if (cannotSendMessage)
                Logger.Trace("Email sender is disabled. Message will not be sent.");

            return cannotSendMessage;
        }

        private SendGridMessage CreateMessage(string receiver, string sender,
            string subject, string message = null)
        {
            var emailMessage = new SendGridMessage();
            emailMessage.AddTo(receiver);
            emailMessage.From = new MailAddress(sender);
            emailMessage.Subject = subject;
            emailMessage.Text = message.Empty() ? " " : message;

            return emailMessage;
        }

        private async Task SendMessageAsync(SendGridMessage message)
        {
            var transportWeb = new SendGrid.Web(_emailSettings.ApiKey);
            await transportWeb.DeliverAsync(message);
        }
    }
}