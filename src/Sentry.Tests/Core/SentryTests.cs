using System;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using Sentry.Core;
using It = Machine.Specifications.It;

namespace Sentry.Tests.Core
{
    public class Sentry_specs
    {
        protected static ISentry Sentry { get; set; }
        protected static SentryConfiguration SentryConfiguration { get; set; }
        protected static Exception Exception { get; set; }
    }

    [Subject("Sentry initialization")]
    public class when_initializing_without_configuration : Sentry_specs
    {
        Establish context = () => SentryConfiguration = null;

        Because of = () => Exception = Catch.Exception(() => Sentry = new Sentry(SentryConfiguration));

        It should_fail = () => Exception.ShouldBeOfExactType<ArgumentNullException>();
        It should_have_a_specific_reason =  () => Exception.Message.ShouldContain("Sentry configuration has not been provided.");
    }

    [Subject("Sentry initialization")]
    public class when_initializing_with_configuration : Sentry_specs
    {
        Establish context = () => SentryConfiguration = SentryConfiguration.Create().Build();

        Because of = () => Sentry = new Sentry(SentryConfiguration);

        It should_create_new_sentry_instance = () => Sentry.ShouldNotBeNull();
    }

    [Subject("Sentry execution with watcher")]
    public class when_running_single_iteration : Sentry_specs
    {
        static Mock<IWatcher> WatcherMock;

        Establish context = () =>
        {
            WatcherMock = new Mock<IWatcher>();
            WatcherMock.SetupGet(x => x.Name).Returns("Watcher mock");
            SentryConfiguration = SentryConfiguration
                .Create()
                .AddWatcher(WatcherMock.Object)
                .RunOnlyOnce()
                .Build();
            Sentry = new Sentry(SentryConfiguration);
        };

        Because of = async () => await Sentry.StartAsync().Await().AsTask;

        It should_invoke_execute_async_method_only_once = () => WatcherMock.Verify(x => x.ExecuteAsync(), Times.Once);
    }

    [Subject("Sentry execution with watcher")]
    public class when_running_ten_thousand_iterations : Sentry_specs
    {
        static Mock<IWatcher> WatcherMock;
        static int IterationsCount { get; set; }

        Establish context = () =>
        {
            WatcherMock = new Mock<IWatcher>();
            WatcherMock.SetupGet(x => x.Name).Returns("Watcher mock");
            SentryConfiguration = SentryConfiguration
                .Create()
                .AddWatcher(WatcherMock.Object)
                .SetIterationsCount(IterationsCount)
                .WithoutIterationDelay()
                .Build();
            Sentry = new Sentry(SentryConfiguration);
        };

        Because of = async () => await Sentry.StartAsync().Await().AsTask;

        It should_invoke_execute_async_method_ten_thousand_times = () => WatcherMock.Verify(x => x.ExecuteAsync(), Times.Exactly(IterationsCount));
    }

    [Subject("Sentry execution with watcher")]
    public class when_running_thousand_iterations_and_processing_watcher_hooks : Sentry_specs
    {
        static Mock<IWatcher> WatcherMock { get; set; }
        static Mock<Action<IWatcherCheck>> OnStartMock { get; set; }
        static Mock<Func<IWatcherCheck, Task>> OnStartAsyncMock { get; set; }
        static Mock<Action<ISentryCheckResult>> OnSuccessMock { get; set; }
        static Mock<Func<ISentryCheckResult, Task>> OnSuccessAsyncMock { get; set; }
        static Mock<Action<ISentryCheckResult>> OnCompletedMock { get; set; }
        static Mock<Func<ISentryCheckResult, Task>> OnCompletedAsyncMock { get; set; }
        static int IterationsCount { get; set; }

