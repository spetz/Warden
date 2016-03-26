using System;
using System.Collections.Generic;
using System.Data;
using Machine.Specifications;
using Moq;
using Sentry.Watchers.MsSql;
using It = Machine.Specifications.It;

namespace Sentry.Tests.Unit.Watchers.MsSql
{
    public class MsSqlWatcher_specs
    {
        protected static string ConnectionString =
            @"Data Source=.\sqlexpress;Initial Catalog=MyDatabase;Integrated Security=True";

        protected static MsSqlWatcher Watcher { get; set; }
        protected static MsSqlWatcherConfiguration Configuration { get; set; }
        protected static IWatcherCheckResult CheckResult { get; set; }
        protected static Exception Exception { get; set; }
    }

    [Subject("MSSQL watcher initialization")]
    public class when_initializing_without_configuration : MsSqlWatcher_specs
    {
        Establish context = () => Configuration = null;

        Because of = () => Exception = Catch.Exception((Action) (() => Watcher = MsSqlWatcher.Create("test", Configuration)));

        It should_fail = () => Exception.ShouldBeOfExactType<ArgumentNullException>();
        It should_have_a_specific_reason = () => Exception.Message.ShouldContain("MSSQL Watcher configuration has not been provided.");
    }

    [Subject("MSSQL watcher execution")]
    public class when_invoking_execute_async_method_without_query : MsSqlWatcher_specs
    {
        static Mock<IMsSql> MsSqlServicerMock;
        static Mock<IDbConnection> DbConnectionMock;

        Establish context = () =>
        {
            MsSqlServicerMock = new Mock<IMsSql>();
            DbConnectionMock = new Mock<IDbConnection>();
            Configuration = MsSqlWatcherConfiguration
                .Create(ConnectionString)
                .WithConnectionProvider((connectionString) => DbConnectionMock.Object)
                .WithMsSqlServiceProvider(() => MsSqlServicerMock.Object)
                .Build();
            Watcher = MsSqlWatcher.Create("MSSQL watcher", Configuration);
        };

        Because of = async () => await Watcher.ExecuteAsync().Await().AsTask;

        It should_invoke_open_method_only_once = () => DbConnectionMock.Verify( x => x.Open(), Times.Once);
    }

    [Subject("MSSQL watcher execution")]
    public class when_invoking_execute_async_method_with_query : MsSqlWatcher_specs
    {
        static Mock<IMsSql> MsSqlServicerMock;
        static Mock<IDbConnection> DbConnectionMock;

        Establish context = () =>
        {
            MsSqlServicerMock = new Mock<IMsSql>();
            DbConnectionMock = new Mock<IDbConnection>();
            Configuration = MsSqlWatcherConfiguration
                .Create(ConnectionString)
                .WithQuery("select * from users")
                .WithConnectionProvider((connectionString) => DbConnectionMock.Object)
                .WithMsSqlServiceProvider(() => MsSqlServicerMock.Object)
                .Build();
            Watcher = MsSqlWatcher.Create("MSSQL watcher", Configuration);
        };

        Because of = async () => await Watcher.ExecuteAsync().Await().AsTask;

        It should_invoke_open_method_only_once = () => DbConnectionMock.Verify(x => x.Open(), Times.Once);
        It should_invoke_query_async_method_only_once = () => MsSqlServicerMock.Verify(
                x => x.QueryAsync(Moq.It.IsAny<IDbConnection>(), Moq.It.IsAny<string>(),
                Moq.It.IsAny<IDictionary<string, object>>(), Moq.It.IsAny<TimeSpan?>()), Times.Once);
    }
}