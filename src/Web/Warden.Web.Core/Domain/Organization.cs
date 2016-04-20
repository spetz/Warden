using System;
using System.Linq;
using System.Collections.Generic;
using Warden.Web.Core.Extensions;

namespace Warden.Web.Core.Domain
{
    public class Organization : Entity, ITimestampable
    {
        private HashSet<UserInOrganization> _users = new HashSet<UserInOrganization>();
        private HashSet<Warden> _wardens = new HashSet<Warden>();

        public string Name { get; protected set; }
        public Guid OwnerId { get; protected set; }
        public DateTime CreatedAt { get; protected set; }
        public DateTime UpdatedAt { get; protected set; }

        public IEnumerable<UserInOrganization> Users
        {
            get { return _users; }
            protected set { _users = new HashSet<UserInOrganization>(value); }
        }

        public IEnumerable<Warden> Wardens
        {
            get { return _wardens; }
            protected set { _wardens = new HashSet<Warden>(value); }
        }

        protected Organization()
        {
        }

        public Organization(string name, User owner)
        {
            SetName(name);
            SetOwner(owner);
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetName(string name)
        {
            if (name.Empty())
                throw new DomainException("Organization name can not be empty.");

            Name = name;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetOwner(User owner)
        {
            if (owner == null)
                throw new DomainException("Organization owner can not be null.");

            OwnerId = owner.Id;
            AddUser(owner, OrganizationRole.Owner);
            UpdatedAt = DateTime.UtcNow;
        }

        public void AddUser(User user, OrganizationRole role = OrganizationRole.User)
        {
            if (user == null)
                throw new DomainException("Can not add empty user to the organization.");

            if (Users.Any(x => x.Id == user.Id))
                return;

            _users.Add(UserInOrganization.Create(user, role));
            UpdatedAt = DateTime.UtcNow;
        }

        public void RemoveUser(User user)
        {
            if (user == null)
                throw new DomainException("Can not remove empty user from the organization.");

            var userInOrganization = Users.FirstOrDefault(x => x.Id == user.Id);
            if (userInOrganization == null)
                return;

            _users.Remove(userInOrganization);
            UpdatedAt = DateTime.UtcNow;
        }

        public void AddWarden(string name, bool enabled = true)
        {
            if (name.Empty())
                throw new DomainException("Can not add a warden without a name to the organization.");

            var warden = GetWardenByName(name);
            if (warden != null)
                throw new DomainException($"Warden with name: '{name}' has been already added.");

            _wardens.Add(new Warden(name, enabled));
            UpdatedAt = DateTime.UtcNow;
        }

        public void RemoveWarden(string name)
        {
            if (name.Empty())
                throw new DomainException("Can not remove a warden without a name from the organization.");

            var warden = GetWardenByNameOrFail(name);
            _wardens.Remove(warden);
            UpdatedAt = DateTime.UtcNow;
        }

        public void EnableWarden(string name)
        {
            var warden = GetWardenByNameOrFail(name);
            warden.Enable();
            UpdatedAt = DateTime.UtcNow;
        }

        public void DisableWarden(string name)
        {
            var warden = GetWardenByNameOrFail(name);
            warden.Disable();
            UpdatedAt = DateTime.UtcNow;
        }

        public Warden GetWardenByNameOrFail(string name)
        {
            if (name.Empty())
                throw new DomainException("Warden name can not be empty.");

            var warden = GetWardenByName(name);
            if (warden == null)
                throw new DomainException($"Warden with name: '{name}' has not been found.");

            return warden;
        }

        public Warden GetWardenByName(string name) => Wardens.FirstOrDefault(x => x.Name.EqualsCaseInvariant(name));
    }
}