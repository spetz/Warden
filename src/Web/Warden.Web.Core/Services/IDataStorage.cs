using System.Threading.Tasks;
using Warden.Web.Core.Dto;
using Warden.Web.Core.Models;
using Warden.Web.Core.Queries;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Warden.Web.Core.Extensions;
using System.Linq;

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
            await _database.GetCollection<WardenIterationDto>(CollectionName).InsertOneAsync(iteration);
        }

        public async Task<PagedResult<WardenIterationDto>> GetIterationsAsync(BrowseWardenIterations query)
        {
            if (query == null)
                return PagedResult<WardenIterationDto>.Empty;

            var iterations = _database.GetCollection<WardenIterationDto>(CollectionName).AsQueryable();
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
                    x.Results.Any(r => r.WatcherCheckResult.WatcherName == watcherName));
            }

            var watcherTypeName = query.WatcherTypeName?.Trim().ToLowerInvariant() ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(watcherTypeName))
            {
                iterations = iterations.Where(x =>
                    x.Results.Any(r => r.WatcherCheckResult.WatcherType == watcherTypeName));
            }

            if (query.From.HasValue)
                iterations = iterations.Where(x => x.CompletedAt >= query.From);
            if (query.To.HasValue)
                iterations = iterations.Where(x => x.CompletedAt <= query.To);

            return await iterations
                .OrderByDescending(x => x.CompletedAt)
                .PaginateAsync(query);
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