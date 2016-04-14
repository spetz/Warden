using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Warden.Web.Dto;

namespace Warden.Web.Services.DataStorage
{
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

        public async Task<IEnumerable<WardenIterationDto>> GetIterationsAsync(WardenIterationFiltersDto filters)
        {
            if (filters == null)
                return Enumerable.Empty<WardenIterationDto>();

            var iterations = _database.GetCollection<WardenIterationDto>(CollectionName).AsQueryable();
            switch (filters.ResultType)
            {
                case ResultType.Valid:
                    iterations = iterations.Where(x => x.IsValid);
                    break;
                case ResultType.Invalid:
                    iterations = iterations.Where(x => !x.IsValid);
                    break;
            }

            var watcherName = filters.WatcherName?.Trim() ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(watcherName))
            {
                iterations = iterations.Where(x =>
                    x.Results.Any(r => r.WatcherCheckResult.WatcherName == watcherName));
            }

            var watcherTypeName = filters.WatcherTypeName?.Trim().ToLowerInvariant() ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(watcherTypeName))
            {
                iterations = iterations.Where(x =>
                    x.Results.Any(r => r.WatcherCheckResult.WatcherType == watcherTypeName));
            }

            if (filters.From.HasValue)
                iterations = iterations.Where(x => x.CompletedAt >= filters.From);
            if (filters.To.HasValue)
                iterations = iterations.Where(x => x.CompletedAt <= filters.To);

            return await iterations.ToListAsync();
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