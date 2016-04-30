using System;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using NLog;
using Warden.Web.Core.Domain;
using Warden.Web.Core.Dto;
using Warden.Web.Core.Extensions;
using Warden.Web.Core.Mongo;
using Warden.Web.Core.Mongo.Queries;
using Warden.Web.Core.Queries;
using Warden.Web.Core.Settings;

namespace Warden.Web.Core.Services
{
    public interface IOrganizationService
    {
        Task<OrganizationDto> GetAsync(Guid organizationId);
        Task<OrganizationDto> GetAsync(string name, Guid ownerId);
        Task<OrganizationDto> GetDefaultAsync(Guid ownerId);
        Task CreateDefaultAsync(Guid ownerId);
        Task CreateAsync(string name, Guid ownerId, bool autoRegisterNewWarden = true);
        Task AddWardenAsync(Guid organizationId, string name, bool enabled = true);
        Task AddUserAsync(Guid organizationId, string email, OrganizationRole role = OrganizationRole.User);
        Task EnableWardenAsync(Guid organizationId, string name);
        Task DisableWardenAsync(Guid organizationId, string name);
        Task<bool> IsUserInOrganizationAsync(Guid organizationId, Guid userId);
        Task<PagedResult<OrganizationDto>> BrowseAsync(BrowseOrganizations query);
        Task DeleteAsync(Guid organizationId, bool removeAllIterations = true);
        Task DeleteUserAsync(Guid organizationId, Guid userId);
        Task DeleteWardenAsync(Guid organizationId, Guid wardenId);
        Task EnableAutoRegisterNewWardenAsync(Guid organizationId);
        Task DisableAutoRegisterNewWardenAsync(Guid organizationId);
    }

    public class OrganizationService : IOrganizationService
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly IMongoDatabase _database;
        private readonly FeatureSettings _featureSettings;
        private const string DefaultName = "My organization";

        public OrganizationService(IMongoDatabase database, FeatureSettings featureSettings)
        {
            _database = database;
            _featureSettings = featureSettings;
        }

        public async Task<OrganizationDto> GetAsync(Guid organizationId)
        {
            var organization = await _database.Organizations().GetByIdAsync(organizationId);

            return GetOrganizationDto(organization);
        }

        public async Task<OrganizationDto> GetDefaultAsync(Guid ownerId)
            => await GetAsync(DefaultName, ownerId);


        public async Task<OrganizationDto> GetAsync(string name, Guid ownerId)
        {
            var organization = await _database.Organizations().GetByNameForOwnerAsync(name, ownerId);

            return GetOrganizationDto(organization);
        }

        private OrganizationDto GetOrganizationDto(Organization organization)
        {
            return organization == null ? null : new OrganizationDto(organization);
        }

        public async Task<PagedResult<OrganizationDto>> BrowseAsync(BrowseOrganizations query)
        {
            if (query == null)
                return PagedResult<OrganizationDto>.Empty;

            var organizations = await _database.Organizations()
                .Query(query)
                .OrderBy(x => x.Name)
                .PaginateAsync(query);

            return PagedResult<OrganizationDto>.From(organizations,
                organizations.Items.Select(x => new OrganizationDto(x)));
        }

        public async Task CreateDefaultAsync(Guid ownerId)
        {
            await CreateAsync(DefaultName, ownerId);
        }

        public async Task CreateAsync(string name, Guid ownerId, bool autoRegisterNewWarden = true)
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

            var existingOrganizationsCount = await _database.Organizations().CountAsync(x => x.OwnerId == ownerId);
            if (existingOrganizationsCount >= _featureSettings.MaxOrganizations)
            {
                throw new ServiceException($"Limit of {_featureSettings.MaxOrganizations} " +
                                           "organizations has been reached.");
            }

