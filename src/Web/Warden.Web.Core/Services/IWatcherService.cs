using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using Warden.Web.Core.Domain;
using Warden.Web.Core.Dto;
using Warden.Web.Core.Mongo.Queries;
using Warden.Web.Core.Queries;

namespace Warden.Web.Core.Services
{
    public interface IWatcherService
    {
        Task<IEnumerable<WatcherDto>> GetAllAsync(Guid organizationId, Guid wardenId);
        Task<WatcherDto> GetAsync(Guid organizationId, Guid wardenId, string name);
        Task<WatcherStatsDto> GetStatsAsync(GetWatcherStats query);
    }

    public class WatcherService : IWatcherService
    {
        private readonly IMongoDatabase _database;

        public WatcherService(IMongoDatabase database)
        {
            _database = database;
        }

        public async Task<IEnumerable<WatcherDto>> GetAllAsync(Guid organizationId, Guid wardenId)
        {
            if (organizationId == Guid.Empty || wardenId == Guid.Empty)
                return Enumerable.Empty<WatcherDto>();

            var warden = await _database.Organizations().GetWardenByIdAsync(organizationId, wardenId);
            if (warden == null)
                throw new ServiceException($"Warden has not been found for given id: '{wardenId}'.");

            return warden.Watchers.Select(x => new WatcherDto(x));
        }

        public async Task<WatcherDto> GetAsync(Guid organizationId, Guid wardenId, string name)
        {
            var warden = await _database.Organizations().GetWardenByIdAsync(organizationId, wardenId);
            var watcher = warden?.GetWatcherByName(name);

            return watcher == null ? null : new WatcherDto(watcher);
        }

        public async Task<WatcherStatsDto> GetStatsAsync(GetWatcherStats query)
        {
            throw new NotImplementedException();
        }
    }
}