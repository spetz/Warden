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
        Task CreateAsync(Guid organizationId);
        Task<IEnumerable<string>> GetAllForOrganizationAsync(Guid organizationId);
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

        public async Task CreateAsync(Guid organizationId)
        {
            var organization = await _database.Organizations().GetByIdAsync(organizationId);
            if (organization == null)
                throw new ServiceException("Can not create an API key without organization.");

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

            if(!isValid)
                throw new ServiceException("Could not create an API key, please try again.");

            var apiKey = new ApiKey(key, organization);
            await _database.ApiKeys().InsertOneAsync(apiKey);
        }

        public async Task<IEnumerable<string>> GetAllForOrganizationAsync(Guid organizationId)
        {
            var apiKey = await _database.ApiKeys().GetAllForOrganizationAsync(organizationId);

            return apiKey.Select(x => x.Key);
        }
    }
}