using System;

namespace Sentry.Core
{
    /// <summary>
    /// Custom class for the exceptions thrown by Sentry.
    /// </summary>
    public class SentryException : Exception
    {
        public SentryException()
        {
        }

        public SentryException(string message) : base(message)
        {
        }

        public SentryException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}