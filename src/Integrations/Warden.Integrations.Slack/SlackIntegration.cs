using System;
using System.Threading.Tasks;

namespace Warden.Integrations.Slack
{
    /// <summary>
    /// Integration with the Slack.
    /// </summary>
    public class SlackIntegration : IIntegration
    {
        private readonly SlackIntegrationConfiguration _configuration;
        private readonly ISlackService _slackService;

        public SlackIntegration(SlackIntegrationConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration),
                    "Slack Integration configuration has not been provided.");
            }

            _configuration = configuration;
            _slackService = _configuration.SlackServiceProvider();
        }

        /// <summary>
        /// Sends a default message to the default channel, using a default username.
        /// </summary>
        /// <returns></returns>
        public async Task SendMessageAsync()
        {
            await SendMessageAsync(_configuration.DefaultMessage, _configuration.DefaultChannel, _configuration.DefaultUsername);
        }

        /// <summary>
        /// Sends a message to the default channel, using a default username.
        /// </summary>
        /// <param name="message">Message text. If default message has been set, it will override its value.</param>
        /// <returns></returns>
        public async Task SendMessageAsync(string message)
        {
            await SendMessageAsync(message, _configuration.DefaultChannel, _configuration.DefaultUsername);
        }

        /// <summary>
        /// Sends a message to the selected channel, using a default username.
        /// </summary>
        /// <param name="message">Message text. If default message has been set, it will override its value.</param>
        /// <param name="channel">Channel name. If default channel has been set, it will override its value.</param>
        /// <returns></returns>
        public async Task SendMessageAsync(string message, string channel)
        {
            await SendMessageAsync(message, channel, _configuration.DefaultUsername);
        }

        /// <summary>
        /// Sends a message to the selected channel, using a given username.
        /// </summary>
        /// <param name="message">Message text. If default message has been set, it will override its value.</param>
        /// <param name="channel">Channel name. If default channel has been set, it will override its value.</param>
        /// <param name="username">Custom username. If default username has been set, it will override its value.</param>
        /// <returns></returns>
        public async Task SendMessageAsync(string message, string channel, string username)
        {
            await _slackService.SendMessageAsync(message, channel, username);
        }

        /// <summary>
        /// Factory method for creating a new instance of SlackIntegration.
        /// </summary>
        /// <param name="webhookUrl">Full URL of the Slack webhook integration.</param>
        /// <param name="configurator">Lambda expression for configuring the SlackIntegration integration.</param>
        /// <returns>Instance of SlackIntegration.</returns>
        public static SlackIntegration Create(string webhookUrl,
            Action<SlackIntegrationConfiguration.Builder> configurator = null)
        {
            var config = new SlackIntegrationConfiguration.Builder(webhookUrl);
            configurator?.Invoke(config);

            return Create(config.Build());
        }

        /// <summary>
        /// Factory method for creating a new instance of SlackIntegration.
        /// </summary>
        /// <param name="configuration">Configuration of Slack integration.</param>
        /// <returns>Instance of SlackIntegration.</returns>
        public static SlackIntegration Create(SlackIntegrationConfiguration configuration)
            => new SlackIntegration(configuration);
    }
}