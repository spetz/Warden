using System;

namespace Warden.Web.Core.Domain
{
    public class WardenInstance : Entity, ITimestampable
    {
        public Guid UserId { get; protected set; }
        public string Name { get; protected set; }
        public bool Enabled { get; protected set; }
        public DateTime CreatedAt { get; protected set; }
        public DateTime UpdatedAt { get; protected set; }
    }
}