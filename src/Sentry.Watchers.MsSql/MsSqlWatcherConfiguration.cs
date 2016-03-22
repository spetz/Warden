using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Sentry.Core;

namespace Sentry.Watchers.MsSql
{
    public class MsSqlWatcherConfiguration
    {
        public string ConnectionString { get; protected set; }
        public string Query { get; protected set; }
        public IDictionary<string, object> QueryParameters { get; protected set; }
        public Func<IEnumerable<dynamic>, bool> EnsureThat { get; protected set; }
        public Func<IEnumerable<dynamic>, Task<bool>> EnsureThatAsync { get; protected set; }
        public TimeSpan Timeout { get; protected set; }

        protected internal MsSqlWatcherConfiguration(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentException("Connection string can not be empty.", nameof(connectionString));

            ConnectionString = connectionString;
        }

        public static Builder Create(string connectionString) => new Builder(connectionString);

        public abstract class Configurator<T> : WatcherConfigurator<T,MsSqlWatcherConfiguration> where T : Configurator<T>
        {
            protected Configurator(string connectionString)
            {
                Configuration = new MsSqlWatcherConfiguration(connectionString);
            }

            protected Configurator(MsSqlWatcherConfiguration configuration) : base(configuration)
            {
            }

            public T WithQuery(string query, IDictionary<string, object> parameters)
            {
                if (string.IsNullOrEmpty(query))
                    throw new ArgumentException("SQL query can not be empty.", nameof(query));

                Configuration.Query = query;
                Configuration.QueryParameters = parameters;

                return Configurator;
            }

            public T WithTimeout(TimeSpan timeout)
            {
                if (timeout == null)
                    throw new ArgumentNullException(nameof(timeout), "Timeout can not be null.");

                if (timeout == TimeSpan.Zero)
                    throw new ArgumentException("Timeout can not be equal to zero.", nameof(timeout));

                Configuration.Timeout = timeout;

                return Configurator;
            }

            public T EnsureThat(Func<IEnumerable<dynamic>, bool> ensureThat)
            {
                if (ensureThat == null)
                    throw new ArgumentException("Ensure that predicate can not be null.", nameof(ensureThat));

                Configuration.EnsureThat = results => ensureThat(results);

                return Configurator;
            }

            public T EnsureThatAsync(Func<IEnumerable<dynamic>, Task<bool>> ensureThat)
            {
                if (ensureThat == null)
                    throw new ArgumentException("Ensure that async predicate can not be null.", nameof(ensureThat));

                Configuration.EnsureThatAsync = ensureThat;

                return Configurator;
            }
        }

        public class Default : Configurator<Default>
        {
            public Default(MsSqlWatcherConfiguration configuration) : base(configuration)
            {
                SetInstance(this);
            }
        }

        public class Builder : Configurator<Builder>
        {
            public Builder(string connectionString) : base(connectionString)
            {
                SetInstance(this);
            }

            public MsSqlWatcherConfiguration Build() => Configuration;

            public static explicit operator Default(Builder builder) => new Default(builder.Configuration);
        }
    }
}