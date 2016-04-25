using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Warden.Web.Core.Domain;
using Warden.Web.Core.Extensions;

namespace Warden.Web.Core.Mongo.Queries
{
    public static class ApiKeyQueries
    {
        public static IMongoCollection<ApiKey> ApiKeys(this IMongoDatabase database)
            => database.GetCollection<ApiKey>("ApiKeys");

        public static async Task<ApiKey> GetByKeyAsync(this IMongoCollection<ApiKey> keys,
            string key)
        {
            if (key.Empty())
                return null;

            return await keys.AsQueryable().FirstOrDefaultAsync(x => x.Key == key);
        }

        public static async Task<IEnumerable<ApiKey>> GetAllForUserAsync(this IMongoCollection<ApiKey> keys,
            Guid userId)
        {
            if (userId == Guid.Empty)
                return Enumerable.Empty<ApiKey>();

            return await keys.AsQueryable()
                .Where(x => x.UserId == userId)
                .ToListAsync();
        }

        public static async Task<bool> ExistsAsync(this IMongoCollection<ApiKey> keys,
            string key) => await keys.AsQueryable().AnyAsync(x => x.Key == key);
    }
}