        Establish context = () =>
        {
            IterationsCount = 1000;
            OnStartMock = new Mock<Action<IWatcherCheck>>();
            OnStartAsyncMock = new Mock<Func<IWatcherCheck, Task>>();
            OnSuccessMock = new Mock<Action<ISentryCheckResult>>();
            OnSuccessAsyncMock = new Mock<Func<ISentryCheckResult, Task>>();
            OnCompletedMock = new Mock<Action<ISentryCheckResult>>();
            OnCompletedAsyncMock = new Mock<Func<ISentryCheckResult, Task>>();
            WatcherMock = new Mock<IWatcher>();
            WatcherMock.SetupGet(x => x.Name).Returns("Watcher mock");
            WatcherMock.Setup(x => x.ExecuteAsync()).ReturnsAsync(WatcherCheckResult.Create(WatcherMock.Object, true));
            SentryConfiguration = SentryConfiguration
                .Create()
                .AddWatcher(WatcherMock.Object)
                .SetGlobalWatcherHooks(hooks =>
                {
                    hooks.OnStart(check => OnStartMock.Object(check));
                    hooks.OnStartAsync(check => OnStartAsyncMock.Object(check));
                    hooks.OnSuccess(result => OnSuccessMock.Object(result));
                    hooks.OnSuccessAsync(check => OnSuccessAsyncMock.Object(check));
                    hooks.OnCompleted(result => OnCompletedMock.Object(result));
                    hooks.OnCompletedAsync(check => OnCompletedAsyncMock.Object(check));
                })
                .SetIterationsCount(IterationsCount)
                .WithoutIterationDelay()
                .Build();
            Sentry = new Sentry(SentryConfiguration);
        };

        Because of = async () => await Sentry.StartAsync().Await().AsTask;

        It should_invoke_execute_async_method_one_thousand_times = () => WatcherMock.Verify(x => x.ExecuteAsync(), Times.Exactly(IterationsCount));
        It should_invoke_on_start_hook_one_thousand_times = () => OnStartMock.Verify(x => x(Moq.It.IsAny<IWatcherCheck>()), Times.Exactly(IterationsCount));
        It should_invoke_on_start_async_hook_one_thousand_times = () => OnStartAsyncMock.Verify(x => x(Moq.It.IsAny<IWatcherCheck>()), Times.Exactly(IterationsCount));
        It should_invoke_on_success_hook_one_thousand_times = () => OnSuccessMock.Verify(x => x(Moq.It.IsAny<ISentryCheckResult>()), Times.Exactly(IterationsCount));
        It should_invoke_on_success_async_hook_one_thousand_times = () => OnSuccessAsyncMock.Verify(x => x(Moq.It.IsAny<ISentryCheckResult>()), Times.Exactly(IterationsCount));
        It should_invoke_on_completed_hook_one_thousand_times = () => OnCompletedMock.Verify(x => x(Moq.It.IsAny<ISentryCheckResult>()), Times.Exactly(IterationsCount));
        It should_invoke_on_completed_async_hook_one_thousand_times = () => OnCompletedAsyncMock.Verify(x => x(Moq.It.IsAny<ISentryCheckResult>()), Times.Exactly(IterationsCount));
    }

    [Subject("Sentry execution with watcher")]
    public class when_running_one_iteration_and_processing_iteration_hooks : Sentry_specs
    {
        static Mock<IWatcher> WatcherMock { get; set; }
        static Mock<Action> OnStartMock { get; set; }
        static Mock<Func<Task>> OnStartAsyncMock { get; set; }
        static Mock<Action<long>> OnIterationStartMock { get; set; }
        static Mock<Func<long,Task>> OnIterationStartAsyncMock { get; set; }
        static Mock<Action<ISentryIteration>> OnIterationCompletedMock { get; set; }
        static Mock<Func<ISentryIteration, Task>> OnIterationCompletedAsyncMock { get; set; }

