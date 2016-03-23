using System;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Sentry.Core;

namespace Sentry.Watchers.MsSql
{
    public class MsSqlWatcher : IWatcher
    {
        private readonly IQueryExecutor queryExecutor;
        private readonly MsSqlWatcherConfiguration _configuration;
        public string Name { get; }

        protected MsSqlWatcher(string name, MsSqlWatcherConfiguration configuration)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Watcher name can not be empty.");

            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration),
                    "MSSQL Watcher configuration has not been provided.");
            }

            Name = name;
            _configuration = configuration;
            queryExecutor = _configuration.QueryExecutorProvider();
        }

        public async Task<IWatcherCheckResult> ExecuteAsync()
        {
            try
            {
                using (var connection = _configuration.ConnectionProvider(_configuration.ConnectionString))
                {
                    connection.Open();
                    if (string.IsNullOrWhiteSpace(_configuration.Query))
                        return WatcherCheckResult.Create(this, true);

                    var result = await queryExecutor.QueryAsync(connection, _configuration.Query,
                        _configuration.QueryParameters, _configuration.Timeout);

                    var collection = result.ToList();
                    var isValid = true;
                    if (_configuration.EnsureThatAsync != null)
                        isValid = await _configuration.EnsureThatAsync?.Invoke(collection);

                    isValid = isValid && (_configuration.EnsureThat?.Invoke(collection) ?? true);

                    return WatcherCheckResult.Create(this, isValid);
                }
            }
            catch (SqlException ex)
            {
                return WatcherCheckResult.Create(this, false, ex.Message);
            }
            catch (Exception ex)
            {
                throw new WatcherException("There was an error while trying to access MSSQL database.", ex);
            }
        }

        public static MsSqlWatcher Create(string name, string connectionString,
            Action<MsSqlWatcherConfiguration.Default> configurator = null)
        {
            var config = new MsSqlWatcherConfiguration.Builder(connectionString);
            configurator?.Invoke((MsSqlWatcherConfiguration.Default)config);

            return Create(name, config.Build());
        }

        public static MsSqlWatcher Create(string name, MsSqlWatcherConfiguration configuration)
            => new MsSqlWatcher(name, configuration);
    }
}