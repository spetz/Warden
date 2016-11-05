using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Warden.Core;

namespace Warden.Integrations.SendGrid
{
    /// <summary>
    /// Custom extension methods for the SendGrid integration.
    /// </summary>
    public static class Extensions
    {
        private static readonly Regex EmailRegex =
            new Regex(
                @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
                @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$",
                RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant);

        /// <summary>
        /// Extension method for adding the SendGrid integration to the the WardenConfiguration.
        /// </summary>
        /// <param name="builder">Instance of the Warden configuration builder.</param>
        /// <param name="apiKey">API key of the SendGrid account.</param>
        /// <param name="sender">Email address of the message sender.</param>
        /// <param name="configurator">Optional lambda expression for configuring the SendGridIntegration.</param>
        /// <returns>Instance of fluent builder for the WardenConfiguration.</returns>
        public static WardenConfiguration.Builder IntegrateWithSendGrid(
            this WardenConfiguration.Builder builder,
            string apiKey, string sender,
            Action<SendGridIntegrationConfiguration.Builder> configurator = null)
        {
            builder.AddIntegration(SendGridIntegration.Create(apiKey, sender, configurator));

            return builder;
        }

        /// <summary>
        /// Extension method for adding the SendGrid integration to the the WardenConfiguration.
        /// </summary>
        /// <param name="builder">Instance of the Warden configuration builder.</param>
        /// <param name="configuration">Configuration of SendGridIntegration.</param>
        /// <returns>Instance of fluent builder for the WardenConfiguration.</returns>
        public static WardenConfiguration.Builder IntegrateWithSendGrid(
            this WardenConfiguration.Builder builder,
            SendGridIntegrationConfiguration configuration)
        {
            builder.AddIntegration(SendGridIntegration.Create(configuration));

            return builder;
        }

        /// <summary>
        /// Extension method for resolving the SendGrid integration from the IIntegrator.
        /// </summary>
        /// <param name="integrator">Instance of the IIntegrator.</param>
        /// <returns>Instance of fluent builder for the WardenConfiguration.</returns>
        public static SendGridIntegration SendGrid(this IIntegrator integrator)
            => integrator.Resolve<SendGridIntegration>();

        internal static string Or(this string value, string target)
            => string.IsNullOrWhiteSpace(value) ? target : value;

        internal static bool IsEmail(this string value) => EmailRegex.IsMatch(value);

        internal static IEnumerable<string> GetInvalidEmails(this IEnumerable<string> values)
            => values.Where(x => !EmailRegex.IsMatch(x));

        internal static void ValidateEmails(this IEnumerable<string> values, string parameterName)
        {
            if (values?.Any() == false)
                return;

            var invalidEmailReceivers = values.GetInvalidEmails();
            if (invalidEmailReceivers.Any())
            {
                throw new ArgumentException($"Invalid email(s): {string.Join(", ", invalidEmailReceivers)}.",
                    nameof(parameterName));
            }
        }
    }
}