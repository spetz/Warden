using System;

namespace Sentry.Core
{
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