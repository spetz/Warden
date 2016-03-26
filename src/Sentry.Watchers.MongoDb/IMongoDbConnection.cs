using System;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace Sentry.Watchers.MongoDb
{
    public interface IMongoDbConnection
    {
        string Database { get; }
        string ConnectionString { get; }
        TimeSpan Timeout { get; }
        Task<IMongoDb> GetDatabaseAsync();
    }

    public class MongoDbConnection : IMongoDbConnection
    {
        private readonly MongoClient _client;
        public string Database { get; }
        public string ConnectionString { get; }
        public TimeSpan Timeout { get; }

        public MongoDbConnection(string database, string connectionString, TimeSpan timeout)
        {
            Database = database;
            ConnectionString = connectionString;
            Timeout = timeout;
            _client = new MongoClient(InitializeSettings());
        }

        public async Task<IMongoDb> GetDatabaseAsync()
        {
            var databases = await _client.ListDatabasesAsync();
            var hasDatabase = false;
            while (await databases.MoveNextAsync())
            {
                hasDatabase = databases.Current.Any(x => x["name"] == Database);
            }

            return hasDatabase ? new MongoDb(_client.GetDatabase(Database)) : null;
        }

        protected MongoClientSettings InitializeSettings()
        {
            var settings = new MongoClientSettings
            {
                Server = GetServerAddress(),
                ConnectTimeout = Timeout,
                ServerSelectionTimeout = Timeout
            };

            return settings;
        }

        protected MongoServerAddress GetServerAddress()
        {
            //Remove the "mongodb://" substring
            var cleanedConnectionString = ConnectionString.Substring(10);
            var hostAndPort = cleanedConnectionString.Split(':');

            return new MongoServerAddress(hostAndPort[0], int.Parse(hostAndPort[1]));
        }
    }
}