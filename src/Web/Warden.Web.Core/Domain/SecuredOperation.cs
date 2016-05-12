using System;
using Warden.Web.Core.Extensions;

namespace Warden.Web.Core.Domain
{
    public class SecuredOperation : Entity, ITimestampable
    {
        public Guid? UserId { get; protected set; }
        public string Email { get; protected set; }
        public SecuredOperationType Type { get; protected set; }
        public string Token { get; protected set; }
        public string RequesterIpAddress { get; protected set; }
        public string RequesterUserAgent { get; protected set; }
        public string ConsumerIpAddress { get; protected set; }
        public string ConsumerUserAgent { get; protected set; }
        public bool Consumed { get; set; }
        public DateTime CreatedAt { get; protected set; }
        public DateTime UpdatedAt { get; protected set; }
        public DateTime Expiry { get; protected set; }

        public SecuredOperation(SecuredOperationType type, string token,
            DateTime expiry, Guid? userId = null, string email = null,
            string ipAddress = null, string userAgent = null)
        {
            if (!userId.HasValue)
            {
                if (email.Empty())
                    throw new DomainException("Both user id and email can not be empty.");

                if (email.IsEmail())
                    throw new DomainException($"Invalid email: '{email}.");
            }

            if (token.Empty())
                throw new DomainException("Token can not be empty.");

            UserId = userId;
            Email = email?.ToLowerInvariant();
            Type = type;
            Token = token;
            Expiry = expiry.ToUniversalTime();
            RequesterIpAddress = ipAddress;
            RequesterUserAgent = userAgent;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public void CheckIfCanBeConsumedOrFail()
        {
            if (Consumed)
                throw new DomainException("Token has been already consumed.");

            if (Expiry <= DateTime.UtcNow)
                throw new DomainException($"Token has expired at: {Expiry}.");
        }

        public void Consume(string ipAddress = null, string userAgent = null)
        {
            CheckIfCanBeConsumedOrFail();
            Consumed = true;
            ConsumerIpAddress = ipAddress;
            ConsumerUserAgent = userAgent;
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public enum SecuredOperationType
    {
        NotSet = 0,
        ResetPassword = 1
    }
}