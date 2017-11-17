using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using MailKit.Net.Smtp;
using MimeKit;
using MailKit.Security;


namespace Warden.Integrations.Smtp
{
    public interface ISmtpService
    {
        Task SendEmailAsync(
            string to,
            string from,
            string subject,
            string body,
            IEnumerable<string> cc = null,
            bool isBodyHtml = false,

            string username = null,
            string password = null,
            TimeSpan? timeout = null);
    }


    public class SmtpService : ISmtpService
    {
        private readonly string _host;
        private readonly int _port;
        private readonly bool _enableSsl;

        public SmtpService(string host, int port, bool enableSsl)
        {
            _host = host;
            _port = port;
            _enableSsl = enableSsl;
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
            try
            {
                using (var _client = new SmtpClient())
                {
                    await _client.ConnectAsync(_host
                        , _port
                        , _enableSsl ? SecureSocketOptions.StartTlsWhenAvailable : SecureSocketOptions.None);

                    _client.AuthenticationMechanisms.Remove("XOAUTH2");


                    if (!string.IsNullOrWhiteSpace(username) &&
                        !string.IsNullOrWhiteSpace(password))
                    {
                        await _client.AuthenticateAsync(username, password);
                    }

                    if (timeout.HasValue)
                    {
                        _client.Timeout = (int)timeout.Value.TotalMilliseconds;
                    }

                    var message = PrepareMessage(from, to, subject, body, cc, isBodyHtml);

                    await _client.SendAsync(message);

                    await _client.DisconnectAsync(true);
                }
            }
            catch (Exception exception)
            {
                throw new IntegrationException("There was an error while executing the SendEmailAsync(): " +
                                               $"{exception}", exception);
            }
        }

        private MimeMessage PrepareMessage(string from, string to, string subject, string body,
            IEnumerable<string> cc, bool isBodyHtml)
        {
            var message = new MimeMessage();

            message.From.Add(new MailboxAddress(from));

            message.To.Add(new MailboxAddress(to));

            message.Subject = subject;

            message.Body = new TextPart(isBodyHtml ? "html" : "plain")
            {
                Text = body
            };

            if (cc != null && cc.Any())
            {
                foreach (var address in cc)
                {
                    message.Cc.Add(new MailboxAddress(address));
                }
            }

            return message;
        }
    }
}