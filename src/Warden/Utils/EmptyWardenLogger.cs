using System;

namespace Warden.Utils
{
    public class EmptyWardenLogger : IWardenLogger
    {
        public void Trace(string message)
        {
        }

        public void Info(string message)
        {
        }

        public void Error(string message, Exception exception = null)
        {
        }
    }
}