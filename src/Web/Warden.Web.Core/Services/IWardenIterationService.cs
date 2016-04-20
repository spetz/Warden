using System;
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
    public interface IWardenIterationService
    {
        Task SaveIterationAsync(WardenIterationDto iteration, Guid organizationId, bool registerWardenIfNotFound = true);
        Task<PagedResult<WardenIterationDto>> GetIterationsAsync(BrowseWardenIterations query);
    }

    public class WardenIterationService : IWardenIterationService
    {
        private const string WatcherNameSuffix = "watcher";
        private readonly IMongoDatabase _database;

        public WardenIterationService(IMongoDatabase database)
        {
            _database = database;
        }

        public async Task SaveIterationAsync(WardenIterationDto iteration, Guid organizationId, bool registerWardenIfNotFound = true)
        {
            if (iteration == null)
                return;

            var wardenCheckResults = (iteration.Results ?? Enumerable.Empty<WardenCheckResultDto>()).ToList();
            var watcherCheckResults = wardenCheckResults.Select(x => x.WatcherCheckResult).ToList();
            watcherCheckResults.ForEach(SetWatcherTypeFromFullNamespace);

            var organization = await _database.Organizations().GetByIdAsync(organizationId);
            if (organization == null)
                throw new ServiceException($"Organization has not been found for given id: '{organizationId}'.");

            var warden = organization.GetWardenByName(iteration.WardenName);
            if (warden == null && !registerWardenIfNotFound)
                throw new ServiceException($"Warden with name: '{iteration.WardenName}' has not been registered.");
            if (warden == null)
            {
                organization.AddWarden(iteration.WardenName);
                await _database.Organizations().ReplaceOneAsync(x => x.Id == organizationId, organization);
            }

            var wardenIteration = new WardenIteration(iteration.WardenName, organization,
                iteration.Ordinal, iteration.StartedAt, iteration.CompletedAt, iteration.IsValid);

            foreach (var result in wardenCheckResults)
            {
                var watcherType =
                    (WatcherType) Enum.Parse(typeof(WatcherType), result.WatcherCheckResult.WatcherType, true);
                var watcher = Watcher.Create(result.WatcherCheckResult.WatcherName, watcherType);
                var watcherCheckResult = result.WatcherCheckResult.IsValid
                    ? WatcherCheckResult.Valid(watcher, result.WatcherCheckResult.Description)
                    : WatcherCheckResult.Invalid(watcher, result.WatcherCheckResult.Description);

                var checkResult = result.IsValid
                    ? WardenCheckResult.Valid(watcherCheckResult, result.StartedAt, result.CompletedAt)
                    : WardenCheckResult.Invalid(watcherCheckResult, result.StartedAt, result.CompletedAt,
                        CreatExceptionInfo(result.Exception));

                wardenIteration.AddResult(checkResult);
            }

            await _database.WardenIterations().InsertOneAsync(wardenIteration);
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

        public async Task<PagedResult<WardenIterationDto>> GetIterationsAsync(BrowseWardenIterations query)
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
    }
}