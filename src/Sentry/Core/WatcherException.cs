using System;

namespace Sentry.Core
{
    /// <summary>
    /// Custom class for the exceptions thrown by watchers.
    /// </summary>
    public class WatcherException : Exception
    {
        public WatcherException()
        {
        }

        public WatcherException(string message) : base(message)
        {
        }

        public WatcherException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}