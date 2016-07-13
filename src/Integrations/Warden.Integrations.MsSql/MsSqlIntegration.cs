using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Warden.Watchers;

namespace Warden.Integrations.MsSql
{
    /// <summary>
    /// Integration with the MS SQL.
    /// </summary>
    public class MsSqlIntegration : IIntegration
    {
        private readonly IMsSqlService _msSqlService;
        private readonly MsSqlIntegrationConfiguration _configuration;

        public MsSqlIntegration(MsSqlIntegrationConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration),
                    "MS SQL integration configuration has not been provided.");
            }

            _configuration = configuration;
            _msSqlService = _configuration.MsSqlServiceProvider();
        }


        /// <summary>
        /// Executes the SQL query and returns a collection of the strongly typed results.
        /// </summary>
        /// <param name="query">SQL query.</param>
        /// <param name="parameters">SQL query parameters.</param>
        /// <returns>Collection of the strongly typed results.</returns>
        public async Task<IEnumerable<T>> QueryAsync<T>(string query, IDictionary<string, object> parameters = null)
        {
            try
            {
                using (var connection = _configuration.ConnectionProvider(_configuration.ConnectionString))
                {
                    connection.Open();
                    var queryToExecute = string.IsNullOrWhiteSpace(query) ? _configuration.Query : query;
                    var queryParameters = parameters ?? _configuration.QueryParameters;

                    return await _msSqlService.QueryAsync<T>(connection, queryToExecute,
                        queryParameters, _configuration.QueryTimeout);
                }
            }
            catch (SqlException exception)
            {
                throw new IntegrationException("There was a SQL error while trying to execute the query.", exception);
            }
            catch (Exception exception)
            {
                throw new IntegrationException("There was an error while trying to access the MS SQL database.",
                    exception);
            }
        }

        /// <summary>
        /// Executes the SQL command and returns a scalar representing the number of affected rows.
        /// </summary>
        /// <param name="command">SQL command.</param>
        /// <param name="parameters">SQL command parameters.</param>
        /// <returns>Scalar representing the number of affected rows.</returns>
        public async Task<int> ExecuteAsync(string command, IDictionary<string, object> parameters = null)
        {
            try
            {
                using (var connection = _configuration.ConnectionProvider(_configuration.ConnectionString))
                {
                    connection.Open();
                    var commandToExecute = string.IsNullOrWhiteSpace(command) ? _configuration.Command : command;
                    var commandParameters = parameters ?? _configuration.CommandParameters;

                    return await _msSqlService.ExecuteAsync(connection, commandToExecute,
                        commandParameters, _configuration.CommandTimeout);
                }
            }
            catch (SqlException exception)
            {
                throw new IntegrationException("There was a SQL error while trying to execute the command.", exception);
            }
            catch (Exception exception)
            {
                throw new IntegrationException("There was an error while trying to access the MS SQL database.",
                    exception);
            }
        }

        /// <summary>
        /// Inserts the IWardenIteration into the MS SQL database using tables based on the provided schema file.
        /// </summary>
        /// <param name="iteration">Iteration object that will be saved into the MS SQL database.</param>
        /// <returns></returns>
        public async Task SaveIterationAsync(IWardenIteration iteration)
        {
            var wardenIterationCommand = "insert into WardenIterations values" +
                                         "(@wardenName, @ordinal, @startedAt, @completedAt, @executionTime, @isValid);" +
                                         "select cast(scope_identity() as bigint)";
            var wardenIterationParameters = new Dictionary<string, object>
            {
                ["wardenName"] = iteration.WardenName,
                ["ordinal"] = iteration.Ordinal,
                ["startedAt"] = iteration.StartedAt,
                ["completedAt"] = iteration.CompletedAt,
                ["executionTime"] = iteration.ExecutionTime,
                ["isValid"] = iteration.IsValid,
            };
            var iterationResultIds = await QueryAsync<long>(wardenIterationCommand, wardenIterationParameters);
            var iterationId = iterationResultIds.FirstOrDefault();
            if(iterationId <= 0)
                return;
            await SaveWardenCheckResultsAsync(iteration.Results, iterationId);
        }

        private async Task SaveWardenCheckResultsAsync(IEnumerable<IWardenCheckResult> results, long iterationId)
        {
            foreach (var result in results)
            {
                var wardenCheckResultCommand = "insert into WardenCheckResults values (@wardenIteration_Id, @isValid, " +
                                               "@startedAt, @completedAt, @executionTime);select cast(scope_identity() as bigint)";
                var wardenCheckResultParameters = new Dictionary<string, object>
                {
                    ["wardenIteration_Id"] = iterationId,
                    ["isValid"] = result.IsValid,
                    ["startedAt"] = result.StartedAt,
                    ["completedAt"] = result.CompletedAt,
                    ["executionTime"] = result.ExecutionTime
                };
                var wardenCheckResultIds = await QueryAsync<long>(wardenCheckResultCommand, wardenCheckResultParameters);
                var wardenCheckResultId = wardenCheckResultIds.FirstOrDefault();
                if(wardenCheckResultId <= 0)
                    return;
                await SaveWatcherCheckResultAsync(result.WatcherCheckResult, wardenCheckResultId);
                await SaveExceptionAsync(result.Exception, wardenCheckResultId);
            }
        }

        private async Task SaveWatcherCheckResultAsync(IWatcherCheckResult result, long wardenCheckResultId)
        {
            if (result == null)
                return;

            var watcherCheckResultCommand = "insert into WatcherCheckResults values " +
                                            "(@wardenCheckResult_Id, @watcherName, @watcherType, @description, @isValid)";
            var watcherCheckResultParameters = new Dictionary<string, object>
            {
                ["wardenCheckResult_Id"] = wardenCheckResultId,
                ["watcherName"] = result.WatcherName,
                ["watcherType"] = result.WatcherType.ToString().Split('.').Last(),
                ["description"] = result.Description,
                ["isValid"] = result.IsValid
            };
            await ExecuteAsync(watcherCheckResultCommand, watcherCheckResultParameters);
        }

        private async Task<long?> SaveExceptionAsync(Exception exception, long wardenCheckResultId, long? parentExceptionId = null)
        {
            if (exception == null)
                return null;

            var exceptionCommand = "insert into Exceptions values (@wardenCheckResult_Id, @parentException_Id, " +
                     "@message, @source, @stackTrace);select cast(scope_identity() as bigint)";
            var exceptionParameters = new Dictionary<string, object>
            {
                ["wardenCheckResult_Id"] = wardenCheckResultId,
                ["parentException_Id"] = parentExceptionId,
                ["message"] = exception.Message,
                ["source"] = exception.Source,
                ["stackTrace"] = exception.StackTrace
            };
            var exceptionIds = await QueryAsync<long>(exceptionCommand, exceptionParameters);
            var exceptionId = exceptionIds.FirstOrDefault();
            if (exceptionId <= 0)
                return null;
            if (exception.InnerException == null)
                return exceptionId;

            await SaveExceptionAsync(exception.InnerException, wardenCheckResultId, exceptionId);

            return exceptionId;
        }

        /// <summary>
        /// Factory method for creating a new instance of MsSqlIntegration.
        /// </summary>
        /// <param name="connectionString">Connection string of the MS SQL server.</param>
        /// <param name="configurator">Lambda expression for configuring MS SQL integration.</param>
        /// <returns>Instance of MsSqlIntegration.</returns>
        public static MsSqlIntegration Create(string connectionString,
            Action<MsSqlIntegrationConfiguration.Builder> configurator)
        {
            var config = new MsSqlIntegrationConfiguration.Builder(connectionString);
            configurator?.Invoke(config);

            return Create(config.Build());
        }

        /// <summary>
        /// Factory method for creating a new instance of MsSqlIntegration.
        /// </summary>
        /// <param name="configuration">Configuration of MS SQL integration.</param>
        /// <returns>Instance of MsSqlIntegration.</returns>
        public static MsSqlIntegration Create(MsSqlIntegrationConfiguration configuration)
            => new MsSqlIntegration(configuration);
    }
}