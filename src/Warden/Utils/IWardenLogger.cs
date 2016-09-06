using System;

namespace Warden.Utils
{
    public interface IWardenLogger
    {
        void Trace(string message);
        void Info(string message);
        void Error(string message, Exception exception = null);
    }
}