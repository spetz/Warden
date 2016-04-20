using System;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using Warden.Web.Core.Domain;
using Warden.Web.Core.Dto;
using Warden.Web.Core.Extensions;
using Warden.Web.Core.Mongo.Queries;

namespace Warden.Web.Core.Services
{
    public interface IOrganizationService
    {
        Task<OrganizationDto> GetAsync(Guid organizationId);
        Task<OrganizationDto> GetDefaultAsync(Guid ownerId);
        Task CreateDefaultAsync(Guid ownerId);
        Task CreateAsync(string name, Guid ownerId);
        Task AddWarden(Guid organizationId, string name, bool enabled = true);
        Task<bool> IsUserInOrganizationAsync(Guid organizationId, Guid userId);
    }

    public class OrganizationService : IOrganizationService
    {
        private readonly IMongoDatabase _database;
        private const string DefaultName = "My organization";

        public OrganizationService(IMongoDatabase database)
        {
            _database = database;
        }

        public async Task<OrganizationDto> GetAsync(Guid organizationId)
        {
            var organization = await _database.Organizations().GetByIdAsync(organizationId);
            if (organization == null)
                return null;

            var apiKeys = await _database.ApiKeys().GetAllForOrganizationAsync(organizationId);

            return new OrganizationDto(organization)
            {
                ApiKeys = apiKeys.Select(x => x.Key)
            };
        }

        public async Task<OrganizationDto> GetDefaultAsync(Guid ownerId)
        {
            var organization = await _database.Organizations().GetByNameForOwnerAsync(DefaultName, ownerId);

            return organization == null ? null : new OrganizationDto(organization);
        }

        public async Task CreateDefaultAsync(Guid ownerId)
        {
            await CreateAsync(DefaultName, ownerId);
        }

        public async Task CreateAsync(string name, Guid ownerId)
        {
            if (name.Empty())
                throw new ServiceException("Organization name can not be empty.");

            var owner = await _database.Users().GetByIdAsync(ownerId);
            if (owner == null)
                throw new ServiceException($"User has not been found for given id: '{ownerId}'.");

            var organization = await _database.Organizations().GetByNameForOwnerAsync(name, ownerId);
            if (organization != null)
                throw new ServiceException($"There's already an organization with name: '{name}' " +
                                           $"for owner with id: '{ownerId}'.");

            organization = new Organization(name, owner);
            await _database.Organizations().InsertOneAsync(organization);
        }

        public async Task AddWarden(Guid organizationId, string name, bool enabled = true)
        {
            var organization = await _database.Organizations().GetByIdAsync(organizationId);
            if (organization == null)
                throw new ServiceException($"Organization has not been found for given id: '{organizationId}'.");

            organization.AddWarden(name, enabled);
            await _database.Organizations().ReplaceOneAsync(x => x.Id == organization.Id, organization);
        }

        public async Task<bool> IsUserInOrganizationAsync(Guid organizationId, Guid userId)
        {
            var organization = await _database.Organizations().GetByIdAsync(organizationId);

            return organization != null && organization.Users.Any(x => x.Id == userId);
        }
    }
}