using System;
using System.Linq;
using Machine.Specifications;
using Moq;
using Sentry.Watchers.MongoDb;
using It = Machine.Specifications.It;

namespace Sentry.Tests.Watchers.MongoDb
{
    public class MongoDbWatcher_specs
    {
        protected static string ConnectionString = "mongodb://localhost:27017";
        protected static string Database = "TestDb";
        protected static MongoDbWatcher Watcher { get; set; }
        protected static MongoDbWatcherConfiguration Configuration { get; set; }
        protected static IWatcherCheckResult CheckResult { get; set; }
        protected static Exception Exception { get; set; }
    }

    [Subject("MongoDB watcher initialization")]
    public class when_initializing_without_configuration : MongoDbWatcher_specs
    {
        Establish context = () => Configuration = null;

        Because of = () => Exception = Catch.Exception((Action) (() => Watcher = MongoDbWatcher.Create("test", Configuration)));

        It should_fail = () => Exception.ShouldBeOfExactType<ArgumentNullException>();
        It should_have_a_specific_reason = () => Exception.Message.ShouldContain("MongoDB Watcher configuration has not been provided.");
    }

    [Subject("MongoDB watcher execution")]
    public class when_invoking_execute_async_with_configuration : MongoDbWatcher_specs
    {
        static Mock<IMongoDbConnection> MongoDbConnectionMock;

        Establish context = () =>
        {
            MongoDbConnectionMock = new Mock<IMongoDbConnection>();
            Configuration = MongoDbWatcherConfiguration
                .Create(Database, ConnectionString)
                .WithConnectionProvider(connectionString => MongoDbConnectionMock.Object)
                .Build();
            Watcher = MongoDbWatcher.Create("MongoDB watcher", Configuration);
        };

        Because of = async () => await Watcher.ExecuteAsync().Await().AsTask;

        It should_invoke_get_database_async_method_only_once =
            () => MongoDbConnectionMock.Verify(x => x.GetDatabaseAsync(), Times.Once);
    }

    [Subject("MongoDB watcher execution")]
    public class when_invoking_execute_async_with_defined_query : MongoDbWatcher_specs
    {
        static Mock<IMongoDbConnection> MongoDbConnectionMock;
        static Mock<IMongoDb> MongoDbMock;

        Establish context = () =>
        {
            MongoDbConnectionMock = new Mock<IMongoDbConnection>();
            MongoDbMock = new Mock<IMongoDb>();
            MongoDbMock.Setup(x =>
                x.QueryAsync(Moq.It.IsAny<IMongoDbConnection>(), Moq.It.IsAny<string>(), Moq.It.IsAny<string>()))
                .ReturnsAsync(Enumerable.Empty<dynamic>());
            MongoDbConnectionMock.Setup(x => x.GetDatabaseAsync()).ReturnsAsync(MongoDbMock.Object);

            Configuration = MongoDbWatcherConfiguration
                .Create(Database, ConnectionString)
                .WithQuery("Users", "{\"name\": \"admin\"}")
                .WithConnectionProvider(connectionString => MongoDbConnectionMock.Object)
                .Build();
            Watcher = MongoDbWatcher.Create("MongoDB watcher", Configuration);
        };

        Because of = async () => await Watcher.ExecuteAsync().Await().AsTask;

        It should_invoke_get_database_async_method_only_once =
            () => MongoDbConnectionMock.Verify(x => x.GetDatabaseAsync(), Times.Once);

        It should_invoke_query_async_method_only_once =
            () => MongoDbMock.Verify(x => x.QueryAsync(Moq.It.IsAny<IMongoDbConnection>(),
                Moq.It.IsAny<string>(), Moq.It.IsAny<string>()), Times.Once);
    }
}