            organization = new Organization(name, owner, autoRegisterNewWarden);
            await _database.Organizations().InsertOneAsync(organization);
            Logger.Info($"New organization: '{name}' with id: '{organization}' " +
                        $"was created by user: '{ownerId}'.");
        }

        public async Task AddWardenAsync(Guid organizationId, string name, bool enabled = true)
        {
            var organization = await GetByIdOrFailAsync(organizationId);
            if (organization.Wardens.Count() >= _featureSettings.MaxWardensInOrganization)
            {
                throw new ServiceException($"Limit of {_featureSettings.MaxWardensInOrganization} " +
                                           "wardens in organization has been reached.");
            }

            organization.AddWarden(name, enabled);
            await _database.Organizations().ReplaceOneAsync(x => x.Id == organization.Id, organization);
            Logger.Info($"Warden '{name}' was added to organization: " +
                        $"'{organization.Name}' with id: '{organization.Id}'.");
        }

        public async Task AddUserAsync(Guid organizationId, string email, OrganizationRole role = OrganizationRole.User)
        {
            var organization = await GetByIdOrFailAsync(organizationId);
            var user = await _database.Users().GetByEmailAsync(email);
            if (user == null)
                throw new ServiceException($"User has not been found for email: '{email}'.");

            if (organization.Users.Count() >= _featureSettings.MaxUsersInOrganization)
            {
                throw new ServiceException($"Limit of {_featureSettings.MaxUsersInOrganization} " +
                                           "users in organization has been reached.");
            }

            organization.AddUser(user, role);
            await _database.Organizations().ReplaceOneAsync(x => x.Id == organization.Id, organization);
            Logger.Info($"User '{user.Id}' was added to organization: " +
                        $"'{organization.Name}' with id: '{organization.Id}'.");
        }

        public async Task EnableWardenAsync(Guid organizationId, string name)
        {
            var organization = await GetByIdOrFailAsync(organizationId);
            var warden = organization.GetWardenByNameOrFail(name);
            warden.Enable();
            await _database.Organizations().ReplaceOneAsync(x => x.Id == organization.Id, organization);
            Logger.Info($"Warden '{name}' was enabled in organization: " +
                        $"'{organization.Name}' with id: '{organization.Id}'.");
        }

        public async Task DisableWardenAsync(Guid organizationId, string name)
        {
            var organization = await GetByIdOrFailAsync(organizationId);
            var warden = organization.GetWardenByNameOrFail(name);
            warden.Disable();
            await _database.Organizations().ReplaceOneAsync(x => x.Id == organization.Id, organization);
            Logger.Info($"Warden '{name}' was disabled in organization: " +
                        $"'{organization.Name}' with id: '{organization.Id}'.");
        }

        public async Task<bool> IsUserInOrganizationAsync(Guid organizationId, Guid userId)
        {
            var organization = await GetByIdOrFailAsync(organizationId);

            return organization.Users.Any(x => x.Id == userId);
        }

        public async Task DeleteAsync(Guid organizationId, bool removeAllIterations = true)
        {
            var organization = await GetByIdOrFailAsync(organizationId);
            await _database.Organizations().DeleteOneAsync(x => x.Id == organizationId);
            if (!removeAllIterations)
                return;

            await _database.WardenIterations().DeleteManyAsync(x => x.Warden.OrganizationId == organizationId);
        }

        public async Task DeleteUserAsync(Guid organizationId, Guid userId)
        {
            var organization = await GetByIdOrFailAsync(organizationId);
            organization.RemoveUser(userId);
            await _database.Organizations().ReplaceOneAsync(x => x.Id == organizationId, organization);
            Logger.Info($"Organization: '{organization.Name}' with id: '{organization.Id}' was deleted.");
        }

        public async Task DeleteWardenAsync(Guid organizationId, Guid wardenId)
        {
            var organization = await GetByIdOrFailAsync(organizationId);
            var warden = organization.Wardens.FirstOrDefault(x => x.Id == wardenId);
            if (warden == null)
                throw new ServiceException($"Warden has not been found for id: '{wardenId}'.");

            organization.RemoveWarden(warden.Name);
            await _database.Organizations().ReplaceOneAsync(x => x.Id == organizationId, organization);
            Logger.Info($"Warden '{warden.Name}' was deleted from organization: " +
                        $"'{organization.Name}' with id: '{organization.Id}'.");
        }

        public async Task EnableAutoRegisterNewWardenAsync(Guid organizationId)
        {
            var organization = await GetByIdOrFailAsync(organizationId);
            organization.EnableAutoRegisterNewWarden();
            await _database.Organizations().ReplaceOneAsync(x => x.Id == organizationId, organization);
            Logger.Info("Automatic registration of new Wardens was enabled in organization: " +
                        $"'{organization.Name}' with id: '{organization.Id}'");
        }

        public async Task DisableAutoRegisterNewWardenAsync(Guid organizationId)
        {
            var organization = await GetByIdOrFailAsync(organizationId);
            organization.DisableAutoRegisterNewWarden();
            await _database.Organizations().ReplaceOneAsync(x => x.Id == organizationId, organization);
            Logger.Info("Automatic registration of new Wardens was disabled in organization: " +
                        $"'{organization.Name}' with id: '{organization.Id}'");
        }

        private async Task<Organization> GetByIdOrFailAsync(Guid id)
        {
            var organization = await _database.Organizations().GetByIdAsync(id);
            if (organization == null)
                throw new ServiceException($"Organization has not been found for given id: '{id}'.");

            return organization;
        }
    }
}