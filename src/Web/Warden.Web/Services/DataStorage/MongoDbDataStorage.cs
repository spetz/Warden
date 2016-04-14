using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using Warden.Web.Dto;

namespace Warden.Web.Services.DataStorage
{
    public class MongoDbDataStorage : IDataStorage
    {
        private readonly IMongoDatabase _database;

        public MongoDbDataStorage(IMongoDatabase database)
        {
            _database = database;
        }

        public async Task SaveIterationAsync(WardenIterationDto iterationDto)
        {
            if (iterationDto == null)
                return;
            var watcherCheckResults = (iterationDto.Results?.Select(x => x.WatcherCheckResult)
                                       ?? Enumerable.Empty<WatcherCheckResultDto>()).ToList();
            watcherCheckResults.ForEach(SetWatcherType);
            await _database.GetCollection<WardenIterationDto>("Iterations").InsertOneAsync(iterationDto);
        }

        private static void SetWatcherType(WatcherCheckResultDto watcherCheck)
        {
            watcherCheck.WatcherType = string.IsNullOrWhiteSpace(watcherCheck.WatcherTypeFullName)
                ? string.Empty
                : watcherCheck.WatcherTypeFullName.Contains(",")
                    ? watcherCheck.WatcherTypeFullName.Split(',').FirstOrDefault()?.Split('.').LastOrDefault()
                    : watcherCheck.WatcherTypeFullName;
        }
    }
}