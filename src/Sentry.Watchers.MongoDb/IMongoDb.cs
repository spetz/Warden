using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Sentry.Watchers.MongoDb
{
    public interface IMongoDb
    {
        Task<IEnumerable<dynamic>> QueryAsync(IMongoDbConnection connection, string collection, string query);
    }

    public class MongoDb : IMongoDb
    {
        private readonly IMongoDatabase _database;

        public MongoDb(IMongoDatabase database)
        {
            _database = database;
        }

        public async Task<IEnumerable<dynamic>> QueryAsync(IMongoDbConnection connection, string collection,
            string query)
        {
            var findQuery = MongoDB.Bson.Serialization.BsonSerializer.Deserialize<BsonDocument>(query);
            var result = await _database.GetCollection<dynamic>(collection).FindAsync(findQuery);

            return await result.ToListAsync();
        }
    }
}