using System;

namespace Warden.Integrations.Twilio
{
    public class TwilioIntegrationConfiguration
    {
        /// <summary>
        /// Custom provider for the ITwilioService.
        /// </summary>
        public Func<ITwilioService> TwilioServiceProvider { get; protected set; }
    }
}