        Establish context = () =>
        {
            OnStartMock = new Mock<Action>();
            OnStartAsyncMock = new Mock<Func<Task>>();
            OnIterationStartMock = new Mock<Action<long>>();
            OnIterationStartAsyncMock = new Mock<Func<long, Task>>();
            OnIterationCompletedMock = new Mock<Action<ISentryIteration>>();
            OnIterationCompletedAsyncMock = new Mock<Func<ISentryIteration, Task>>();
            WatcherMock = new Mock<IWatcher>();
            WatcherMock.SetupGet(x => x.Name).Returns("Watcher mock");
            WatcherMock.Setup(x => x.ExecuteAsync()).ReturnsAsync(WatcherCheckResult.Create(WatcherMock.Object, true));
            SentryConfiguration = SentryConfiguration
                .Create()
                .AddWatcher(WatcherMock.Object)
                .SetHooks(hooks =>
                {
                    hooks.OnStart(() => OnStartMock.Object());
                    hooks.OnStartAsync(() => OnStartAsyncMock.Object());
                    hooks.OnIterationStart(ordinal => OnIterationStartMock.Object(ordinal));
                    hooks.OnIterationStartAsync(ordinal => OnIterationStartAsyncMock.Object(ordinal));
                    hooks.OnIterationCompleted(iteration => OnIterationCompletedMock.Object(iteration));
                    hooks.OnIterationCompletedAsync(iteration => OnIterationCompletedAsyncMock.Object(iteration));
                })
                .WithoutIterationDelay()
                .RunOnlyOnce()
                .Build();
            Sentry = new Sentry(SentryConfiguration);
        };

        Because of = async () => await Sentry.StartAsync().Await().AsTask;

        It should_invoke_execute_async_method_only_once = () => WatcherMock.Verify(x => x.ExecuteAsync(), Times.Once);
        It should_invoke_on_start_hook_only_once = () => OnStartMock.Verify(x => x(), Times.Once);
        It should_invoke_on_start_async_hook_only_once = () => OnStartAsyncMock.Verify(x => x(), Times.Once);
        It should_invoke_on_iteration_start_hook_only_once = () => OnIterationStartMock.Verify(x => x(Moq.It.IsAny<long>()), Times.Once);
        It should_invoke_on_iteration_start_async_hook_only_once = () => OnIterationStartAsyncMock.Verify(x => x(Moq.It.IsAny<long>()), Times.Once);
        It should_invoke_on_iteration_completed_hook_only_once = () => OnIterationCompletedMock.Verify(x => x(Moq.It.IsAny<ISentryIteration>()), Times.Once);
        It should_invoke_on_iteration_completed_async_hook_only_once = () => OnIterationCompletedAsyncMock.Verify(x => x(Moq.It.IsAny<ISentryIteration>()), Times.Once);
    }

    [Subject("Sentry execution with watcher")]
    public class when_running_one_iteration_and_processing_watcher_hooks_for_valid_result : Sentry_specs
    {
        static Mock<IWatcher> WatcherMock { get; set; }
        static Mock<Action<IWatcherCheck>> OnStartMock { get; set; }
        static Mock<Func<IWatcherCheck, Task>> OnStartAsyncMock { get; set; }
        static Mock<Action<ISentryCheckResult>> OnSuccessMock { get; set; }
        static Mock<Func<ISentryCheckResult, Task>> OnSuccessAsyncMock { get; set; }
        static Mock<Action<ISentryCheckResult>> OnCompletedMock { get; set; }
        static Mock<Func<ISentryCheckResult, Task>> OnCompletedAsyncMock { get; set; }

