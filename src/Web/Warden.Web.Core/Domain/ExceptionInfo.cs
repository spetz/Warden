using Warden.Web.Core.Extensions;

namespace Warden.Web.Core.Domain
{
    public class ExceptionInfo
    {
        public string Message { get; protected set; }
        public string Source { get; protected set; }
        public string StackTrace { get; protected set; }
        public ExceptionInfo InnerException { get; protected set; }

        protected ExceptionInfo()
        {
        }

        protected ExceptionInfo(string message, string source, string stackTrace,
            ExceptionInfo innerException = null)
        {
            if (message.Empty())
                throw new DomainException("Exception message can not be empty.");

            Message = message;
            Source = source;
            StackTrace = stackTrace;
            InnerException = innerException;
        }

        public static ExceptionInfo Create(string message, string source, string stackTrace,
            ExceptionInfo innerException = null)
            => new ExceptionInfo(message, source, stackTrace, innerException);
    }
}