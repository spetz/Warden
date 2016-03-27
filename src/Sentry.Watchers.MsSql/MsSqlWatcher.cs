using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Sentry.Core;

namespace Sentry.Watchers.MsSql
{
    /// <summary>
    /// MsSqlWatcher designed for MSSQL monitoring.
    /// </summary>
    public class MsSqlWatcher : IWatcher
    {
        private readonly IMsSql _msSql;
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
                        return MsSqlWatcherCheckResult.Create(this, true, _configuration.ConnectionString);

                    var queryResult = await _msSql.QueryAsync(connection, _configuration.Query,
                        _configuration.QueryParameters, _configuration.QueryTimeout);

                    var isValid = true;
                    if (_configuration.EnsureThatAsync != null)
                        isValid = await _configuration.EnsureThatAsync?.Invoke(queryResult);

                    isValid = isValid && (_configuration.EnsureThat?.Invoke(queryResult) ?? true);

                    return MsSqlWatcherCheckResult.Create(this, isValid, _configuration.ConnectionString,
                        _configuration.Query, queryResult);
                }
            }
            catch (SqlException ex)
            {
                return MsSqlWatcherCheckResult.Create(this, false, _configuration.ConnectionString, ex.Message);
            }
            catch (Exception ex)
            {
                throw new WatcherException("There was an error while trying to access MSSQL database.", ex);
            }
        }

        /// <summary>
        /// Factory method for creating a new instance of MsSqlWatcher.
        /// </summary>
        /// <param name="name">Name of the MsSqlWatcher.</param>
        /// <param name="connectionString">Connection string of the MSSQL databse.</param>
        /// <param name="configurator">Optional lambda expression for configuring the MsSqlWatcher.</param>
        /// <returns>Instance of MsSqlWatcher.</returns>
        public static MsSqlWatcher Create(string name, string connectionString,
            Action<MsSqlWatcherConfiguration.Default> configurator = null)
        {
            var config = new MsSqlWatcherConfiguration.Builder(connectionString);
            configurator?.Invoke((MsSqlWatcherConfiguration.Default) config);

            return Create(name, config.Build());
        }

        /// <summary>
        /// Factory method for creating a new instance of MsSqlWatcher.
        /// </summary>
        /// <param name="name">Name of the MsSqlWatcher.</param>
        /// <param name="configuration">Configuration of MsSqlWatcher.</param>
        /// <returns>Instance of MsSqlWatcher.</returns>
        public static MsSqlWatcher Create(string name, MsSqlWatcherConfiguration configuration)
            => new MsSqlWatcher(name, configuration);
    }
}