        Establish context = () =>
        {
            OnStartMock = new Mock<Action<IWatcherCheck>>();
            OnStartAsyncMock = new Mock<Func<IWatcherCheck, Task>>();
            OnSuccessMock = new Mock<Action<ISentryCheckResult>>();
            OnSuccessAsyncMock = new Mock<Func<ISentryCheckResult, Task>>();
            OnCompletedMock = new Mock<Action<ISentryCheckResult>>();
            OnCompletedAsyncMock = new Mock<Func<ISentryCheckResult, Task>>();
            WatcherMock = new Mock<IWatcher>();
            WatcherMock.SetupGet(x => x.Name).Returns("Watcher mock");
            WatcherMock.Setup(x => x.ExecuteAsync()).ReturnsAsync(WatcherCheckResult.Create(WatcherMock.Object, true));
            SentryConfiguration = SentryConfiguration
                .Create()
                .AddWatcher(WatcherMock.Object, hooks =>
                {
                    hooks.OnStart(check => OnStartMock.Object(check));
                    hooks.OnStartAsync(check => OnStartAsyncMock.Object(check));
                    hooks.OnSuccess(result => OnSuccessMock.Object(result));
                    hooks.OnSuccessAsync(check => OnSuccessAsyncMock.Object(check));
                    hooks.OnCompleted(result => OnCompletedMock.Object(result));
                    hooks.OnCompletedAsync(check => OnCompletedAsyncMock.Object(check));
                })
                .WithoutIterationDelay()
                .RunOnlyOnce()
                .Build();
            Sentry = new Sentry(SentryConfiguration);
        };

        Because of = async () => await Sentry.StartAsync().Await().AsTask;

        It should_invoke_execute_async_method_only_once = () => WatcherMock.Verify(x => x.ExecuteAsync(), Times.Once);
        It should_invoke_on_start_hook_only_once = () => OnStartMock.Verify(x => x(Moq.It.IsAny<IWatcherCheck>()), Times.Once);
        It should_invoke_on_start_async_hook_only_once = () => OnStartAsyncMock.Verify(x => x(Moq.It.IsAny<IWatcherCheck>()), Times.Once);
        It should_invoke_on_success_hook_only_once = () => OnSuccessMock.Verify(x => x(Moq.It.IsAny<ISentryCheckResult>()), Times.Once);
        It should_invoke_on_success_async_hook_only_once = () => OnSuccessAsyncMock.Verify(x => x(Moq.It.IsAny<ISentryCheckResult>()), Times.Once);
        It should_invoke_on_completed_hook_only_once = () => OnCompletedMock.Verify(x => x(Moq.It.IsAny<ISentryCheckResult>()), Times.Once);
        It should_invoke_on_completed_async_hook_only_once = () => OnCompletedAsyncMock.Verify(x => x(Moq.It.IsAny<ISentryCheckResult>()), Times.Once);
    }

    [Subject("Sentry execution with watcher")]
    public class when_running_one_iteration_and_processing_global_watcher_hooks_for_valid_result : Sentry_specs
    {
        static Mock<IWatcher> WatcherMock { get; set; }
        static Mock<Action<IWatcherCheck>> OnStartMock { get; set; }
        static Mock<Func<IWatcherCheck, Task>> OnStartAsyncMock { get; set; }
        static Mock<Action<ISentryCheckResult>> OnSuccessMock { get; set; }
        static Mock<Func<ISentryCheckResult, Task>> OnSuccessAsyncMock { get; set; }
        static Mock<Action<ISentryCheckResult>> OnCompletedMock { get; set; }
        static Mock<Func<ISentryCheckResult, Task>> OnCompletedAsyncMock { get; set; }

        Establish context = () =>
        {
            OnStartMock = new Mock<Action<IWatcherCheck>>();
            OnStartAsyncMock = new Mock<Func<IWatcherCheck, Task>>();
            OnSuccessMock = new Mock<Action<ISentryCheckResult>>();
            OnSuccessAsyncMock = new Mock<Func<ISentryCheckResult, Task>>();
            OnCompletedMock = new Mock<Action<ISentryCheckResult>>();
            OnCompletedAsyncMock = new Mock<Func<ISentryCheckResult, Task>>();
            WatcherMock = new Mock<IWatcher>();
            WatcherMock.SetupGet(x => x.Name).Returns("Watcher mock");
            WatcherMock.Setup(x => x.ExecuteAsync()).ReturnsAsync(WatcherCheckResult.Create(WatcherMock.Object, true));
            SentryConfiguration = SentryConfiguration
                .Create()
                .AddWatcher(WatcherMock.Object)
                .SetGlobalWatcherHooks(hooks =>
                {
                    hooks.OnStart(check => OnStartMock.Object(check));
                    hooks.OnStartAsync(check => OnStartAsyncMock.Object(check));
                    hooks.OnSuccess(result => OnSuccessMock.Object(result));
                    hooks.OnSuccessAsync(check => OnSuccessAsyncMock.Object(check));
                    hooks.OnCompleted(result => OnCompletedMock.Object(result));
                    hooks.OnCompletedAsync(check => OnCompletedAsyncMock.Object(check));
                })
                .WithoutIterationDelay()
                .RunOnlyOnce()
                .Build();
            Sentry = new Sentry(SentryConfiguration);
        };

