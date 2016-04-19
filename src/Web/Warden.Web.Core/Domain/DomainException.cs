using System;

namespace Warden.Web.Core.Domain
{
    public class DomainException : Exception
    {
        public DomainException()
        {
        }

        public DomainException(string message, params object[] args) : this(null, message, args)
        {
        }

        public DomainException(Exception innerException, string message, params object[] args)
            : base(string.Format(message, args), innerException)
        {
        }
    }
}