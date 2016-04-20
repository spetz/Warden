using System;
using Warden.Web.Core.Extensions;

namespace Warden.Web.Core.Domain
{
    public class ApiKey : Entity, ITimestampable
    {
        public string Key { get; protected set; }
        public Guid OrganizationId { get; protected set; }
        public DateTime CreatedAt { get; protected set; }

        protected ApiKey()
        {
        }

        public ApiKey(string key, Organization organization)
        {
            if (key.Empty())
                throw new DomainException("API key can not be empty.");
            if (organization == null)
                throw new DomainException("Can not create an API key without organization.");

            Key = key;
            OrganizationId = organization.Id;
            CreatedAt = DateTime.UtcNow;
        }
    }
}