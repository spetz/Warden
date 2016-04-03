using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using Sentry.Watchers.Redis;
using It = Machine.Specifications.It;

namespace Sentry.Tests.Watchers.Redis
{
    public class RedisWatcher_specs
    {
        protected static string ConnectionString = "localhost";
        protected static int Database = 1;
        protected static RedisWatcher Watcher { get; set; }
        protected static RedisWatcherConfiguration Configuration { get; set; }
        protected static IWatcherCheckResult CheckResult { get; set; }
        protected static RedisWatcherCheckResult RedisCheckResult { get; set; }
        protected static Exception Exception { get; set; }

        protected static IEnumerable<dynamic> QueryResult = new List<dynamic>
        {
            "test-value"
        };
    }

    [Subject("Redis watcher initialization")]
    public class when_initializing_without_configuration : RedisWatcher_specs
    {
        Establish context = () => Configuration = null;

        Because of =() => Exception = Catch.Exception((() => Watcher = RedisWatcher.Create("test", Configuration)));

        It should_fail = () => Exception.ShouldBeOfExactType<ArgumentNullException>();

        It should_have_a_specific_reason = () => Exception.Message.ShouldContain("Redis Watcher configuration has not been provided.");
    }

    [Subject("Redis watcher execution")]
    public class when_invoking_execute_async_with_configuration : RedisWatcher_specs
    {
        static Mock<IRedisConnection> RedisConnectionMock;

        Establish context = () =>
        {
            RedisConnectionMock = new Mock<IRedisConnection>();
            Configuration = RedisWatcherConfiguration
                .Create(ConnectionString, Database)
                .WithConnectionProvider(connectionString => RedisConnectionMock.Object)
                .Build();
            Watcher = RedisWatcher.Create("Redis watcher", Configuration);
        };

        Because of = async () => await Watcher.ExecuteAsync().Await().AsTask;

        It should_invoke_get_database_async_method_only_once =
            () => RedisConnectionMock.Verify(x => x.GetDatabaseAsync(Moq.It.IsAny<int>()), Times.Once);
    }

    [Subject("Redis watcher execution")]
    public class when_invoking_execute_async_with_query : RedisWatcher_specs
    {
        static Mock<IRedisConnection> RedisConnectionMock;
        static Mock<IRedis> RedisMock;

        Establish context = () =>
        {
            RedisConnectionMock = new Mock<IRedisConnection>();
            RedisMock = new Mock<IRedis>();
            RedisMock.Setup(x => x.QueryAsync(Moq.It.IsAny<string>()))
                .ReturnsAsync(QueryResult);
            RedisConnectionMock.Setup(x => x.GetDatabaseAsync(Moq.It.IsAny<int>())).ReturnsAsync(RedisMock.Object);
            Configuration = RedisWatcherConfiguration
                .Create(ConnectionString, Database)
                .WithQuery("get test")
                .WithConnectionProvider(connectionString => RedisConnectionMock.Object)
                .Build();
            Watcher = RedisWatcher.Create("Redis watcher", Configuration);
        };

        Because of = async () =>
        {
            CheckResult = await Watcher.ExecuteAsync().Await().AsTask;
            RedisCheckResult = CheckResult as RedisWatcherCheckResult;
        };

        It should_invoke_get_database_async_method_only_once =
            () => RedisConnectionMock.Verify(x => x.GetDatabaseAsync(Moq.It.IsAny<int>()), Times.Once);

        It should_invoke_query_async_method_only_once =
            () => RedisMock.Verify(x => x.QueryAsync(Moq.It.IsAny<string>()), Times.Once);

        It should_have_valid_check_result = () => CheckResult.IsValid.ShouldBeTrue();
        It should_have_check_result_of_type_Redis = () => RedisCheckResult.ShouldNotBeNull();

        It should_have_set_values_in_Redis_check_result = () =>
        {
            RedisCheckResult.WatcherName.ShouldNotBeEmpty();
            RedisCheckResult.WatcherType.ShouldNotBeNull();
            RedisCheckResult.ConnectionString.ShouldNotBeEmpty();
            RedisCheckResult.Query.ShouldNotBeEmpty();
            RedisCheckResult.QueryResult.ShouldNotBeEmpty();
        };
    }

    [Subject("Redis watcher execution")]
    public class when_invoking_ensure_predicate_that_is_valid : RedisWatcher_specs
    {
        static Mock<IRedisConnection> RedisConnectionMock;
        static Mock<IRedis> RedisMock;

