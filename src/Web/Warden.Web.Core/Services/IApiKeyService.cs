using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using Warden.Web.Core.Domain;
using Warden.Web.Core.Mongo.Queries;

namespace Warden.Web.Core.Services
{
    public interface IApiKeyService
    {
        Task<ApiKey> GetAsync(string key);
        Task CreateAsync(Guid userId);
        Task CreateAsync(string email);
        Task<IEnumerable<string>> GetAllForOrganizationAsync(Guid organizationId);
        Task DeleteAsync(string key);
    }

    public class ApiKeyService : IApiKeyService
    {
        private readonly IMongoDatabase _database;
        private readonly IEncrypter _encrypter;
        private readonly int RetryTimes = 5;

        public ApiKeyService(IMongoDatabase database, IEncrypter encrypter)
        {
            _database = database;
            _encrypter = encrypter;
        }

        public async Task<ApiKey> GetAsync(string key)
        {
            var apiKey = await _database.ApiKeys().GetByKeyAsync(key);

            return apiKey;
        }

        public async Task CreateAsync(Guid userId)
        {
            var user = await _database.Users().GetByIdAsync(userId);
            if (user == null)
                throw new ServiceException($"User has not been found for id: '{userId}'.");

            await CreateAsync(user);
        }

        public async Task CreateAsync(string email)
        {
            var user = await _database.Users().GetByEmailAsync(email);
            if (user == null)
                throw new ServiceException($"User has not been found for email: '{email}'.");

            await CreateAsync(user);
        }
        private async Task CreateAsync(User user)
        {
            var isValid = false;
            var currentTry = 0;
            var key = string.Empty;
            while (currentTry < RetryTimes)
            {
                key = _encrypter.GetRandomSecureKey();
                isValid = await _database.ApiKeys().ExistsAsync(key) == false;
                if (isValid)
                    break;

                currentTry++;
            }

            if (!isValid)
                throw new ServiceException("Could not create an API key, please try again.");

            var apiKey = new ApiKey(key, user);
            await _database.ApiKeys().InsertOneAsync(apiKey);
        }

        public async Task<IEnumerable<string>> GetAllForOrganizationAsync(Guid organizationId)
        {
            var apiKey = await _database.ApiKeys().GetAllForUserAsync(organizationId);

            return apiKey.Select(x => x.Key);
        }

        public async Task DeleteAsync(string key)
        {
            var apiKey = await _database.ApiKeys().GetByKeyAsync(key);
            if (apiKey == null)
                throw new ServiceException($"API key has not been found for key: '{key}'.");

            await _database.ApiKeys().DeleteOneAsync(x => x.Id == apiKey.Id);
        }
    }
}