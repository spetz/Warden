using System;

namespace Warden.Integrations
{
    /// <summary>
    /// Custom class for the exceptions thrown by integrations.
    /// </summary>
    public class IntegrationException : Exception
    {
        public IntegrationException()
        {
        }

        public IntegrationException(string message) : base(message)
        {
        }

        public IntegrationException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}