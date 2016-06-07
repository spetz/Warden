using System;

namespace Warden.Integrations.Slack
{
    /// <summary>
    /// Configuration of the SlackIntegration.
    /// </summary>
    public class SlackIntegrationConfiguration
    {
        /// <summary>
        /// Full URL of the webhook integration.
        /// </summary>
        public string WebhookUrl { get; protected set; }

        /// <summary>
        /// Custom provider for the ISlackService.
        /// </summary>
        public Func<ISlackService> SlackServiceProvider { get; protected set; }

        public SlackIntegrationConfiguration(string webhookUrl)
        {
            SlackServiceProvider = () => new SlackService(webhookUrl);
        }
    }
}