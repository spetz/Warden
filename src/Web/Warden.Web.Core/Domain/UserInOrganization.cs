using System;

namespace Warden.Web.Core.Domain
{
    public class UserInOrganization : ITimestampable
    {
        public Guid Id { get; protected set; }
        public string Email { get; protected set; }
        public OrganizationRole Role { get; protected set; }
        public DateTime CreatedAt { get; protected set; }

        protected UserInOrganization()
        {
        }

        protected UserInOrganization(User user, OrganizationRole role)
        {
            if (user == null)
                throw new DomainException("Can not create new user in organization from empty user.");

            Id = user.Id;
            Email = user.Email;
            Role = role;
            CreatedAt = DateTime.UtcNow;
        }

        public static UserInOrganization Create(User user, OrganizationRole role = OrganizationRole.User)
            => new UserInOrganization(user, role);
    }
}