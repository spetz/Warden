using System;

namespace Warden.Core
{
    /// <summary>
    /// Custom class for the exceptions thrown by Warden.
    /// </summary>
    public class WardenException : Exception
    {
        public WardenException()
        {
        }

        public WardenException(string message) : base(message)
        {
        }

        public WardenException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}