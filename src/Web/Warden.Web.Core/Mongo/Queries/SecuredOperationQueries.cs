using System;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Warden.Web.Core.Domain;
using Warden.Web.Core.Extensions;

namespace Warden.Web.Core.Mongo.Queries
{
    public static class SecuredOperationQueries
    {
        public static IMongoCollection<SecuredOperation> SecuredOperations(this IMongoDatabase database)
            => database.GetCollection<SecuredOperation>("SecuredOperations");

        public static async Task<SecuredOperation> GetByUserIdAndTokenAsync(
            this IMongoCollection<SecuredOperation> operations,
            Guid userId, string token)
        {
            if (userId == Guid.Empty)
                return null;

            if (token.Empty())
                return null;

            var operation = await operations.AsQueryable()
                .FirstOrDefaultAsync(x => x.UserId == userId && x.Token.Equals(token));

            return operation;
        }

        public static async Task<SecuredOperation> GetByEmailAndTokenAsync(
            this IMongoCollection<SecuredOperation> operations,
            string email, string token)
        {
            if (email.Empty())
                return null;

            if (token.Empty())
                return null;

            var fixedEmail = email.ToLowerInvariant();
            var operation =
                await operations.AsQueryable()
                    .FirstOrDefaultAsync(x => x.Email.Equals(fixedEmail) && x.Token.Equals(token));

            return operation;
        }
    }
}