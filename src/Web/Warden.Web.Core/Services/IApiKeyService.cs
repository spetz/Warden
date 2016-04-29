using System;
using System.Threading.Tasks;
using MongoDB.Driver;
using NLog;
using Warden.Web.Core.Domain;
using Warden.Web.Core.Mongo.Queries;
using Warden.Web.Core.Settings;

namespace Warden.Web.Core.Services
{
    public interface IApiKeyService
    {
        Task<ApiKey> GetAsync(string key);
        Task CreateAsync(Guid userId);
        Task CreateAsync(string email);
        Task DeleteAsync(string key);
    }

    public class ApiKeyService : IApiKeyService
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly IMongoDatabase _database;
        private readonly IEncrypter _encrypter;
        private readonly FeatureSettings _featureSettings;
        private readonly int RetryTimes = 5;

        public ApiKeyService(IMongoDatabase database, IEncrypter encrypter, FeatureSettings featureSettings)
        {
            _database = database;
            _encrypter = encrypter;
            _featureSettings = featureSettings;
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
            var apiKeysCount = await _database.ApiKeys().CountAsync(x => x.UserId == user.Id);
            if (apiKeysCount >= _featureSettings.MaxApiKeys)
            {
                throw new ServiceException($"Limit of {_featureSettings.MaxApiKeys} " +
                                           "API keys has been reached.");
            }

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

        public async Task DeleteAsync(string key)
        {
            var apiKey = await _database.ApiKeys().GetByKeyAsync(key);
            if (apiKey == null)
                throw new ServiceException($"API key has not been found for key: '{key}'.");

            await _database.ApiKeys().DeleteOneAsync(x => x.Id == apiKey.Id);
        }
    }
}