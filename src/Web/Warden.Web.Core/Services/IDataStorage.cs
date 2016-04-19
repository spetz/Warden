using System.Threading.Tasks;
using Warden.Web.Core.Dto;
using Warden.Web.Core.Queries;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System.Linq;
using Warden.Web.Core.Domain;

namespace Warden.Web.Core.Services
{
    public interface IDataStorage
    {
        Task SaveIterationAsync(WardenIterationDto iteration);
        Task<PagedResult<WardenIterationDto>> GetIterationsAsync(BrowseWardenIterations query);
    }

    public class MongoDbDataStorage : IDataStorage
    {
        private const string CollectionName = "Iterations";
        private const string WatcherNameSuffix = "watcher";
        private readonly IMongoDatabase _database;

        public MongoDbDataStorage(IMongoDatabase database)
        {
            _database = database;
        }

        public async Task SaveIterationAsync(WardenIterationDto iteration)
        {
            if (iteration == null)
                return;
            var watcherCheckResults = (iteration.Results?.Select(x => x.WatcherCheckResult)
                                       ?? Enumerable.Empty<WatcherCheckResultDto>()).ToList();
            watcherCheckResults.ForEach(SetWatcherType);

            //TODO: Create warden iteration entity with valid organization
            var wardenIteration = new WardenIteration(iteration.WardenName, null,
                iteration.Ordinal, iteration.StartedAt, iteration.CompletedAt, iteration.IsValid);

            await _database.GetCollection<WardenIteration>(CollectionName).InsertOneAsync(wardenIteration);
        }

        public async Task<PagedResult<WardenIterationDto>> GetIterationsAsync(BrowseWardenIterations query)
        {
            if (query == null)
                return PagedResult<WardenIterationDto>.Empty;

            var iterations = _database.GetCollection<WardenIteration>(CollectionName).AsQueryable();
            switch (query.ResultType)
            {
                case ResultType.Valid:
                    iterations = iterations.Where(x => x.IsValid);
                    break;
                case ResultType.Invalid:
                    iterations = iterations.Where(x => !x.IsValid);
                    break;
            }

            var watcherName = query.WatcherName?.Trim() ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(watcherName))
            {
                iterations = iterations.Where(x =>
                    x.Results.Any(r => r.WatcherCheckResult.Watcher.Name == watcherName));
            }

            if (query.WatcherTypeName.HasValue)
            {
                iterations = iterations.Where(x =>
                    x.Results.Any(r => r.WatcherCheckResult.Watcher.Type == query.WatcherTypeName));
            }

            if (query.From.HasValue)
                iterations = iterations.Where(x => x.CompletedAt >= query.From);
            if (query.To.HasValue)
                iterations = iterations.Where(x => x.CompletedAt <= query.To);

            //TODO: Mapping
            //return await iterations
            //    .OrderByDescending(x => x.CompletedAt)
            //    .PaginateAsync(query);

            return PagedResult<WardenIterationDto>.Empty;
        }

        private static void SetWatcherType(WatcherCheckResultDto watcherCheck)
        {
            watcherCheck.WatcherType = (string.IsNullOrWhiteSpace(watcherCheck.WatcherType)
                ? string.Empty
                : watcherCheck.WatcherType.Contains(",")
                    ? watcherCheck.WatcherType.Split(',').FirstOrDefault()?.Split('.').LastOrDefault() ??
                      string.Empty
                    : watcherCheck.WatcherType).Trim().ToLowerInvariant().Replace(WatcherNameSuffix, string.Empty);
        }
    }

}