using System;
using Warden.Web.Core.Extensions;

namespace Warden.Web.Core.Domain
{
    public class Warden : Entity, ITimestampable
    {
        public string Name { get; protected set; }
        public bool Enabled { get; protected set; }
        public DateTime CreatedAt { get; protected set; }
        public DateTime UpdatedAt { get; protected set; }

        protected Warden()
        {
        }

        public Warden(string name, bool enabled = true)
        {
            SetName(name);
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
            if (enabled)
                Enable();
        }

        public void SetName(string name)
        {
            if (name.Empty())
                throw new DomainException("Warden name can not be empty.");

            Name = name;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Enable()
        {
            if (Enabled)
                return;

            Enabled = true;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Disable()
        {
            if (!Enabled)
                return;

            Enabled = false;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}