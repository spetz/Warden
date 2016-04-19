using System;
using Warden.Web.Core.Extensions;

namespace Warden.Web.Core.Domain
{
    public class ApiKey : ITimestampable
    {
        public string Key { get; protected set; }
        public DateTime CreatedAt { get; protected set; }

        protected ApiKey()
        {
            
        }
        protected ApiKey(string key)
        {
            if(key.Empty())
                throw new DomainException("API key can not be empty.");

            Key = key;
            CreatedAt = DateTime.UtcNow;
        }

        public static ApiKey Create(string key) => new ApiKey(key);
    }
}