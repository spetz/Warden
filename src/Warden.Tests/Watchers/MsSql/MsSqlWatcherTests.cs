using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using Warden.Core;
using Warden.Watchers.MsSql;
using It = Machine.Specifications.It;

namespace Warden.Tests.Watchers.MsSql
{
    public class MsSqlWatcher_specs
    {
        protected static string ConnectionString =
            @"Data Source=.\sqlexpress;Initial Catalog=MyDatabase;Integrated Security=True";

        protected static MsSqlWatcher Watcher { get; set; }
        protected static MsSqlWatcherConfiguration Configuration { get; set; }
        protected static IWatcherCheckResult CheckResult { get; set; }
        protected static MsSqlWatcherCheckResult MsSqlCheckResult { get; set; }
        protected static Exception Exception { get; set; }

        protected static IEnumerable<dynamic> QueryResult = new List<dynamic>
        {
            new {id = 1, name = "admin", role = "admin"},
            new {id = 2, name = "user", role = "user"}
        };
    }

    [Subject("MSSQL watcher initialization")]
    public class when_initializing_without_configuration : MsSqlWatcher_specs
    {
        Establish context = () => Configuration = null;

        Because of = () => Exception = Catch.Exception((() => Watcher = MsSqlWatcher.Create("test", Configuration)));

        It should_fail = () => Exception.ShouldBeOfExactType<ArgumentNullException>();

        It should_have_a_specific_reason =
            () => Exception.Message.ShouldContain("MSSQL Watcher configuration has not been provided.");
    }

    [Subject("MSSQL watcher execution")]
    public class when_invoking_execute_async_without_query : MsSqlWatcher_specs
    {
        static Mock<IMsSql> MsSqlMock;
        static Mock<IDbConnection> DbConnectionMock;

        Establish context = () =>
        {
            MsSqlMock = new Mock<IMsSql>();
            DbConnectionMock = new Mock<IDbConnection>();
            Configuration = MsSqlWatcherConfiguration
                .Create(ConnectionString)
                .WithConnectionProvider(connectionString => DbConnectionMock.Object)
                .WithMsSqlProvider(() => MsSqlMock.Object)
                .Build();
            Watcher = MsSqlWatcher.Create("MSSQL watcher", Configuration);
        };

        Because of = async () =>
        {
            CheckResult = await Watcher.ExecuteAsync().Await().AsTask;
            MsSqlCheckResult = CheckResult as MsSqlWatcherCheckResult;
        };

        It should_invoke_open_method_only_once = () => DbConnectionMock.Verify(x => x.Open(), Times.Once);

        It should_not_invoke_query_async_method = () => MsSqlMock.Verify(
            x => x.QueryAsync(Moq.It.IsAny<IDbConnection>(), Moq.It.IsAny<string>(),
                Moq.It.IsAny<IDictionary<string, object>>(), Moq.It.IsAny<TimeSpan?>()), Times.Never);

        It should_have_valid_check_result = () => CheckResult.IsValid.ShouldBeTrue();
        It should_have_check_result_of_type_mssql = () => MsSqlCheckResult.ShouldNotBeNull();

        It should_have_set_values_in_mssl_check_result = () =>
        {
            MsSqlCheckResult.WatcherName.ShouldNotBeEmpty();
            MsSqlCheckResult.WatcherType.ShouldNotBeNull();
            MsSqlCheckResult.ConnectionString.ShouldNotBeEmpty();
            MsSqlCheckResult.Query.ShouldBeEmpty();
            MsSqlCheckResult.QueryResult.ShouldBeEmpty();
        };
    }

    [Subject("MSSQL watcher execution")]
    public class when_invoking_execute_async_with_query : MsSqlWatcher_specs
    {
        static Mock<IMsSql> MsSqlMock;
        static Mock<IDbConnection> DbConnectionMock;

        Establish context = () =>
        {
            MsSqlMock = new Mock<IMsSql>();
            DbConnectionMock = new Mock<IDbConnection>();
            MsSqlMock.Setup(x => x.QueryAsync(Moq.It.IsAny<IDbConnection>(), Moq.It.IsAny<string>(),
                Moq.It.IsAny<IDictionary<string, object>>(), Moq.It.IsAny<TimeSpan?>()))
                .ReturnsAsync(QueryResult);
            Configuration = MsSqlWatcherConfiguration
                .Create(ConnectionString)
                .WithQuery("select * from users")
                .WithConnectionProvider(connectionString => DbConnectionMock.Object)
                .WithMsSqlProvider(() => MsSqlMock.Object)
                .Build();
            Watcher = MsSqlWatcher.Create("MSSQL watcher", Configuration);
        };

        Because of = async () =>
        {
            CheckResult = await Watcher.ExecuteAsync().Await().AsTask;
            MsSqlCheckResult = CheckResult as MsSqlWatcherCheckResult;
        };

