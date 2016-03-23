using System;
using Machine.Specifications;
using Sentry.Watchers.MongoDb;
using It = Machine.Specifications.It;

namespace Sentry.Tests.Unit.Watchers.MongoDb
{
    public class MongoDbWatcher_specs
    {
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
}