        Establish context = () =>
        {
            RedisConnectionMock = new Mock<IRedisConnection>();
            RedisMock = new Mock<IRedis>();
            RedisMock.Setup(x => x.QueryAsync(Moq.It.IsAny<string>()))
                .ReturnsAsync(QueryResult);
            RedisConnectionMock.Setup(x => x.GetDatabaseAsync(Moq.It.IsAny<int>())).ReturnsAsync(RedisMock.Object);
            Configuration = RedisWatcherConfiguration
                .Create(ConnectionString, Database)
                .WithQuery("get test")
                .EnsureThat(results => results.Any(x => x == "test-value"))
                .WithConnectionProvider(connectionString => RedisConnectionMock.Object)
                .Build();
            Watcher = RedisWatcher.Create("Redis watcher", Configuration);
        };

        Because of = async () =>
        {
            CheckResult = await Watcher.ExecuteAsync().Await().AsTask;
            RedisCheckResult = CheckResult as RedisWatcherCheckResult;
        };


        It should_invoke_get_database_async_method_only_once =
            () => RedisConnectionMock.Verify(x => x.GetDatabaseAsync(Moq.It.IsAny<int>()), Times.Once);

        It should_invoke_query_async_method_only_once =
            () => RedisMock.Verify(x => x.QueryAsync(Moq.It.IsAny<string>()), Times.Once);

        It should_have_valid_check_result = () => CheckResult.IsValid.ShouldBeTrue();
        It should_have_check_result_of_type_Redis = () => RedisCheckResult.ShouldNotBeNull();

        It should_have_set_values_in_Redis_check_result = () =>
        {
            RedisCheckResult.WatcherName.ShouldNotBeEmpty();
            RedisCheckResult.WatcherType.ShouldNotBeNull();
            RedisCheckResult.ConnectionString.ShouldNotBeEmpty();
            RedisCheckResult.Query.ShouldNotBeEmpty();
            RedisCheckResult.QueryResult.ShouldNotBeEmpty();
        };
    }

    [Subject("Redis watcher execution")]
    public class when_invoking_ensure_async_predicate_that_is_valid : RedisWatcher_specs
    {
        static Mock<IRedisConnection> RedisConnectionMock;
        static Mock<IRedis> RedisMock;

        Establish context = () =>
        {
            RedisConnectionMock = new Mock<IRedisConnection>();
            RedisMock = new Mock<IRedis>();
            RedisMock.Setup(x => x.QueryAsync(Moq.It.IsAny<string>()))
                .ReturnsAsync(QueryResult);
            RedisConnectionMock.Setup(x => x.GetDatabaseAsync(Moq.It.IsAny<int>())).ReturnsAsync(RedisMock.Object);
            Configuration = RedisWatcherConfiguration
                .Create(ConnectionString, Database)
                .WithQuery("get test")
                .EnsureThatAsync(results => Task.Factory.StartNew(() => results.Any(x => x == "test-value")))
                .WithConnectionProvider(connectionString => RedisConnectionMock.Object)
                .Build();
            Watcher = RedisWatcher.Create("Redis watcher", Configuration);
        };

        Because of = async () =>
        {
            CheckResult = await Watcher.ExecuteAsync().Await().AsTask;
            RedisCheckResult = CheckResult as RedisWatcherCheckResult;
        };

        It should_invoke_get_database_async_method_only_once =
            () => RedisConnectionMock.Verify(x => x.GetDatabaseAsync(Moq.It.IsAny<int>()), Times.Once);

        It should_invoke_query_async_method_only_once =
            () => RedisMock.Verify(x => x.QueryAsync(Moq.It.IsAny<string>()), Times.Once);

        It should_have_valid_check_result = () => CheckResult.IsValid.ShouldBeTrue();
        It should_have_check_result_of_type_Redis = () => RedisCheckResult.ShouldNotBeNull();

        It should_have_set_values_in_Redis_check_result = () =>
        {
            RedisCheckResult.WatcherName.ShouldNotBeEmpty();
            RedisCheckResult.WatcherType.ShouldNotBeNull();
            RedisCheckResult.ConnectionString.ShouldNotBeEmpty();
            RedisCheckResult.Query.ShouldNotBeEmpty();
            RedisCheckResult.QueryResult.ShouldNotBeEmpty();
        };
    }

