using System;
using System.Threading.Tasks;
using MongoDB.Driver;
using Warden.Web.Core.Domain;
using Warden.Web.Core.Extensions;
using Warden.Web.Core.Mongo.Queries;

namespace Warden.Web.Core.Services
{
    public interface IOrganizationService
    {
        Task CreateAsync(string name, Guid ownerId);
    }

    public class OrganizationService : IOrganizationService
    {
        private readonly IMongoDatabase _database;

        public OrganizationService(IMongoDatabase database)
        {
            _database = database;
        }

        public async Task CreateAsync(string name, Guid ownerId)
        {
            if(name.Empty())
                throw new ServiceException("Organization name can not be empty.");

            var organization = await _database.Organizations().GetByNameAsync(name);
            if(organization != null)
                throw new ServiceException($"There's already an organization with name: '{name}.");

            var owner = await _database.Users().GetByIdAsync(ownerId);
            if(owner == null)
                throw new ServiceException($"User has not been found for given id: '{ownerId}'.");

            organization = new Organization(name, owner);
            await _database.Organizations().InsertOneAsync(organization);
        }
    }
}