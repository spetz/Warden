using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Warden.Web.Core.Dto;
using Warden.Web.Core.Queries;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System.Linq;
using Warden.Web.Core.Domain;
using Warden.Web.Core.Mongo;
using Warden.Web.Core.Mongo.Queries;

namespace Warden.Web.Core.Services
{
    public interface IWardenService
    {
        Task<WardenIterationDto> SaveIterationAsync(WardenIterationDto iteration, Guid organizationId);
        Task<PagedResult<WardenIterationDto>> BrowseIterationsAsync(BrowseWardenIterations query);
        Task<WardenIterationDto> GetIterationAsync(Guid id);
        Task<WardenStatsDto> GetStatsAsync(GetWardenStats query);
    }

    public class WardenService : IWardenService
    {
        private const string WatcherNameSuffix = "watcher";
        private readonly IMongoDatabase _database;
        private readonly IStatsCalculator _statsCalculator;

        public WardenService(IMongoDatabase database, IStatsCalculator statsCalculator)
        {
            _database = database;
            _statsCalculator = statsCalculator;
        }

        public async Task<WardenIterationDto> SaveIterationAsync(WardenIterationDto iteration, Guid organizationId)
        {
            if (iteration == null)
                return null;

            var wardenCheckResults = (iteration.Results ?? Enumerable.Empty<WardenCheckResultDto>()).ToList();
            var watcherCheckResults = wardenCheckResults.Select(x => x.WatcherCheckResult).ToList();
            watcherCheckResults.ForEach(SetWatcherTypeFromFullNamespace);

            var organization = await _database.Organizations().GetByIdAsync(organizationId);
            if (organization == null)
                throw new ServiceException($"Organization has not been found for given id: '{organizationId}'.");

            var warden = organization.GetWardenByName(iteration.WardenName);
            if (warden == null && !organization.AutoRegisterNewWarden)
                throw new ServiceException($"Warden with name: '{iteration.WardenName}' has not been registered.");

            var updateOrganization = false;
            if (warden == null)
            {
                updateOrganization = true;
                organization.AddWarden(iteration.WardenName);
                warden = organization.GetWardenByName(iteration.WardenName);
            }
            else if(!warden.Enabled)
                throw new ServiceException($"Warden with name: '{iteration.WardenName}' is disabled.");

            var wardenIteration = new WardenIteration(warden.Name, organization,
                iteration.Ordinal, iteration.StartedAt, iteration.CompletedAt, iteration.IsValid);

            foreach (var result in wardenCheckResults)
            {
                var watcherName = result.WatcherCheckResult.WatcherName;
                var watcherType = (WatcherType)Enum.Parse(typeof(WatcherType), result.WatcherCheckResult.WatcherType, true);
                var watcher = warden.GetWatcherByName(watcherName);
                if (watcher == null)
                {
                    updateOrganization = true;
                    warden.AddWatcher(watcherName, watcherType);
                    watcher = warden.GetWatcherByName(watcherName);
                }

                var watcherCheckResult = result.WatcherCheckResult.IsValid
                    ? WatcherCheckResult.Valid(watcher, result.WatcherCheckResult.Description)
                    : WatcherCheckResult.Invalid(watcher, result.WatcherCheckResult.Description);

                var checkResult = result.IsValid
                    ? WardenCheckResult.Valid(watcherCheckResult, result.StartedAt, result.CompletedAt)
                    : WardenCheckResult.Invalid(watcherCheckResult, result.StartedAt, result.CompletedAt,
                        CreatExceptionInfo(result.Exception));

                wardenIteration.AddResult(checkResult);
            }

            if (updateOrganization)
                await _database.Organizations().ReplaceOneAsync(x => x.Id == organizationId, organization);

            await _database.WardenIterations().InsertOneAsync(wardenIteration);

            return new WardenIterationDto(wardenIteration);
        }

        private static void SetWatcherTypeFromFullNamespace(WatcherCheckResultDto watcherCheck)
        {
            watcherCheck.WatcherType = (string.IsNullOrWhiteSpace(watcherCheck.WatcherType)
                ? string.Empty
                : watcherCheck.WatcherType.Contains(",")
                    ? watcherCheck.WatcherType.Split(',').FirstOrDefault()?.Split('.').LastOrDefault() ??
                      string.Empty
                    : watcherCheck.WatcherType).Trim().ToLowerInvariant().Replace(WatcherNameSuffix, string.Empty);
        }

        private static ExceptionInfo CreatExceptionInfo(ExceptionDto dto)
        {
            return dto == null
                ? null
                : ExceptionInfo.Create(dto.Message, dto.Source,
                    dto.StackTrace, CreatExceptionInfo(dto.InnerException));
        }

        public async Task<PagedResult<WardenIterationDto>> BrowseIterationsAsync(BrowseWardenIterations query)
        {
            if (query == null)
                return PagedResult<WardenIterationDto>.Empty;
            if (query.OrganizationId == Guid.Empty)
                return PagedResult<WardenIterationDto>.Empty;

            var iterations = await _database.WardenIterations()
                .Query(query)
                .OrderByDescending(x => x.CompletedAt)
                .PaginateAsync(query);

            return PagedResult<WardenIterationDto>.From(iterations,
                iterations.Items.Select(x => new WardenIterationDto(x)));
        }

        public async Task<WardenIterationDto> GetIterationAsync(Guid id)
        {
            var iteration = await _database.WardenIterations().GetByIdAsync(id);

            return iteration == null ? null : new WardenIterationDto(iteration);
        }

        public async Task<WardenStatsDto> GetStatsAsync(GetWardenStats query)
        {
            var warden = await _database.Organizations()
                .GetWardenByNameAsync(query.OrganizationId, query.WardenName);
            if (warden == null)
                return null;

            var iterations = await _database.WardenIterations()
                .GetForWardenAsync(query.OrganizationId, query.WardenName);
            var stats = GetWardenStats(iterations.ToList());
            stats.OrganizationId = query.OrganizationId;
            stats.WardenName = warden.Name;
            stats.Enabled = warden.Enabled;

            return stats;
        }

        private WardenStatsDto GetWardenStats(IList<WardenIteration> iterations)
        {
            var wardenStats = _statsCalculator.Calculate(iterations);
            var stats = new WardenStatsDto
            {
                TotalUptime = wardenStats.TotalUptime,
                TotalDowntime = wardenStats.TotalDowntime,
                TotalIterations = wardenStats.TotalResults,
                TotalValidIterations = wardenStats.TotalValidResults,
                TotalInvalidIterations = wardenStats.TotalInvalidResults
            };

            var watcherCheckResults = iterations.SelectMany(x => x.Results)
                .Select(x => x.WatcherCheckResult)
                .GroupBy(x => x.Watcher.Name, x => x)
                .ToList();

            stats.Watchers = (from watcherGroup in watcherCheckResults
                let watcher = watcherGroup.First().Watcher
                let watcherStats = _statsCalculator.Calculate(watcherGroup)
                select new WatcherStatsDto
                {
                    Name = watcher.Name,
                    Type = watcher.Type.ToString().ToLowerInvariant(),
                    TotalUptime = watcherStats.TotalUptime,
                    TotalDowntime = watcherStats.TotalDowntime,
                    TotalChecks = watcherStats.TotalResults,
                    TotalValidChecks = watcherStats.TotalValidResults,
                    TotalInvalidChecks = watcherStats.TotalInvalidResults,
                })
                .OrderBy(x => x.Type);

            return stats;
        }
    }
}