        It should_invoke_open_method_only_once = () => DbConnectionMock.Verify(x => x.Open(), Times.Once);

        It should_invoke_query_async_method_only_once = () => MsSqlMock.Verify(
            x => x.QueryAsync(Moq.It.IsAny<IDbConnection>(), Moq.It.IsAny<string>(),
                Moq.It.IsAny<IDictionary<string, object>>(), Moq.It.IsAny<TimeSpan?>()), Times.Once);

        It should_have_valid_check_result = () => CheckResult.IsValid.ShouldBeTrue();
        It should_have_check_result_of_type_mssql = () => MsSqlCheckResult.ShouldNotBeNull();

        It should_have_set_values_in_mssl_check_result = () =>
        {
            MsSqlCheckResult.WatcherName.ShouldNotBeEmpty();
            MsSqlCheckResult.WatcherType.ShouldNotBeNull();
            MsSqlCheckResult.ConnectionString.ShouldNotBeEmpty();
            MsSqlCheckResult.Query.ShouldNotBeEmpty();
            MsSqlCheckResult.QueryResult.ShouldNotBeEmpty();
        };
    }

    [Subject("MSSQL watcher execution")]
    public class when_invoking_open_connection_that_fails : MsSqlWatcher_specs
    {
        static Mock<IMsSql> MsSqlMock;
        static Mock<IDbConnection> DbConnectionMock;

        Establish context = () =>
        {
            MsSqlMock = new Mock<IMsSql>();
            DbConnectionMock = new Mock<IDbConnection>();
            DbConnectionMock.Setup(x => x.Open()).Throws(new Exception("Error"));
            Configuration = MsSqlWatcherConfiguration
                .Create(ConnectionString)
                .WithQuery("select * from users")
                .WithConnectionProvider(connectionString => DbConnectionMock.Object)
                .WithMsSqlProvider(() => MsSqlMock.Object)
                .Build();
            Watcher = MsSqlWatcher.Create("MSSQL watcher", Configuration);
        };

        Because of = () => Exception = Catch.Exception(() => Watcher.ExecuteAsync().Await());

        It should_invoke_open_method_only_once = () => DbConnectionMock.Verify(x => x.Open(), Times.Once);
        It should_fail = () => Exception.ShouldBeOfExactType<WatcherException>();
    }

    [Subject("MSSQL watcher execution")]
    public class when_invoking_query_async_that_fails : MsSqlWatcher_specs
    {
        static Mock<IMsSql> MsSqlMock;
        static Mock<IDbConnection> DbConnectionMock;

        Establish context = () =>
        {
            MsSqlMock = new Mock<IMsSql>();
            DbConnectionMock = new Mock<IDbConnection>();
            MsSqlMock.Setup(x => x.QueryAsync(Moq.It.IsAny<IDbConnection>(), Moq.It.IsAny<string>(),
                Moq.It.IsAny<IDictionary<string, object>>(), Moq.It.IsAny<TimeSpan?>()))
                .ThrowsAsync(new Exception("Error"));
            Configuration = MsSqlWatcherConfiguration
                .Create(ConnectionString)
                .WithQuery("select * from users")
                .WithConnectionProvider(connectionString => DbConnectionMock.Object)
                .WithMsSqlProvider(() => MsSqlMock.Object)
                .Build();
            Watcher = MsSqlWatcher.Create("MSSQL watcher", Configuration);
        };

        Because of = () => Exception = Catch.Exception(() => Watcher.ExecuteAsync().Await());

        It should_invoke_open_method_only_once = () => DbConnectionMock.Verify(x => x.Open(), Times.Once);
        It should_fail = () => Exception.ShouldBeOfExactType<WatcherException>();

        It should_have_set_values_in_mssl_check_result = () =>
        {
            MsSqlCheckResult.WatcherName.ShouldNotBeEmpty();
            MsSqlCheckResult.WatcherType.ShouldNotBeNull();
            MsSqlCheckResult.ConnectionString.ShouldNotBeEmpty();
            MsSqlCheckResult.Query.ShouldNotBeEmpty();
            MsSqlCheckResult.QueryResult.ShouldNotBeEmpty();
        };
    }

    [Subject("MSSQL watcher execution")]
    public class when_invoking_ensure_predicate_that_is_valid : MsSqlWatcher_specs
    {
        static Mock<IMsSql> MsSqlMock;
        static Mock<IDbConnection> DbConnectionMock;

