using System;
using System.Threading.Tasks;
using System.Linq;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Warden.Web.Core.Domain;
using Warden.Web.Core.Queries;

namespace Warden.Web.Core.Mongo.Queries
{
    public static class WardenIterationQueries
    {
        public static IMongoCollection<WardenIteration> WardenIterations(this IMongoDatabase database)
            => database.GetCollection<WardenIteration>("WardenIterations");

        public static async Task<WardenIteration> GetByIdAsync(this IMongoCollection<WardenIteration> iterations, 
            Guid id)
        {
            if (id == Guid.Empty)
                return null;

            return await iterations.AsQueryable().FirstOrDefaultAsync(x => x.Id == id);
        }

        public static IMongoQueryable<WardenIteration> Query(this IMongoCollection<WardenIteration> iterations,
            BrowseWardenIterations query)
        {
            var values = iterations.AsQueryable();
            switch (query.ResultType)
            {
                case ResultType.Valid:
                    values = values.Where(x => x.IsValid);
                    break;
                case ResultType.Invalid:
                    values = values.Where(x => !x.IsValid);
                    break;
            }

            var watcherName = query.WatcherName?.Trim() ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(watcherName))
            {
                values = values.Where(x =>
                    x.Results.Any(r => r.WatcherCheckResult.Watcher.Name == watcherName));
            }

            if (query.WatcherType.HasValue)
            {
                values = values.Where(x =>
                    x.Results.Any(r => r.WatcherCheckResult.Watcher.Type == query.WatcherType));
            }

            if (query.From.HasValue)
                values = values.Where(x => x.CompletedAt >= query.From);
            if (query.To.HasValue)
                values = values.Where(x => x.CompletedAt <= query.To);

            return values;
        }
    }
}