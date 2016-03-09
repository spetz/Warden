using System;
using System.Collections.Generic;

namespace Sentry.Watchers.MsSql
{
    public class MsSqlWatcherConfiguration
    {
        public string ConnectionString { get; protected set; }
        public string Query { get; protected set; }
        public IDictionary<string, object> QueryParameters { get; protected set; }
        public Func<IEnumerable<dynamic>, bool> EnsureThat { get; protected set; }

        protected internal MsSqlWatcherConfiguration(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentException("Connection string can not be empty.", nameof(connectionString));

            ConnectionString = connectionString;
        }

        public static Builder Create(string connectionString) => new Builder(connectionString);

        public class Builder
        {
            private readonly MsSqlWatcherConfiguration _configuration;

            public Builder(string connectionString)
            {
                _configuration = new MsSqlWatcherConfiguration(connectionString);
            }

            public Builder WithQuery(string query, IDictionary<string, object> parameters)
            {
                if (string.IsNullOrEmpty(query))
                    throw new ArgumentException("SQL query can not be empty.", nameof(query));

                _configuration.Query = query;
                _configuration.QueryParameters = parameters;

                return this;
            }

            public Builder EnsureThat(Func<IEnumerable<dynamic>, bool> ensureThat)
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