        Establish context = () =>
        {
            MsSqlMock = new Mock<IMsSql>();
            DbConnectionMock = new Mock<IDbConnection>();
            MsSqlMock.Setup(x => x.QueryAsync(Moq.It.IsAny<IDbConnection>(), Moq.It.IsAny<string>(),
                Moq.It.IsAny<IDictionary<string, object>>(), Moq.It.IsAny<TimeSpan?>()))
                .ReturnsAsync(QueryResult);
            Configuration = MsSqlWatcherConfiguration
                .Create(ConnectionString)
                .WithQuery("select * from users")
                .EnsureThat(users => users.Any(user => user.id == 1))
                .WithConnectionProvider(connectionString => DbConnectionMock.Object)
                .WithMsSqlProvider(() => MsSqlMock.Object)
                .Build();
            Watcher = MsSqlWatcher.Create("MSSQL watcher", Configuration);
        };

        Because of = async () =>
        {
            CheckResult = await Watcher.ExecuteAsync().Await().AsTask;
            MsSqlCheckResult = CheckResult as MsSqlWatcherCheckResult;
        };


        It should_invoke_open_method_only_once = () => DbConnectionMock.Verify(x => x.Open(), Times.Once);

        It should_invoke_query_async_method_only_once = () => MsSqlMock.Verify(
            x => x.QueryAsync(Moq.It.IsAny<IDbConnection>(), Moq.It.IsAny<string>(),
                Moq.It.IsAny<IDictionary<string, object>>(), Moq.It.IsAny<TimeSpan?>()), Times.Once);

        It should_have_valid_check_result = () => CheckResult.IsValid.ShouldBeTrue();
        It should_have_check_result_of_type_mssql = () => MsSqlCheckResult.ShouldNotBeNull();

        It should_have_set_values_in_mssl_check_result = () =>
        {
            MsSqlCheckResult.WatcherName.ShouldNotBeEmpty();
            MsSqlCheckResult.WatcherType.ShouldNotBeNull();
            MsSqlCheckResult.ConnectionString.ShouldNotBeEmpty();
            MsSqlCheckResult.Query.ShouldNotBeEmpty();
            MsSqlCheckResult.QueryResult.ShouldNotBeEmpty();
        };
    }

    [Subject("MSSQL watcher execution")]
    public class when_invoking_ensure_async_predicate_that_is_valid : MsSqlWatcher_specs
    {
        static Mock<IMsSql> MsSqlMock;
        static Mock<IDbConnection> DbConnectionMock;

        Establish context = () =>
        {
            MsSqlMock = new Mock<IMsSql>();
            DbConnectionMock = new Mock<IDbConnection>();
            MsSqlMock.Setup(x => x.QueryAsync(Moq.It.IsAny<IDbConnection>(), Moq.It.IsAny<string>(),
                Moq.It.IsAny<IDictionary<string, object>>(), Moq.It.IsAny<TimeSpan?>()))
                .ReturnsAsync(QueryResult);
            Configuration = MsSqlWatcherConfiguration
                .Create(ConnectionString)
                .WithQuery("select * from users")
                .EnsureThatAsync(users => Task.Factory.StartNew(() => users.Any(user => user.id == 1)))
                .WithConnectionProvider(connectionString => DbConnectionMock.Object)
                .WithMsSqlProvider(() => MsSqlMock.Object)
                .Build();
            Watcher = MsSqlWatcher.Create("MSSQL watcher", Configuration);
        };

        Because of = async () =>
        {
            CheckResult = await Watcher.ExecuteAsync().Await().AsTask;
            MsSqlCheckResult = CheckResult as MsSqlWatcherCheckResult;
        };


        It should_invoke_open_method_only_once = () => DbConnectionMock.Verify(x => x.Open(), Times.Once);

        It should_invoke_query_async_method_only_once = () => MsSqlMock.Verify(
            x => x.QueryAsync(Moq.It.IsAny<IDbConnection>(), Moq.It.IsAny<string>(),
                Moq.It.IsAny<IDictionary<string, object>>(), Moq.It.IsAny<TimeSpan?>()), Times.Once);

        It should_have_valid_check_result = () => CheckResult.IsValid.ShouldBeTrue();
        It should_have_check_result_of_type_mssql = () => MsSqlCheckResult.ShouldNotBeNull();

        It should_have_set_values_in_mssl_check_result = () =>
        {
            MsSqlCheckResult.WatcherName.ShouldNotBeEmpty();
            MsSqlCheckResult.WatcherType.ShouldNotBeNull();
            MsSqlCheckResult.ConnectionString.ShouldNotBeEmpty();
            MsSqlCheckResult.Query.ShouldNotBeEmpty();
            MsSqlCheckResult.QueryResult.ShouldNotBeEmpty();
        };
    }


    [Subject("MSSQL watcher execution")]
    public class when_invoking_ensure_predicate_that_is_invalid : MsSqlWatcher_specs
    {
        static Mock<IMsSql> MsSqlMock;
        static Mock<IDbConnection> DbConnectionMock;

