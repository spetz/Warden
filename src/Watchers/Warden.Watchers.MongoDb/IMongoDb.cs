using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Warden.Watchers.MongoDb
{
    /// <summary>
    /// Custom MongoDB connector for executing the MongoDB queries.
    /// </summary>
    public interface IMongoDb
    {
        /// <summary>
        /// Executes the MongoDB query and returns a collection of the dynamic results.
        /// </summary>
        /// <param name="collection">Name of the collection in selected MongoDB database.</param>
        /// <param name="query">MongoDB query.</param>
        /// <returns></returns>
        Task<IEnumerable<dynamic>> QueryAsync(string collection, string query);
    }

    /// <summary>
    /// Default implementation of the IMongoDb based on MongoDB Driver.
    /// </summary>
    public class MongoDb : IMongoDb
    {
        private readonly IMongoDatabase _database;

        public MongoDb(IMongoDatabase database)
        {
            _database = database;
        }

        public async Task<IEnumerable<dynamic>> QueryAsync(string collection, string query)
        {
            var findQuery = MongoDB.Bson.Serialization.BsonSerializer.Deserialize<BsonDocument>(query);
            var result = await _database.GetCollection<dynamic>(collection).FindAsync(findQuery);

            return await result.ToListAsync();
        }
    }
}