using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using NLog;
using Warden.Web.Core.Domain;
using Warden.Web.Core.Dto;
using Warden.Web.Core.Extensions;
using Warden.Web.Core.Mongo;
using Warden.Web.Core.Mongo.Queries;
using Warden.Web.Core.Queries;

namespace Warden.Web.Core.Services
{
    public interface IWatcherService
    {
        Task<IEnumerable<WatcherDto>> GetAllAsync(Guid organizationId, Guid wardenId);
        Task<WatcherDto> GetAsync(Guid organizationId, Guid wardenId, string name);
        Task<WatcherStatsDto> GetStatsAsync(GetWatcherStats query);
        Task<PagedResult<WardenCheckResultDto>> BrowseChecksAsync(BrowseWardenCheckResults query);
    }

    public class WatcherService : IWatcherService
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly IMongoDatabase _database;
        private readonly IStatsCalculator _statsCalculator;

        public WatcherService(IMongoDatabase database, IStatsCalculator statsCalculator)
        {
            _database = database;
            _statsCalculator = statsCalculator;
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
            var watcher = await GetAsync(query.OrganizationId, query.WardenId, query.WatcherName);
            if (watcher == null)
                return null;

            var iterations = await _database.WardenIterations()
               .GetForWatcherAsync(query.OrganizationId, query.WardenId, query.WatcherName);
            var results = iterations.SelectMany(x => x.Results)
                .Select(x => x.WatcherCheckResult)
                .Where(x => x.Watcher.Name.EqualsCaseInvariant(query.WatcherName))
                .ToList();
            var stats = _statsCalculator.Calculate(results);
            Logger.Trace($"Statistics for watcher {watcher.Name} for Warden: '{query.WardenId}' " +
                         $"in organization: '{query.OrganizationId}' were created.");

            return new WatcherStatsDto
            {
                Name = watcher.Name,
                Type = watcher.Type,
                TotalUptime = stats.TotalUptime,
                TotalDowntime = stats.TotalDowntime,
                TotalChecks = stats.TotalResults,
                TotalValidChecks = stats.TotalValidResults,
                TotalInvalidChecks = stats.TotalInvalidResults
            };
        }

        public async Task<PagedResult<WardenCheckResultDto>> BrowseChecksAsync(BrowseWardenCheckResults query)
        {
            var iterations = await _database.WardenIterations().Query(new BrowseWardenIterations
            {
                OrganizationId = query.OrganizationId,
                WardenName = query.WardenName,
                WatcherName = query.WatcherName,
                Page = query.Page,
                Results = query.Results
            }).PaginateAsync(query);

            var checkResults = new List<WardenCheckResultDto>();
            foreach (var iteration in iterations.Items)
            {
                var checks = iteration.Results.Where(r =>
                    r.WatcherCheckResult.Watcher.Name.EqualsCaseInvariant(query.WatcherName))
                    .Select(x => new WardenCheckResultDto(x)
                    {
                        IterationId = iteration.Id
                    });
                checkResults.AddRange(checks);
            }

            return PagedResult<WardenCheckResultDto>.From(iterations, checkResults);
        }
    }
}