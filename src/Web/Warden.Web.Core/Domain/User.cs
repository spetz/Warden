using System;
using System.Linq;
using Warden.Web.Core.Extensions;
using Warden.Web.Core.Services;

namespace Warden.Web.Core.Domain
{
    public class User : Entity, ITimestampable
    {
        public string Email { get; protected set; }
        public Role Role { get; protected set; }
        public State State { get; protected set; }
        public string Password { get; protected set; }
        public string Salt { get; protected set; }
        public DateTime CreatedAt { get; protected set; }
        public DateTime UpdatedAt { get; protected set; }
        public Guid RecentlyViewedOrganizationId { get; protected set; }
        public Guid RecentlyViewedWardenId { get; protected set; }

        protected User()
        {
        }

        public User(string email, string password, IEncrypter encrypter, Role role = Role.User)
        {
            SetEmail(email);
            SetPassword(password, encrypter);
            Role = role;
            State = State.Active;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetEmail(string email)
        {
            if (email.Empty())
                throw new DomainException("Email can not be empty.");

            if (Email.EqualsCaseInvariant(email))
                return;

            Email = email.ToLowerInvariant();
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetRole(Role role)
        {
            if (Role == role)
                return;

            Role = role;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetPassword(string password, IEncrypter encrypter)
        {
            if (password.Empty())
                throw new DomainException("Password can not be empty.");

            var salt = encrypter.GetSalt(password);
            var hash = encrypter.GetHash(password, salt);

            Password = hash;
            Salt = salt;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Lock()
        {
            if(State == State.Locked)
                return;

            State = State.Locked;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Activate()
        {
            if (State == State.Active)
                return;

            State = State.Active;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Delete()
        {
            if (State == State.Deleted)
                return;

            State = State.Deleted;
            UpdatedAt = DateTime.UtcNow;
        }

        public bool ValidatePassword(string password, IEncrypter encrypter)
        {
            var hashedPassword = encrypter.GetHash(password, Salt);

            return Password.Equals(hashedPassword);
        }

        public void SetRecentlyViewedWardenInOrganization(Organization organization, Guid wardenId)
        {
            var organizationId = organization?.Id ?? Guid.Empty;
            var foundWardenId = organization?.Wardens.Any(x => x.Id == wardenId) == true ? wardenId : Guid.Empty;
            if (RecentlyViewedOrganizationId == organizationId && RecentlyViewedWardenId == foundWardenId)
                return;

            RecentlyViewedOrganizationId = organizationId;
            RecentlyViewedWardenId = foundWardenId;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}