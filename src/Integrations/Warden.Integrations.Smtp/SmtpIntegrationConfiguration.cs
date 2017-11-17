using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Warden.Integrations.Smtp
{
    public class SmtpIntegrationConfiguration
    {
        //All Parameters

        public string Host { get; protected set; }

        public int Port { get; protected set; }

        public bool EnableSsl { get; protected set; }

        public string Username { get; protected set; }

        public string Password { get; protected set; }

        public TimeSpan? Timeout { get; protected set; }

        public string DefaultToAddress { get; protected set; }

        public string DefaultFromAddress { get; protected set; }

        public string DefaultSubject { get; protected set; }

        public string DefaultBody { get; protected set; }

        public bool DefaultIsBodyHtml { get; protected set; }

        public IEnumerable<string> DefaultCCAddresses { get; protected set; }

        public Func<ISmtpService> SmtpServiceProvider { get; protected set; }

        public static Builder Create(string host, int port, bool enableSsl) => new Builder(host, port, enableSsl);

        protected SmtpIntegrationConfiguration(string host, int port, bool enableSsl)
        {
            Host = host;
            Port = port;
            EnableSsl = enableSsl;

            SmtpServiceProvider = () => new SmtpService(Host, Port, EnableSsl);
        }

        public class Builder
        {
            protected readonly SmtpIntegrationConfiguration Configuration;

            public Builder(string host, int port, bool enableSsl)
            {
                Configuration = new SmtpIntegrationConfiguration(host, port, enableSsl);
            }

            public Builder WithDefaultCredentials()
            {
                Configuration.Username = null;
                Configuration.Password = null;
                return this;
            }

            public Builder WithCredentials(string username, string password)
            {
                if (string.IsNullOrWhiteSpace(username))
                    throw new ArgumentException("Username can not be empty.", nameof(username));

                if (string.IsNullOrWhiteSpace(password))
                    throw new ArgumentException("Password can not be empty.", nameof(password));

                Configuration.Username = username;
                Configuration.Password = password;
                return this;
            }

            public Builder WithDefaultTimeout(TimeSpan timeout)
            {
                if (timeout == null)
                    throw new ArgumentNullException(nameof(timeout), "Timeout can not be null.");

                if (timeout == TimeSpan.Zero)
                    throw new ArgumentException("Timeout can not be equal to zero.", nameof(timeout));

                Configuration.Timeout = timeout;

                return this;
            }

            public Builder WithDefaultToAddress(string toAddress)
            {
                if (string.IsNullOrWhiteSpace(toAddress))
                    throw new ArgumentException("To Address can not be empty.", nameof(toAddress));

                Configuration.DefaultToAddress = toAddress;

                return this;
            }

            public Builder WithDefaultFromAddress(string fromAddress)
            {
                if (string.IsNullOrWhiteSpace(fromAddress))
                    throw new ArgumentException("From Address can not be empty.", nameof(fromAddress));

                Configuration.DefaultFromAddress = fromAddress;

                return this;
            }

            public Builder WithDefaultSubject(string subject)
            {
                if (string.IsNullOrWhiteSpace(subject))
                    throw new ArgumentException("Subject  can not be empty.", nameof(subject));

                Configuration.DefaultSubject = subject;

                return this;
            }

            public Builder WithDefaultBody(string body)
            {
                if (string.IsNullOrWhiteSpace(body))
                    throw new ArgumentException("Body can not be empty.", nameof(body));

                Configuration.DefaultBody = body;

                return this;
            }

            public Builder WithDefaultIsBodyHtml(bool isBodyHtml)
            {
                Configuration.DefaultIsBodyHtml = isBodyHtml;

                return this;
            }

            public Builder WithDefaultCCAddress(IEnumerable<string> ccAddresses)
            {
                if (ccAddresses == null)
                    throw new ArgumentNullException(nameof(ccAddresses), "CC Addresses can not be null.");

                if (!ccAddresses.Any())
                    throw new ArgumentException("CC Addresses must contain at least one address.", nameof(ccAddresses));


                List<string> defaultAddresses = new List<string>();

                foreach (var address in ccAddresses)
                {
                    if (!string.IsNullOrWhiteSpace(address))
                    {
                        defaultAddresses.Add(address);
                    }
                }

                if (!defaultAddresses.Any())
                    throw new ArgumentException("CC Addresses can not be empty/blank.", nameof(ccAddresses));

                Configuration.DefaultCCAddresses = defaultAddresses;

                return this;
            }

            public Builder WithSmtpServiceProvider(Func<ISmtpService> smtpServiceProvider)
            {
                if (smtpServiceProvider == null)
                {
                    throw new ArgumentNullException(nameof(smtpServiceProvider),
                        "Smtp service provider can not be null.");
                }

                Configuration.SmtpServiceProvider = smtpServiceProvider;

                return this;
            }

            public SmtpIntegrationConfiguration Build() => Configuration;
        }
    }
}