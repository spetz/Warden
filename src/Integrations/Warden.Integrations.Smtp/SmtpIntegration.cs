using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Warden.Integrations.Smtp
{
    public class SmtpIntegration : IIntegration
    {
        private readonly SmtpIntegrationConfiguration _configuration;
        private readonly ISmtpService _smtpService;

        public SmtpIntegration(SmtpIntegrationConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration),
                    "Smtp Integration configuration has not been provided.");
            }

            _configuration = configuration;
            _smtpService = _configuration.SmtpServiceProvider();
        }

        public async Task SendEmailAsync(string body)
        {
            await _smtpService.SendEmailAsync(
                _configuration.DefaultToAddress,
                _configuration.DefaultFromAddress,
                _configuration.DefaultSubject,
                body,
                _configuration.DefaultCCAddresses,
                _configuration.DefaultIsBodyHtml,
                _configuration.Username,
                _configuration.Password,
                _configuration.Timeout);
        }

        public async Task SendEmailAsync(string subject, string body)
        {
            await _smtpService.SendEmailAsync(
                _configuration.DefaultToAddress,
                _configuration.DefaultFromAddress,
                subject,
                body,
                _configuration.DefaultCCAddresses,
                _configuration.DefaultIsBodyHtml,
                _configuration.Username,
                _configuration.Password,
                _configuration.Timeout);
        }

        public async Task SendEmailAsync(string to, string from, string subject, string body)
        {
            await _smtpService.SendEmailAsync(
                to,
                from,
                subject,
                body,
                _configuration.DefaultCCAddresses,
                _configuration.DefaultIsBodyHtml,
                _configuration.Username,
                _configuration.Password,
                _configuration.Timeout);
        }

        public async Task SendEmailAsync(string to, string from, string subject, string body,
            string username, string password)
        {
            await _smtpService.SendEmailAsync(
                to,
                from,
                subject,
                body,
                _configuration.DefaultCCAddresses,
                _configuration.DefaultIsBodyHtml,
                username,
                password,
                _configuration.Timeout);
        }

        public async Task SendEmailAsync(
            string to,
            string from,
            string subject,
            string body,
            IEnumerable<string> cc = null,
            bool isBodyHtml = false,
            string username = null,
            string password = null,
            TimeSpan? timeout = null)
        {
            await _smtpService.SendEmailAsync(to, from, subject, body, cc, isBodyHtml,
                username, password, timeout);
        }

        public static SmtpIntegration Create(string host,
            int port,
            bool enableSsl,
            Action<SmtpIntegrationConfiguration.Builder> configurator = null)
        {
            var config = new SmtpIntegrationConfiguration.Builder(host, port, enableSsl);
            configurator?.Invoke(config);

            return Create(config.Build());
        }

        public static SmtpIntegration Create(SmtpIntegrationConfiguration configuration)
            => new SmtpIntegration(configuration);
    }
}