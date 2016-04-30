using MongoDB.Driver;
using Warden.Web.Core.Domain;

namespace Warden.Web.Core.Mongo.Queries
{
    public static class UserSessionQueries
    {
        public static IMongoCollection<UserSession> UserSessions(this IMongoDatabase database)
            => database.GetCollection<UserSession>("UserSessions");
    }
}