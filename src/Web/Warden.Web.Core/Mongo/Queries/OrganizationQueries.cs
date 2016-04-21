using System;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Warden.Web.Core.Domain;
using Warden.Web.Core.Extensions;
using Warden.Web.Core.Queries;

namespace Warden.Web.Core.Mongo.Queries
{
    public static class OrganizationQueries
    {
        public static IMongoCollection<Organization> Organizations(this IMongoDatabase database)
            => database.GetCollection<Organization>("Organizations");

        public static async Task<Organization> GetByIdAsync(this IMongoCollection<Organization> organizations,
            Guid id)
        {
            if (id == Guid.Empty)
                return null;

            return await organizations.AsQueryable().FirstOrDefaultAsync(x => x.Id == id);
        }

        public static async Task<Organization> GetByNameForOwnerAsync(this IMongoCollection<Organization> organizations,
            string name, Guid ownerId)
        {
            if (name.Empty() || ownerId == Guid.Empty)
                return null;

            return await organizations.AsQueryable().FirstOrDefaultAsync(x => x.Name == name && x.OwnerId == ownerId);
        }

        public static IMongoQueryable<Organization> Query(this IMongoCollection<Organization> organizations,
            BrowseOrganizations query)
        {
            var values = organizations.AsQueryable();
            if (query.UserId != Guid.Empty)
                values.Where(x => x.Users.Any(u => u.Id == query.UserId));
            if (query.OwnerId != Guid.Empty)
                values.Where(x => x.OwnerId == query.OwnerId);

            return values;
        }
    }
}