using System;
using System.Collections.Generic;
using System.Linq;
using Warden.Web.Core.Services;

namespace Warden.Web.Core.Domain
{
    public class User : Entity, ITimestampable
    {
        private HashSet<ApiKey> _apiKeys = new HashSet<ApiKey>();

        public string Email { get; protected set; }
        public Role Role { get; protected set; }
        public byte[] Password { get; protected set; }
        public byte[] Salt { get; protected set; }
        public DateTime CreatedAt { get; protected set; }
        public DateTime UpdatedAt { get; protected set; }

        public IEnumerable<ApiKey> ApiKeys
        {
            get { return _apiKeys; }
            protected set { _apiKeys = new HashSet<ApiKey>(value); }
        }

        protected User()
        {
        }

        public User(string email, string password, IEncrypter encrypter, Role role = Role.User)
        {
            SetEmail(email);
            SetPassword(password, encrypter);
            Role = role;
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
        }

        public void AddApiKey(string key)
        {
            if (ApiKeys.Any(x => x.Key.EqualsCaseInvariant(key)))
                return;

            _apiKeys.Add(ApiKey.Create(key));
        }

        public void RemoveApiKey(string key)
        {
            var apiKey = ApiKeys.FirstOrDefault(x => x.Key.EqualsCaseInvariant(key));
            if (apiKey == null)
                return;

            _apiKeys.Remove(apiKey);
        }
    }
}