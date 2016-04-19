using System;

namespace Warden.Web.Core.Domain
{
    public abstract class Entity : IIdentifiable
    {
        public Guid Id { get; protected set; }

        protected Entity()
        {
            Id = Guid.NewGuid();
        }
    }
}