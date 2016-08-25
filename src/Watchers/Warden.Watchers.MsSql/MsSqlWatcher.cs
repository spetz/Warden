using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Warden.Watchers.MsSql
{
    /// <summary>
    /// MsSqlWatcher designed for MSSQL monitoring.
    /// </summary>
    public class MsSqlWatcher : IWatcher
    {
        private readonly IMsSql _msSql;
        private readonly MsSqlWatcherConfiguration _configuration;
        public string Name { get; }
        public string Group { get; }
        public const string DefaultName = "MSSQL Watcher";

        protected MsSqlWatcher(string name, MsSqlWatcherConfiguration configuration, string group)
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
            Group = group;
            _msSql = _configuration.MsSqlProvider();
        }

        public async Task<IWatcherCheckResult> ExecuteAsync()
        {
            try
            {
                using (var connection = _configuration.ConnectionProvider(_configuration.ConnectionString))
                {
                    connection.Open();
                    if (string.IsNullOrWhiteSpace(_configuration.Query))
                    {
                        return MsSqlWatcherCheckResult.Create(this, true, _configuration.ConnectionString,
                            $"Database: {_configuration.Database} has been sucessfully checked.");
                    }

                    return await ExecuteForQueryAsync(connection);
                }
            }
            catch (SqlException exception)
            {
                return MsSqlWatcherCheckResult.Create(this, false, _configuration.ConnectionString, 
                    _configuration.Query, Enumerable.Empty<dynamic>(), exception.Message);
            }
            catch (Exception exception)
            {
                throw new WatcherException("There was an error while trying to access MSSQL database.", exception);
            }
        }

        private async Task<IWatcherCheckResult> ExecuteForQueryAsync(IDbConnection connection)
        {
            var queryResult = await _msSql.QueryAsync(connection, _configuration.Query,
                _configuration.QueryParameters, _configuration.QueryTimeout);

            var isValid = true;
            if (_configuration.EnsureThatAsync != null)
                isValid = await _configuration.EnsureThatAsync?.Invoke(queryResult);

            isValid = isValid && (_configuration.EnsureThat?.Invoke(queryResult) ?? true);
            var description = $"MSSQL check has returned {(isValid ? "valid" : "invalid")} result for " +
                              $"database: '{_configuration.Database}' and given query.";

            return MsSqlWatcherCheckResult.Create(this, isValid, _configuration.ConnectionString,
                _configuration.Query, queryResult, description);
        }

        /// <summary>
        /// Factory method for creating a new instance of MsSqlWatcher with default name of MSSQL Watcher.
        /// </summary>
        /// <param name="connectionString">Connection string of the MSSQL database.</param>
        /// <param name="configurator">Optional lambda expression for configuring the MsSqlWatcher.</param>
        /// <param name="group">Optional name of the group that MsSqlWatcher belongs to.</param>
        /// <returns>Instance of MsSqlWatcher.</returns>
        public static MsSqlWatcher Create(string connectionString,
            Action<MsSqlWatcherConfiguration.Default> configurator = null,
            string group = null)
            => Create(DefaultName, connectionString, configurator, group);

        /// <summary>
        /// Factory method for creating a new instance of MsSqlWatcher.
        /// </summary>
        /// <param name="name">Name of the MsSqlWatcher.</param>
        /// <param name="connectionString">Connection string of the MSSQL database.</param>
        /// <param name="configurator">Optional lambda expression for configuring the MsSqlWatcher.</param>
        /// <param name="group">Optional name of the group that MsSqlWatcher belongs to.</param>
        /// <returns>Instance of MsSqlWatcher.</returns>
        public static MsSqlWatcher Create(string name, string connectionString,
            Action<MsSqlWatcherConfiguration.Default> configurator = null,
            string group = null)
        {
            var config = new MsSqlWatcherConfiguration.Builder(connectionString);
            configurator?.Invoke((MsSqlWatcherConfiguration.Default) config);

            return Create(name, config.Build(), group);
        }

        /// <summary>
        /// Factory method for creating a new instance of MsSqlWatcher with default name of MSSQL Watcher.
        /// </summary>
        /// <param name="configuration">Configuration of MsSqlWatcher.</param>
        /// <param name="group">Optional name of the group that MsSqlWatcher belongs to.</param>
        /// <returns>Instance of MsSqlWatcher.</returns>
        public static MsSqlWatcher Create(MsSqlWatcherConfiguration configuration, string group = null)
            => Create(DefaultName, configuration, group);

        /// <summary>
        /// Factory method for creating a new instance of MsSqlWatcher.
        /// </summary>
        /// <param name="name">Name of the MsSqlWatcher.</param>
        /// <param name="configuration">Configuration of MsSqlWatcher.</param>
        /// <param name="group">Optional name of the group that MsSqlWatcher belongs to.</param>
        /// <returns>Instance of MsSqlWatcher.</returns>
        public static MsSqlWatcher Create(string name, MsSqlWatcherConfiguration configuration,
            string group = null)
            => new MsSqlWatcher(name, configuration, group);
    }
}