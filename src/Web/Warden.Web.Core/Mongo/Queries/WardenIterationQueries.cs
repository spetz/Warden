using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Warden.Web.Core.Domain;
using Warden.Web.Core.Extensions;
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


        public static async Task<IEnumerable<WardenIteration>> GetForWardenAsync(this IMongoCollection<WardenIteration> iterations,
            Guid organizationId, string wardenName)
        {
            if (organizationId == Guid.Empty || wardenName.Empty())
                return Enumerable.Empty<WardenIteration>();

            wardenName = wardenName.Trim();

            return await iterations.AsQueryable()
                .Where(x => x.Warden.OrganizationId == organizationId && x.Warden.Name == wardenName)
                .ToListAsync();
        }

        public static IMongoQueryable<WardenIteration> Query(this IMongoCollection<WardenIteration> iterations,
            BrowseWardenIterations query)
        {
            var values = iterations.AsQueryable();
            if (query.OrganizationId != Guid.Empty)
                values = values.Where(x => x.Warden.OrganizationId == query.OrganizationId);
            if (query.WardenName.NotEmpty())
            {
                var fixedWardenName = query.WardenName.TrimToLower();
                values = values.Where(x => x.Warden.Name.ToLower() == fixedWardenName);
            }

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