        Because of = async () => await Sentry.StartAsync().Await().AsTask;

        It should_invoke_execute_async_method_only_once = () => WatcherMock.Verify(x => x.ExecuteAsync(), Times.Once);
        It should_invoke_on_start_hook_only_once = () => OnStartMock.Verify(x => x(Moq.It.IsAny<IWatcherCheck>()), Times.Once);
        It should_invoke_on_start_async_hook_only_once = () => OnStartAsyncMock.Verify(x => x(Moq.It.IsAny<IWatcherCheck>()), Times.Once);
        It should_invoke_on_success_hook_only_once = () => OnSuccessMock.Verify(x => x(Moq.It.IsAny<ISentryCheckResult>()), Times.Once);
        It should_invoke_on_success_async_hook_only_once = () => OnSuccessAsyncMock.Verify(x => x(Moq.It.IsAny<ISentryCheckResult>()), Times.Once);
        It should_invoke_on_completed_hook_only_once = () => OnCompletedMock.Verify(x => x(Moq.It.IsAny<ISentryCheckResult>()), Times.Once);
        It should_invoke_on_completed_async_hook_only_once = () => OnCompletedAsyncMock.Verify(x => x(Moq.It.IsAny<ISentryCheckResult>()), Times.Once);
    }

    [Subject("Sentry execution with watcher")]
    public class when_running_one_iteration_and_processing_watcher_hooks_for_invalid_result : Sentry_specs
    {
        static Mock<IWatcher> WatcherMock { get; set; }
        static Mock<Action<ISentryCheckResult>> OnFailureMock { get; set; }
        static Mock<Func<ISentryCheckResult, Task>> OnFailureAsyncMock { get; set; }

        Establish context = () =>
        {
            OnFailureMock = new Mock<Action<ISentryCheckResult>>();
            OnFailureAsyncMock = new Mock<Func<ISentryCheckResult, Task>>();
            WatcherMock = new Mock<IWatcher>();
            WatcherMock.SetupGet(x => x.Name).Returns("Watcher mock");
            WatcherMock.Setup(x => x.ExecuteAsync()).ReturnsAsync(WatcherCheckResult.Create(WatcherMock.Object, false));
            SentryConfiguration = SentryConfiguration
                .Create()
                .AddWatcher(WatcherMock.Object, hooks =>
                {
                    hooks.OnFailure(result => OnFailureMock.Object(result));
                    hooks.OnFailureAsync(result => OnFailureAsyncMock.Object(result));
                })
                .WithoutIterationDelay()
                .RunOnlyOnce()
                .Build();
            Sentry = new Sentry(SentryConfiguration);
        };

        Because of = async () => await Sentry.StartAsync().Await().AsTask;

        It should_invoke_execute_async_method_only_once = () => WatcherMock.Verify(x => x.ExecuteAsync(), Times.Once);
        It should_invoke_on_failure_hook_only_once = () => OnFailureMock.Verify(x => x(Moq.It.IsAny<ISentryCheckResult>()), Times.Once);
        It should_invoke_on_failure_async_hook_only_once = () => OnFailureAsyncMock.Verify(x => x(Moq.It.IsAny<ISentryCheckResult>()), Times.Once);
    }

