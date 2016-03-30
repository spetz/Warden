using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace Sentry.Watchers.Redis
{
    /// <summary>
    /// Custom Redis server connector for executing the commands.
    /// </summary>
    public interface IRedis
    {
        /// <summary>
        /// Executes the Redis query and returns a collection of the dynamic results.
        /// </summary>
        /// <param name="query">Redis query.</param>
        /// <returns></returns>
        Task<IEnumerable<dynamic>> QueryAsync(string query);
    }

    /// <summary>
    /// Default implementation of the IRedis based on StackExchange.Redis.
    /// </summary>
    public class Redis : IRedis
    {
        private readonly IDatabase _database;

        public Redis(IDatabase database)
        {
            _database = database;
        }

        public async Task<IEnumerable<dynamic>> QueryAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return Enumerable.Empty<dynamic>();

            var command = new Command(query);

            return await ExecuteCommandAsync(command);
        }

        private async Task<IEnumerable<dynamic>> ExecuteCommandAsync(Command command)
        {
            if (string.IsNullOrWhiteSpace(command.Name))
                return Enumerable.Empty<dynamic>();

            switch (command.Name)
            {
                case "get":
                    return await ExecuteGetAsync(command);
                case "set":
                    return await ExecuteSetAsync(command);
                case "lrange":
                    return await ExecuteLRangeAsync(command);
                default:
                    throw new Exception($"Redis command is not implemented: '{command.Name}'.");
            }
        }

        private async Task<IEnumerable<dynamic>> ExecuteGetAsync(Command command)
            => new List<dynamic> {await _database.StringGetAsync(command.Arguments[0])};

        private async Task<IEnumerable<dynamic>> ExecuteSetAsync(Command command)
            => new List<dynamic> {await _database.StringSetAsync(command.Arguments[0], command.Arguments[1])};

        private async Task<IEnumerable<dynamic>> ExecuteLRangeAsync(Command command)
            => new List<dynamic>
            {
                await _database.ListRangeAsync(command.Arguments[0],
                    long.Parse(command.Arguments[1]),
                    long.Parse(command.Arguments[2]))
            };

        private class Command
        {
            public string Name { get; }
            public string[] Arguments { get; }

            public Command(string query)
            {
                var command = query.ToLowerInvariant().Trim().Split(' ');
                if (!command.Any())
                    return;

                Name = command[0].ToLowerInvariant();
                Arguments = command.Skip(1).ToArray();
            }
        }
    }
}