        Establish context = () =>
        {
            MsSqlMock = new Mock<IMsSql>();
            DbConnectionMock = new Mock<IDbConnection>();
            MsSqlMock.Setup(x => x.QueryAsync(Moq.It.IsAny<IDbConnection>(), Moq.It.IsAny<string>(),
                Moq.It.IsAny<IDictionary<string, object>>(), Moq.It.IsAny<TimeSpan?>()))
                .ReturnsAsync(QueryResult);
            Configuration = MsSqlWatcherConfiguration
                .Create(ConnectionString)
                .WithQuery("select * from users")
                .EnsureThat(users => users.Any(user => user.id == 100))
                .WithConnectionProvider(connectionString => DbConnectionMock.Object)
                .WithMsSqlProvider(() => MsSqlMock.Object)
                .Build();
            Watcher = MsSqlWatcher.Create("MSSQL watcher", Configuration);
        };

        Because of = async () =>
        {
            CheckResult = await Watcher.ExecuteAsync().Await().AsTask;
            MsSqlCheckResult = CheckResult as MsSqlWatcherCheckResult;
        };


        It should_invoke_open_method_only_once = () => DbConnectionMock.Verify(x => x.Open(), Times.Once);

        It should_invoke_query_async_method_only_once = () => MsSqlMock.Verify(
            x => x.QueryAsync(Moq.It.IsAny<IDbConnection>(), Moq.It.IsAny<string>(),
                Moq.It.IsAny<IDictionary<string, object>>(), Moq.It.IsAny<TimeSpan?>()), Times.Once);

        It should_have_invvalid_check_result = () => CheckResult.IsValid.ShouldBeFalse();
        It should_have_check_result_of_type_mssql = () => MsSqlCheckResult.ShouldNotBeNull();

        It should_have_set_values_in_mssl_check_result = () =>
        {
            MsSqlCheckResult.WatcherName.ShouldNotBeEmpty();
            MsSqlCheckResult.WatcherType.ShouldNotBeNull();
            MsSqlCheckResult.ConnectionString.ShouldNotBeEmpty();
            MsSqlCheckResult.Query.ShouldNotBeEmpty();
            MsSqlCheckResult.QueryResult.ShouldNotBeEmpty();
        };
    }

    [Subject("MSSQL watcher execution")]
    public class when_invoking_ensure_async_predicate_that_is_invalid : MsSqlWatcher_specs
    {
        static Mock<IMsSql> MsSqlMock;
        static Mock<IDbConnection> DbConnectionMock;

        Establish context = () =>
        {
            MsSqlMock = new Mock<IMsSql>();
            DbConnectionMock = new Mock<IDbConnection>();
            MsSqlMock.Setup(x => x.QueryAsync(Moq.It.IsAny<IDbConnection>(), Moq.It.IsAny<string>(),
                Moq.It.IsAny<IDictionary<string, object>>(), Moq.It.IsAny<TimeSpan?>()))
                .ReturnsAsync(QueryResult);
            Configuration = MsSqlWatcherConfiguration
                .Create(ConnectionString)
                .WithQuery("select * from users")
                .EnsureThatAsync(users => Task.Factory.StartNew(() => users.Any(user => user.id == 100)))
                .WithConnectionProvider(connectionString => DbConnectionMock.Object)
                .WithMsSqlProvider(() => MsSqlMock.Object)
                .Build();
            Watcher = MsSqlWatcher.Create("MSSQL watcher", Configuration);
        };

        Because of = async () =>
        {
            CheckResult = await Watcher.ExecuteAsync().Await().AsTask;
            MsSqlCheckResult = CheckResult as MsSqlWatcherCheckResult;
        };


        It should_invoke_open_method_only_once = () => DbConnectionMock.Verify(x => x.Open(), Times.Once);

        It should_invoke_query_async_method_only_once = () => MsSqlMock.Verify(
            x => x.QueryAsync(Moq.It.IsAny<IDbConnection>(), Moq.It.IsAny<string>(),
                Moq.It.IsAny<IDictionary<string, object>>(), Moq.It.IsAny<TimeSpan?>()), Times.Once);

        It should_have_invalid_check_result = () => CheckResult.IsValid.ShouldBeFalse();
        It should_have_check_result_of_type_mssql = () => MsSqlCheckResult.ShouldNotBeNull();

        It should_have_set_values_in_mssl_check_result = () =>
        {
            MsSqlCheckResult.WatcherName.ShouldNotBeEmpty();
            MsSqlCheckResult.WatcherType.ShouldNotBeNull();
            MsSqlCheckResult.ConnectionString.ShouldNotBeEmpty();
            MsSqlCheckResult.Query.ShouldNotBeEmpty();
            MsSqlCheckResult.QueryResult.ShouldNotBeEmpty();
        };
    }
}