    [Subject("Sentry execution with watcher")]
    public class when_running_one_iteration_and_processing_watcher_hooks_for_exception : Sentry_specs
    {
        static Mock<IWatcher> WatcherMock { get; set; }
        static Mock<Action<Exception>> OnErrorMock { get; set; }
        static Mock<Func<Exception, Task>> OnErrorAsyncMock { get; set; }

        Establish context = () =>
        {
            OnErrorMock = new Mock<Action<Exception>>();
            OnErrorAsyncMock = new Mock<Func<Exception, Task>>();
            WatcherMock = new Mock<IWatcher>();
            WatcherMock.SetupGet(x => x.Name).Returns("Watcher mock");
            WatcherMock.Setup(x => x.ExecuteAsync()).Throws<Exception>();
            SentryConfiguration = SentryConfiguration
                .Create()
                .AddWatcher(WatcherMock.Object, hooks =>
                {
                    hooks.OnError(exception => OnErrorMock.Object(exception));
                    hooks.OnErrorAsync(exception => OnErrorAsyncMock.Object(exception));
                })
                .WithoutIterationDelay()
                .RunOnlyOnce()
                .Build();
            Sentry = new Sentry(SentryConfiguration);
        };

        Because of = async () => await Sentry.StartAsync().Await().AsTask;

        It should_invoke_execute_async_method_only_once = () => WatcherMock.Verify(x => x.ExecuteAsync(), Times.Once);
        It should_invoke_on_error_hook_only_once = () => OnErrorMock.Verify(x => x(Moq.It.IsAny<Exception>()), Times.Once);
        It should_invoke_on_error_async_hook_only_once = () => OnErrorAsyncMock.Verify(x => x(Moq.It.IsAny<Exception>()), Times.Once);
    }

    [Subject("Sentry execution with watcher")]
    public class when_running_ten_iterations_and_processing_on_first_failure_watcher_hooks_for_invalid_result : Sentry_specs
    {
        static Mock<IWatcher> WatcherMock { get; set; }
        static Mock<Action<ISentryCheckResult>> OnFirstFailureMock { get; set; }
        static Mock<Func<ISentryCheckResult, Task>> OnFirstFailureAsyncMock { get; set; }
        static int IterationsCount { get; set; }

        Establish context = () =>
        {
            IterationsCount = 10;
            OnFirstFailureMock = new Mock<Action<ISentryCheckResult>>();
            OnFirstFailureAsyncMock = new Mock<Func<ISentryCheckResult, Task>>();
            WatcherMock = new Mock<IWatcher>();
            WatcherMock.SetupGet(x => x.Name).Returns("Watcher mock");
            WatcherMock.Setup(x => x.ExecuteAsync()).ReturnsAsync(WatcherCheckResult.Create(WatcherMock.Object, false));
            SentryConfiguration = SentryConfiguration
                .Create()
                .AddWatcher(WatcherMock.Object, hooks =>
                {
                    hooks.OnFirstFailure(result => OnFirstFailureMock.Object(result));
                    hooks.OnFirstFailureAsync(result => OnFirstFailureAsyncMock.Object(result));
                })
                .WithoutIterationDelay()
                .SetIterationsCount(IterationsCount)
                .Build();
            Sentry = new Sentry(SentryConfiguration);
        };

        Because of = async () => await Sentry.StartAsync().Await().AsTask;

        It should_invoke_execute_async_method_ten_times = () => WatcherMock.Verify(x => x.ExecuteAsync(), Times.Exactly(IterationsCount));
        It should_invoke_on_first_failure_hook_only_once = () => OnFirstFailureMock.Verify(x => x(Moq.It.IsAny<ISentryCheckResult>()), Times.Once);
        It should_invoke_on_first_failure_async_hook_only_once = () => OnFirstFailureAsyncMock.Verify(x => x(Moq.It.IsAny<ISentryCheckResult>()), Times.Once);
    }

