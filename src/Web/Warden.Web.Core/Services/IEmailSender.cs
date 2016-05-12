using System;
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
        Task SendResetPasswordEmailAsync(string receiver, string token);
        Task SendPasswordChangedEmailAsync(string receiver);
    }

    public class SendGridEmailSender : IEmailSender
    {
        private readonly EmailSettings _emailSettings;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private const string AccountCreatedTemplateName = "account-created";
        private const string ResetPasswordTemplateName = "reset-password";
        private const string ResetPasswordParameterUrlName = "url";
        private const string PasswordChangedTemplateName = "password-changed";

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

        public async Task SendAccountCreatedEmailAsync(string receiver)
        {
            await SendTemplatedEmailAsync(AccountCreatedTemplateName, _emailSettings.NoReplyAccount, receiver);
        }

        public async Task SendResetPasswordEmailAsync(string receiver, string token)
        {
            var template = GetTemplate(ResetPasswordTemplateName);
            var urlParameter = template?.Parameters
                .FirstOrDefault(x => x.Name.EqualsCaseInvariant(ResetPasswordParameterUrlName));
            if (urlParameter == null)
                return;

            var baseUrl = urlParameter.Values.FirstOrDefault();
            if(baseUrl.Empty())
                return;

            var resetPasswordUrl = $"{baseUrl}?token={token}&email={receiver}";
            await SendTemplatedEmailAsync(ResetPasswordTemplateName, _emailSettings.NoReplyAccount, receiver,
                new Dictionary<string, IEnumerable<string>>
                {
                    [urlParameter.Name] = new List<string> {resetPasswordUrl}
                });
        }

        public async Task SendPasswordChangedEmailAsync(string receiver)
        {
            await SendTemplatedEmailAsync(PasswordChangedTemplateName, _emailSettings.NoReplyAccount, receiver);
        }

        public async Task SendTemplatedEmailAsync(string templateName, string sender, string receiver,
            IDictionary<string, IEnumerable<string>> parameters = null)
        {
            if (IsSenderDisabled())
                return;

            var template = GetTemplate(templateName);
            if (template == null)
                return;

            var emailMessage = CreateMessage(receiver, sender, template.Subject);
            emailMessage.Html = "<span></span>";
            emailMessage.EnableTemplateEngine(template.Id);
            var templateParameters = GetTemplateParameters(template, parameters);
            foreach (var parameter in templateParameters)
            {
                emailMessage.AddSubstitution($"-{parameter.Key}-", parameter.Value.ToList());
            }
            await SendMessageAsync(emailMessage);
            Logger.Trace($"Templated email message has been sent -> template: {template.Name}, " +
                         $"sender: {sender}, receiver: {receiver}");
        }

        private EmailTemplateSettings GetTemplate(string name)
        {
            var template = _emailSettings.Templates?.FirstOrDefault(x => x.Name.EqualsCaseInvariant(name));
            if (template == null)
                Logger.Error($"Email template: {name} has not been found. Message will not be sent.");

            return template;
        }

        private static IDictionary<string, IEnumerable<string>> GetTemplateParameters(EmailTemplateSettings template,
            IDictionary<string, IEnumerable<string>> parameters)
            => (parameters?.Any() == true
                ? parameters.ToDictionary(x => x.Key, x => x.Value)
                : template.Parameters?.ToDictionary(x => x.Name, x => x.Values.AsEnumerable())) ??
               new Dictionary<string, IEnumerable<string>>();

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