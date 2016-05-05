using System;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Warden.Web.Core.Domain;
using Warden.Web.Core.Extensions;

namespace Warden.Web.Core.Mongo.Queries
{
    public static class UserQueries
    {
        public static IMongoCollection<User> Users(this IMongoDatabase database)
            => database.GetCollection<User>("Users");

        public static async Task<User> GetByIdAsync(this IMongoCollection<User> users, Guid id)
        {
            if (id == Guid.Empty)
                return null;

            return await users.AsQueryable().FirstOrDefaultAsync(x => x.Id == id);
        }

        public static async Task<User> GetByEmailAsync(this IMongoCollection<User> users, string email)
        {
            if (!email.IsEmail())
                return null;

            var fixedEmail = email.TrimToLower();
            return await users.AsQueryable().FirstOrDefaultAsync(x => x.Email == fixedEmail);
        }

        public static async Task<bool> ExistsAsync(this IMongoCollection<User> users, string email)
        {
            var fixedEmail = email.TrimToLower();
            return await users.AsQueryable().AnyAsync(x => x.Email == fixedEmail);
        }

        public static async Task<bool> IsActiveAsync(this IMongoCollection<User> users, Guid id)
        {
            if (id == Guid.Empty)
                return false;

            return await users.AsQueryable().AnyAsync(x => x.Id == id && x.State == State.Active);
        }
    }
}