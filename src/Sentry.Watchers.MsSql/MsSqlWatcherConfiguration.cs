using System;
using System.Collections.Generic;

namespace Sentry.Watchers.MsSql
{
    public class MsSqlWatcherConfiguration
    {
        public string Name { get; protected set; }
        public string ConnectionString { get; protected set; }
        public string Query { get; protected set; }
        public IDictionary<string, object> QueryParameters { get; protected set; }
        public Func<IEnumerable<dynamic>, bool> EnsureThat { get; protected set; }

        protected internal MsSqlWatcherConfiguration()
        {
        }

        public static MsSqlWatcherConfiguration Empty => new MsSqlWatcherConfiguration();

        public static MsSqlWatcherConfigurationBuilder Create(string name) => new MsSqlWatcherConfigurationBuilder(name);

        public class MsSqlWatcherConfigurationBuilder
        {
            private readonly MsSqlWatcherConfiguration _configuration = new MsSqlWatcherConfiguration();

            public MsSqlWatcherConfigurationBuilder(string name)
            {
                if (string.IsNullOrEmpty(name))
                    throw new ArgumentException("Watcher name can not be empty.", nameof(name));

                _configuration.Name = name;
            }

            public MsSqlWatcherConfigurationBuilder WithConnectionString(string connectionString)
            {
                if (string.IsNullOrEmpty(connectionString))
                    throw new ArgumentException("Connection string can not be empty.", nameof(connectionString));

                _configuration.ConnectionString = connectionString;

                return this;
            }

            public MsSqlWatcherConfigurationBuilder WithQuery(string query, IDictionary<string, object> parameters)
            {
                if (string.IsNullOrEmpty(query))
                    throw new ArgumentException("SQL query can not be empty.", nameof(query));

                _configuration.Query = query;
                _configuration.QueryParameters = parameters;

                return this;
            }

            public MsSqlWatcherConfigurationBuilder EnsureThat(Func<IEnumerable<dynamic>, bool> ensureThat)
            {
                if (ensureThat == null)
                    throw new ArgumentException("Ensure that predicate can not be null.", nameof(ensureThat));

                _configuration.EnsureThat = results => ensureThat(results);

                return this;
            }

            public MsSqlWatcherConfiguration Build() => _configuration;
        }
    }
}