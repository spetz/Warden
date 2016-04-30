using System;

namespace Warden.Web.Core.Domain
{
    public class UserSession : Entity, ITimestampable
    {
        public Guid UserId { get; protected set; }
        public string UserAgent { get; protected set; }
        public string IpAddress { get; protected set; }
        public string Referrer { get; protected set; }
        public DateTime CreatedAt { get; protected set; }

        protected UserSession()
        {
        }

        public UserSession(User user, string userAgent = null,
            string ipAddress = null, string referrer = null)
        {
            if (user == null)
                throw new DomainException("Can not create an empty user session.");

            UserId = user.Id;
            UserAgent = userAgent;
            IpAddress = ipAddress;
            Referrer = referrer;
            CreatedAt = DateTime.UtcNow;
        }
    }
}