using System;
using System.Threading.Tasks;
using MongoDB.Driver;
using System.Linq;
using Warden.Web.Core.Domain;
using Warden.Web.Core.Dto;
using Warden.Web.Core.Extensions;
using Warden.Web.Core.Mongo.Queries;

namespace Warden.Web.Core.Services
{
    public interface IUserService
    {
        Task<UserDto> GetAsync(string email);
        Task<UserDto> GetAsync(Guid id);
        Task RegisterAsync(string email, string password, Role role = Role.User);
        Task LoginAsync(string email, string password);
        Task SetRecentlyViewedWardenInOrganizationAsync(Guid userId, Guid organizationId, Guid wardenId);
        Task SetNewPasswordAsync(Guid userId, string actualPassword, string newPassword);
    }

    public class UserService : IUserService
    {
        private readonly IMongoDatabase _database;
        private readonly IEncrypter _encrypter;

        public UserService(IMongoDatabase database, IEncrypter encrypter)
        {
            _database = database;
            _encrypter = encrypter;
        }

        public async Task<UserDto> GetAsync(string email)
        {
            var user = await _database.Users().GetByEmailAsync(email);

            return await GetUserWithApiKeysAsync(user);
        }

        public async Task<UserDto> GetAsync(Guid id)
        {
            var user = await _database.Users().GetByIdAsync(id);

            return await GetUserWithApiKeysAsync(user);
        }

        private async Task<UserDto> GetUserWithApiKeysAsync(User user)
        {
            if (user == null)
                return null;

            var apiKeys = await _database.ApiKeys().GetAllForUserAsync(user.Id);

            return new UserDto(user)
            {
                ApiKeys = apiKeys.Select(x => x.Key)
            };
        }

        public async Task RegisterAsync(string email, string password, Role role = Role.User)
        {
            if(!email.IsEmail())
                throw new ServiceException($"Invalid email: '{email}.");
            if (password.Empty())
                throw new ServiceException("Password can not be empty.");

            var userExists = await _database.Users().ExistsAsync(email);
            if(userExists)
                throw new ServiceException($"User with email: '{email}' is already registered.");

            var user = new User(email, password, _encrypter, role);
            await _database.Users().InsertOneAsync(user);
        }

        public async Task LoginAsync(string email, string password)
        {
            if (!email.IsEmail())
                throw new ServiceException($"Invalid email: '{email}.");
            if (password.Empty())
                throw new ServiceException("Password can not be empty.");

            var user = await _database.Users().GetByEmailAsync(email);
            if (user == null)
                throw new ServiceException($"User has not been found for email: '{email}'.");

            if(!user.ValidatePassword(password, _encrypter))
                throw new ServiceException("Invalid password.");
        }

        public async Task SetRecentlyViewedWardenInOrganizationAsync(Guid userId, Guid organizationId, Guid wardenId)
        {
            var user = await _database.Users().GetByIdAsync(userId);
            var organization = await _database.Organizations().GetByIdAsync(organizationId);
            user.SetRecentlyViewedWardenInOrganization(organization, wardenId);
            await _database.Users().ReplaceOneAsync(x => x.Id == userId, user);
        }

        public async Task SetNewPasswordAsync(Guid userId, string actualPassword, string newPassword)
        {
            var user = await _database.Users().GetByIdAsync(userId);
            if (user == null)
                throw new ServiceException($"User has not been found for id: '{userId}'.");

            if (!user.ValidatePassword(actualPassword, _encrypter))
                throw new ServiceException("Invalid actual password.");

            user.SetPassword(newPassword, _encrypter);
            await _database.Users().ReplaceOneAsync(x => x.Id == userId, user);
        }
    }
}