    [Subject("Redis watcher execution")]
    public class when_invoking_ensure_predicate_that_is_invalid : RedisWatcher_specs
    {
        static Mock<IRedisConnection> RedisConnectionMock;
        static Mock<IRedis> RedisMock;

        Establish context = () =>
        {
            RedisConnectionMock = new Mock<IRedisConnection>();
            RedisMock = new Mock<IRedis>();
            RedisMock.Setup(x => x.QueryAsync(Moq.It.IsAny<string>()))
                .ReturnsAsync(QueryResult);
            RedisConnectionMock.Setup(x => x.GetDatabaseAsync(Moq.It.IsAny<int>())).ReturnsAsync(RedisMock.Object);
            Configuration = RedisWatcherConfiguration
                .Create(ConnectionString, Database)
                .WithQuery("get test")
                .EnsureThat(results => results.Any(x => x == "invalid-value"))
                .WithConnectionProvider(connectionString => RedisConnectionMock.Object)
                .Build();
            Watcher = RedisWatcher.Create("Redis watcher", Configuration);
        };

        Because of = async () =>
        {
            CheckResult = await Watcher.ExecuteAsync().Await().AsTask;
            RedisCheckResult = CheckResult as RedisWatcherCheckResult;
        };


        It should_invoke_get_database_async_method_only_once =
            () => RedisConnectionMock.Verify(x => x.GetDatabaseAsync(Moq.It.IsAny<int>()), Times.Once);

        It should_invoke_query_async_method_only_once =
            () => RedisMock.Verify(x => x.QueryAsync(Moq.It.IsAny<string>()), Times.Once);

        It should_have_invalid_check_result = () => CheckResult.IsValid.ShouldBeFalse();
        It should_have_check_result_of_type_Redis = () => RedisCheckResult.ShouldNotBeNull();

        It should_have_set_values_in_Redis_check_result = () =>
        {
            RedisCheckResult.WatcherName.ShouldNotBeEmpty();
            RedisCheckResult.WatcherType.ShouldNotBeNull();
            RedisCheckResult.ConnectionString.ShouldNotBeEmpty();
            RedisCheckResult.Query.ShouldNotBeEmpty();
            RedisCheckResult.QueryResult.ShouldNotBeEmpty();
        };
    }

    [Subject("Redis watcher execution")]
    public class when_invoking_ensure_async_predicate_that_is_invalid : RedisWatcher_specs
    {
        static Mock<IRedisConnection> RedisConnectionMock;
        static Mock<IRedis> RedisMock;

        Establish context = () =>
        {
            RedisConnectionMock = new Mock<IRedisConnection>();
            RedisMock = new Mock<IRedis>();
            RedisMock.Setup(x => x.QueryAsync(Moq.It.IsAny<string>()))
                .ReturnsAsync(QueryResult);
            RedisConnectionMock.Setup(x => x.GetDatabaseAsync(Moq.It.IsAny<int>())).ReturnsAsync(RedisMock.Object);
            Configuration = RedisWatcherConfiguration
                .Create(ConnectionString, Database)
                .WithQuery("get test")
                .EnsureThatAsync(results => Task.Factory.StartNew(() => results.Any(x => x == "invalid-value")))
                .WithConnectionProvider(connectionString => RedisConnectionMock.Object)
                .Build();
            Watcher = RedisWatcher.Create("Redis watcher", Configuration);
        };

        Because of = async () =>
        {
            CheckResult = await Watcher.ExecuteAsync().Await().AsTask;
            RedisCheckResult = CheckResult as RedisWatcherCheckResult;
        };

        It should_invoke_get_database_async_method_only_once =
            () => RedisConnectionMock.Verify(x => x.GetDatabaseAsync(Moq.It.IsAny<int>()), Times.Once);

        It should_invoke_query_async_method_only_once =
            () => RedisMock.Verify(x => x.QueryAsync( Moq.It.IsAny<string>()), Times.Once);

        It should_have_invalid_check_result = () => CheckResult.IsValid.ShouldBeFalse();
        It should_have_check_result_of_type_Redis = () => RedisCheckResult.ShouldNotBeNull();

        It should_have_set_values_in_Redis_check_result = () =>
        {
            RedisCheckResult.WatcherName.ShouldNotBeEmpty();
            RedisCheckResult.WatcherType.ShouldNotBeNull();
            RedisCheckResult.ConnectionString.ShouldNotBeEmpty();
            RedisCheckResult.Query.ShouldNotBeEmpty();
            RedisCheckResult.QueryResult.ShouldNotBeEmpty();
        };
    }
}