    [Subject("Sentry execution with watcher")]
    public class when_running_ten_iterations_and_processing_on_first_error_watcher_hooks_for_exception : Sentry_specs
    {
        static Mock<IWatcher> WatcherMock { get; set; }
        static Mock<Action<Exception>> OnFirstErrorMock { get; set; }
        static Mock<Func<Exception, Task>> OnFirstErrorAsyncMock { get; set; }
        static int IterationsCount { get; set; }

        Establish context = () =>
        {
            IterationsCount = 10;
            OnFirstErrorMock = new Mock<Action<Exception>>();
            OnFirstErrorAsyncMock = new Mock<Func<Exception, Task>>();
            WatcherMock = new Mock<IWatcher>();
            WatcherMock.SetupGet(x => x.Name).Returns("Watcher mock");
            WatcherMock.Setup(x => x.ExecuteAsync()).Throws<Exception>();
            SentryConfiguration = SentryConfiguration
                .Create()
                .AddWatcher(WatcherMock.Object, hooks =>
                {
                    hooks.OnFirstError(result => OnFirstErrorMock.Object(result));
                    hooks.OnFirstErrorAsync(result => OnFirstErrorAsyncMock.Object(result));
                })
                .WithoutIterationDelay()
                .SetIterationsCount(IterationsCount)
                .Build();
            Sentry = new Sentry(SentryConfiguration);
        };

        Because of = async () => await Sentry.StartAsync().Await().AsTask;

        It should_invoke_execute_async_method_ten_times = () => WatcherMock.Verify(x => x.ExecuteAsync(), Times.Exactly(IterationsCount));
        It should_invoke_on_first_error_hook_only_once = () => OnFirstErrorMock.Verify(x => x(Moq.It.IsAny<Exception>()), Times.Once);
        It should_invoke_on_first_error_async_hook_only_once = () => OnFirstErrorAsyncMock.Verify(x => x(Moq.It.IsAny<Exception>()), Times.Once);
    }

    [Subject("Sentry execution with watcher")]
    [Ignore("Not sure yet how to implement that test")]
    public class when_running_ten_iterations_and_processing_on_first_success_watcher_hooks_for_valid_result : Sentry_specs
    {
        static Mock<IWatcher> WatcherMock { get; set; }
        static Mock<Action<ISentryCheckResult>> OnFirstSuccessMock { get; set; }
        static Mock<Func<ISentryCheckResult, Task>> OnFirstSuccessAsyncMock { get; set; }
        static int IterationsCount { get; set; }

        Establish context = () =>
        {
            IterationsCount = 10;
            OnFirstSuccessMock = new Mock<Action<ISentryCheckResult>>();
            OnFirstSuccessAsyncMock = new Mock<Func<ISentryCheckResult, Task>>();
            WatcherMock = new Mock<IWatcher>();
            WatcherMock.SetupGet(x => x.Name).Returns("Watcher mock");
            WatcherMock.Setup(x => x.ExecuteAsync()).ReturnsAsync(WatcherCheckResult.Create(WatcherMock.Object, true));
            SentryConfiguration = SentryConfiguration
                .Create()
                .AddWatcher(WatcherMock.Object, hooks =>
                {
                    hooks.OnFirstSuccess(result => OnFirstSuccessMock.Object(result));
                    hooks.OnFirstSuccessAsync(result => OnFirstSuccessAsyncMock.Object(result));
                })
                .WithoutIterationDelay()
                .SetIterationsCount(IterationsCount)
                .Build();
            Sentry = new Sentry(SentryConfiguration);
        };

        Because of = async () => await Sentry.StartAsync().Await().AsTask;

        It should_invoke_execute_async_method_ten_times = () => WatcherMock.Verify(x => x.ExecuteAsync(), Times.Exactly(IterationsCount));
        It should_invoke_on_first_success_hook_only_once = () => OnFirstSuccessMock.Verify(x => x(Moq.It.IsAny<ISentryCheckResult>()), Times.Once);
        It should_invoke_on_first_success_async_hook_only_once = () => OnFirstSuccessAsyncMock.Verify(x => x(Moq.It.IsAny<ISentryCheckResult>()), Times.Once);
    }
}