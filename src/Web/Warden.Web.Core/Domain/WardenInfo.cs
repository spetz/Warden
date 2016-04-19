using System;

namespace Warden.Web.Core.Domain
{
    public class WardenInfo
    {
        public Guid Id { get; protected set; }
        public Guid OrganizationId { get; protected set; }
        public string Name { get; protected set; }

        protected WardenInfo()
        {
        }

        protected WardenInfo(string name, Organization organization)
        {
            var warden = organization.GetWardenByNameOrFail(name);
            if(warden == null)
                throw new DomainException("Warden can not be null.");
            if (organization == null)
                throw new DomainException("Organization can not be null.");

            Id = warden.Id;
            OrganizationId = organization.Id;
            Name = warden.Name;
        }

        public static WardenInfo Create(string name, Organization organization) 
            => new WardenInfo(name, organization);
    }
}