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

        public async Task SendMessageAsync(string message)
        {
            await _slackService.PostMessageAsync(message);
        }
    }
}