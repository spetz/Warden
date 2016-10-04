using System;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Warden.Core;
using Warden.Watchers;
using Machine.Specifications;
using It = Machine.Specifications.It;

namespace Warden.Tests.Core
{
    public class Warden_specs
    {
        protected static IWarden Warden { get; set; }
        protected static WardenConfiguration WardenConfiguration { get; set; }
        protected static Exception Exception { get; set; }
    }

    [Subject("Warden initialization")]
    public class when_initializing_without_configuration : Warden_specs
    {
        Establish context = () => WardenConfiguration = null;

        Because of = () => Exception = Catch.Exception(() => Warden = WardenInstance.Create(WardenConfiguration));

        It should_fail = () => Exception.Should().BeOfType<ArgumentNullException>();
        It should_have_a_specific_reason = () => Exception.Message.Should().Contain("Warden configuration has not been provided.");
    }

    [Subject("Warden initialization")]
    public class when_initializing_with_configuration : Warden_specs
    {
        Establish context = () => WardenConfiguration = WardenConfiguration.Create().Build();

        Because of = () => Warden = WardenInstance.Create(WardenConfiguration);

        It should_create_new_warden_instance = () => Warden.Should().NotBeNull();
    }

    [Subject("Warden execution with watcher")]
    public class when_running_single_iteration : Warden_specs
    {
        static Mock<IWatcher> WatcherMock;

        Establish context = () =>
        {
            WatcherMock = new Mock<IWatcher>();
            WatcherMock.SetupGet(x => x.Name).Returns("Watcher mock");
            WardenConfiguration = WardenConfiguration
                .Create()
                .AddWatcher(WatcherMock.Object)
                .RunOnlyOnce()
                .Build();
            Warden = WardenInstance.Create(WardenConfiguration);
        };

        Because of = async () => await Warden.StartAsync().Await().AsTask;

        It should_invoke_execute_async_method_only_once = () => WatcherMock.Verify(x => x.ExecuteAsync(), Times.Once);

    }

    [Subject("Warden execution with watcher")]
    public class when_running_ten_thousand_iterations : Warden_specs
    {
        static Mock<IWatcher> WatcherMock;
        static int IterationsCount { get; set; }

        Establish context = () =>
        {
            WatcherMock = new Mock<IWatcher>();
            WatcherMock.SetupGet(x => x.Name).Returns("Watcher mock");
            WardenConfiguration = WardenConfiguration
                .Create()
                .AddWatcher(WatcherMock.Object)
                .SetIterationsCount(IterationsCount)
                .WithMinimalInterval()
                .Build();
            Warden = WardenInstance.Create(WardenConfiguration);
        };

        Because of = async () => await Warden.StartAsync().Await().AsTask;

        It should_invoke_execute_async_method_ten_thousand_times = () => WatcherMock.Verify(x => x.ExecuteAsync(), Times.Exactly(IterationsCount));
    }

    [Subject("Warden execution with watcher")]
    public class when_running_thousand_iterations_and_processing_watcher_hooks : Warden_specs
    {
        static Mock<IWatcher> WatcherMock { get; set; }
        static Mock<Action<IWatcherCheck>> OnStartMock { get; set; }
        static Mock<Func<IWatcherCheck, Task>> OnStartAsyncMock { get; set; }
        static Mock<Action<IWardenCheckResult>> OnSuccessMock { get; set; }
        static Mock<Func<IWardenCheckResult, Task>> OnSuccessAsyncMock { get; set; }
        static Mock<Action<IWardenCheckResult>> OnCompletedMock { get; set; }
        static Mock<Func<IWardenCheckResult, Task>> OnCompletedAsyncMock { get; set; }
        static int IterationsCount { get; set; }

        Establish context = () =>
        {
            IterationsCount = 1000;
            OnStartMock = new Mock<Action<IWatcherCheck>>();
            OnStartAsyncMock = new Mock<Func<IWatcherCheck, Task>>();
            OnSuccessMock = new Mock<Action<IWardenCheckResult>>();
            OnSuccessAsyncMock = new Mock<Func<IWardenCheckResult, Task>>();
            OnCompletedMock = new Mock<Action<IWardenCheckResult>>();
            OnCompletedAsyncMock = new Mock<Func<IWardenCheckResult, Task>>();
            WatcherMock = new Mock<IWatcher>();
            WatcherMock.SetupGet(x => x.Name).Returns("Watcher mock");
            WatcherMock.Setup(x => x.ExecuteAsync()).ReturnsAsync(WatcherCheckResult.Create(WatcherMock.Object, true));
            WardenConfiguration = WardenConfiguration
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
                .WithMinimalInterval()
                .Build();
            Warden = WardenInstance.Create(WardenConfiguration);
        };

        Because of = async () => await Warden.StartAsync().Await().AsTask;

        It should_invoke_execute_async_method_one_thousand_times = () => WatcherMock.Verify(x => x.ExecuteAsync(), Times.Exactly(IterationsCount));
        It should_invoke_on_start_hook_one_thousand_times = () => OnStartMock.Verify(x => x(Moq.It.IsAny<IWatcherCheck>()), Times.Exactly(IterationsCount));
        It should_invoke_on_start_async_hook_one_thousand_times = () => OnStartAsyncMock.Verify(x => x(Moq.It.IsAny<IWatcherCheck>()), Times.Exactly(IterationsCount));
        It should_invoke_on_success_hook_one_thousand_times = () => OnSuccessMock.Verify(x => x(Moq.It.IsAny<IWardenCheckResult>()), Times.Exactly(IterationsCount));
        It should_invoke_on_success_async_hook_one_thousand_times = () => OnSuccessAsyncMock.Verify(x => x(Moq.It.IsAny<IWardenCheckResult>()), Times.Exactly(IterationsCount));
        It should_invoke_on_completed_hook_one_thousand_times = () => OnCompletedMock.Verify(x => x(Moq.It.IsAny<IWardenCheckResult>()), Times.Exactly(IterationsCount));
        It should_invoke_on_completed_async_hook_one_thousand_times = () => OnCompletedAsyncMock.Verify(x => x(Moq.It.IsAny<IWardenCheckResult>()), Times.Exactly(IterationsCount));
    }

    [Subject("Warden execution with watcher")]
    public class when_running_one_iteration_and_processing_iteration_hooks : Warden_specs
    {
        static Mock<IWatcher> WatcherMock { get; set; }
        static Mock<Action> OnStartMock { get; set; }
        static Mock<Func<Task>> OnStartAsyncMock { get; set; }
        static Mock<Action<long>> OnIterationStartMock { get; set; }
        static Mock<Func<long,Task>> OnIterationStartAsyncMock { get; set; }
        static Mock<Action<IWardenIteration>> OnIterationCompletedMock { get; set; }
        static Mock<Func<IWardenIteration, Task>> OnIterationCompletedAsyncMock { get; set; }

        Establish context = () =>
        {
            OnStartMock = new Mock<Action>();
            OnStartAsyncMock = new Mock<Func<Task>>();
            OnIterationStartMock = new Mock<Action<long>>();
            OnIterationStartAsyncMock = new Mock<Func<long, Task>>();
            OnIterationCompletedMock = new Mock<Action<IWardenIteration>>();
            OnIterationCompletedAsyncMock = new Mock<Func<IWardenIteration, Task>>();
            WatcherMock = new Mock<IWatcher>();
            WatcherMock.SetupGet(x => x.Name).Returns("Watcher mock");
            WatcherMock.Setup(x => x.ExecuteAsync()).ReturnsAsync(WatcherCheckResult.Create(WatcherMock.Object, true));
            WardenConfiguration = WardenConfiguration
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
                .WithMinimalInterval()
                .RunOnlyOnce()
                .Build();
            Warden = WardenInstance.Create(WardenConfiguration);
        };

        Because of = async () => await Warden.StartAsync().Await().AsTask;

        It should_invoke_execute_async_method_only_once = () => WatcherMock.Verify(x => x.ExecuteAsync(), Times.Once);
        It should_invoke_on_start_hook_only_once = () => OnStartMock.Verify(x => x(), Times.Once);
        It should_invoke_on_start_async_hook_only_once = () => OnStartAsyncMock.Verify(x => x(), Times.Once);
        It should_invoke_on_iteration_start_hook_only_once = () => OnIterationStartMock.Verify(x => x(Moq.It.IsAny<long>()), Times.Once);
        It should_invoke_on_iteration_start_async_hook_only_once = () => OnIterationStartAsyncMock.Verify(x => x(Moq.It.IsAny<long>()), Times.Once);
        It should_invoke_on_iteration_completed_hook_only_once = () => OnIterationCompletedMock.Verify(x => x(Moq.It.IsAny<IWardenIteration>()), Times.Once);
        It should_invoke_on_iteration_completed_async_hook_only_once = () => OnIterationCompletedAsyncMock.Verify(x => x(Moq.It.IsAny<IWardenIteration>()), Times.Once);
    }

    [Subject("Warden execution with watcher")]
    public class when_running_one_iteration_and_processing_watcher_hooks_for_valid_result : Warden_specs
    {
        static Mock<IWatcher> WatcherMock { get; set; }
        static Mock<Action<IWatcherCheck>> OnStartMock { get; set; }
        static Mock<Func<IWatcherCheck, Task>> OnStartAsyncMock { get; set; }
        static Mock<Action<IWardenCheckResult>> OnSuccessMock { get; set; }
        static Mock<Func<IWardenCheckResult, Task>> OnSuccessAsyncMock { get; set; }
        static Mock<Action<IWardenCheckResult>> OnCompletedMock { get; set; }
        static Mock<Func<IWardenCheckResult, Task>> OnCompletedAsyncMock { get; set; }

        Establish context = () =>
        {
            OnStartMock = new Mock<Action<IWatcherCheck>>();
            OnStartAsyncMock = new Mock<Func<IWatcherCheck, Task>>();
            OnSuccessMock = new Mock<Action<IWardenCheckResult>>();
            OnSuccessAsyncMock = new Mock<Func<IWardenCheckResult, Task>>();
            OnCompletedMock = new Mock<Action<IWardenCheckResult>>();
            OnCompletedAsyncMock = new Mock<Func<IWardenCheckResult, Task>>();
            WatcherMock = new Mock<IWatcher>();
            WatcherMock.SetupGet(x => x.Name).Returns("Watcher mock");
            WatcherMock.Setup(x => x.ExecuteAsync()).ReturnsAsync(WatcherCheckResult.Create(WatcherMock.Object, true));
            WardenConfiguration = WardenConfiguration
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
                .WithMinimalInterval()
                .RunOnlyOnce()
                .Build();
            Warden = WardenInstance.Create(WardenConfiguration);
        };

        Because of = async () => await Warden.StartAsync().Await().AsTask;

        It should_invoke_execute_async_method_only_once = () => WatcherMock.Verify(x => x.ExecuteAsync(), Times.Once);
        It should_invoke_on_start_hook_only_once = () => OnStartMock.Verify(x => x(Moq.It.IsAny<IWatcherCheck>()), Times.Once);
        It should_invoke_on_start_async_hook_only_once = () => OnStartAsyncMock.Verify(x => x(Moq.It.IsAny<IWatcherCheck>()), Times.Once);
        It should_invoke_on_success_hook_only_once = () => OnSuccessMock.Verify(x => x(Moq.It.IsAny<IWardenCheckResult>()), Times.Once);
        It should_invoke_on_success_async_hook_only_once = () => OnSuccessAsyncMock.Verify(x => x(Moq.It.IsAny<IWardenCheckResult>()), Times.Once);
        It should_invoke_on_completed_hook_only_once = () => OnCompletedMock.Verify(x => x(Moq.It.IsAny<IWardenCheckResult>()), Times.Once);
        It should_invoke_on_completed_async_hook_only_once = () => OnCompletedAsyncMock.Verify(x => x(Moq.It.IsAny<IWardenCheckResult>()), Times.Once);
    }

    [Subject("Warden execution with watcher")]
    public class when_running_one_iteration_and_processing_global_watcher_hooks_for_valid_result : Warden_specs
    {
        static Mock<IWatcher> WatcherMock { get; set; }
        static Mock<Action<IWatcherCheck>> OnStartMock { get; set; }
        static Mock<Func<IWatcherCheck, Task>> OnStartAsyncMock { get; set; }
        static Mock<Action<IWardenCheckResult>> OnSuccessMock { get; set; }
        static Mock<Func<IWardenCheckResult, Task>> OnSuccessAsyncMock { get; set; }
        static Mock<Action<IWardenCheckResult>> OnCompletedMock { get; set; }
        static Mock<Func<IWardenCheckResult, Task>> OnCompletedAsyncMock { get; set; }

        Establish context = () =>
        {
            OnStartMock = new Mock<Action<IWatcherCheck>>();
            OnStartAsyncMock = new Mock<Func<IWatcherCheck, Task>>();
            OnSuccessMock = new Mock<Action<IWardenCheckResult>>();
            OnSuccessAsyncMock = new Mock<Func<IWardenCheckResult, Task>>();
            OnCompletedMock = new Mock<Action<IWardenCheckResult>>();
            OnCompletedAsyncMock = new Mock<Func<IWardenCheckResult, Task>>();
            WatcherMock = new Mock<IWatcher>();
            WatcherMock.SetupGet(x => x.Name).Returns("Watcher mock");
            WatcherMock.Setup(x => x.ExecuteAsync()).ReturnsAsync(WatcherCheckResult.Create(WatcherMock.Object, true));
            WardenConfiguration = WardenConfiguration
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
                .WithMinimalInterval()
                .RunOnlyOnce()
                .Build();
            Warden = WardenInstance.Create(WardenConfiguration);
        };

        Because of = async () => await Warden.StartAsync().Await().AsTask;

        It should_invoke_execute_async_method_only_once = () => WatcherMock.Verify(x => x.ExecuteAsync(), Times.Once);
        It should_invoke_on_start_hook_only_once = () => OnStartMock.Verify(x => x(Moq.It.IsAny<IWatcherCheck>()), Times.Once);
        It should_invoke_on_start_async_hook_only_once = () => OnStartAsyncMock.Verify(x => x(Moq.It.IsAny<IWatcherCheck>()), Times.Once);
        It should_invoke_on_success_hook_only_once = () => OnSuccessMock.Verify(x => x(Moq.It.IsAny<IWardenCheckResult>()), Times.Once);
        It should_invoke_on_success_async_hook_only_once = () => OnSuccessAsyncMock.Verify(x => x(Moq.It.IsAny<IWardenCheckResult>()), Times.Once);
        It should_invoke_on_completed_hook_only_once = () => OnCompletedMock.Verify(x => x(Moq.It.IsAny<IWardenCheckResult>()), Times.Once);
        It should_invoke_on_completed_async_hook_only_once = () => OnCompletedAsyncMock.Verify(x => x(Moq.It.IsAny<IWardenCheckResult>()), Times.Once);
    }

    [Subject("Warden execution with watcher")]
    public class when_running_one_iteration_and_processing_watcher_hooks_for_invalid_result : Warden_specs
    {
        static Mock<IWatcher> WatcherMock { get; set; }
        static Mock<Action<IWardenCheckResult>> OnFailureMock { get; set; }
        static Mock<Func<IWardenCheckResult, Task>> OnFailureAsyncMock { get; set; }

        Establish context = () =>
        {
            OnFailureMock = new Mock<Action<IWardenCheckResult>>();
            OnFailureAsyncMock = new Mock<Func<IWardenCheckResult, Task>>();
            WatcherMock = new Mock<IWatcher>();
            WatcherMock.SetupGet(x => x.Name).Returns("Watcher mock");
            WatcherMock.Setup(x => x.ExecuteAsync()).ReturnsAsync(WatcherCheckResult.Create(WatcherMock.Object, false));
            WardenConfiguration = WardenConfiguration
                .Create()
                .AddWatcher(WatcherMock.Object, hooks =>
                {
                    hooks.OnFailure(result => OnFailureMock.Object(result));
                    hooks.OnFailureAsync(result => OnFailureAsyncMock.Object(result));
                })
                .WithMinimalInterval()
                .RunOnlyOnce()
                .Build();
            Warden = WardenInstance.Create(WardenConfiguration);
        };

        Because of = async () => await Warden.StartAsync().Await().AsTask;

        It should_invoke_execute_async_method_only_once = () => WatcherMock.Verify(x => x.ExecuteAsync(), Times.Once);
        It should_invoke_on_failure_hook_only_once = () => OnFailureMock.Verify(x => x(Moq.It.IsAny<IWardenCheckResult>()), Times.Once);
        It should_invoke_on_failure_async_hook_only_once = () => OnFailureAsyncMock.Verify(x => x(Moq.It.IsAny<IWardenCheckResult>()), Times.Once);
    }

    [Subject("Warden execution with watcher")]
    public class when_running_one_iteration_and_processing_watcher_hooks_for_exception : Warden_specs
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
            WardenConfiguration = WardenConfiguration
                .Create()
                .AddWatcher(WatcherMock.Object, hooks =>
                {
                    hooks.OnError(exception => OnErrorMock.Object(exception));
                    hooks.OnErrorAsync(exception => OnErrorAsyncMock.Object(exception));
                })
                .WithMinimalInterval()
                .RunOnlyOnce()
                .Build();
            Warden = WardenInstance.Create(WardenConfiguration);
        };

        Because of = async () => await Warden.StartAsync().Await().AsTask;

        It should_invoke_execute_async_method_only_once = () => WatcherMock.Verify(x => x.ExecuteAsync(), Times.Once);
        It should_invoke_on_error_hook_only_once = () => OnErrorMock.Verify(x => x(Moq.It.IsAny<Exception>()), Times.Once);
        It should_invoke_on_error_async_hook_only_once = () => OnErrorAsyncMock.Verify(x => x(Moq.It.IsAny<Exception>()), Times.Once);
    }

    [Subject("Warden execution with watcher")]
    public class when_running_ten_iterations_and_processing_on_first_failure_watcher_hooks_for_invalid_result : Warden_specs
    {
        static Mock<IWatcher> WatcherMock { get; set; }
        static Mock<Action<IWardenCheckResult>> OnFirstFailureMock { get; set; }
        static Mock<Func<IWardenCheckResult, Task>> OnFirstFailureAsyncMock { get; set; }
        static int IterationsCount { get; set; }

        Establish context = () =>
        {
            IterationsCount = 10;
            OnFirstFailureMock = new Mock<Action<IWardenCheckResult>>();
            OnFirstFailureAsyncMock = new Mock<Func<IWardenCheckResult, Task>>();
            WatcherMock = new Mock<IWatcher>();
            WatcherMock.SetupGet(x => x.Name).Returns("Watcher mock");
            WatcherMock.Setup(x => x.ExecuteAsync()).ReturnsAsync(WatcherCheckResult.Create(WatcherMock.Object, false));
            WardenConfiguration = WardenConfiguration
                .Create()
                .AddWatcher(WatcherMock.Object, hooks =>
                {
                    hooks.OnFirstFailure(result => OnFirstFailureMock.Object(result));
                    hooks.OnFirstFailureAsync(result => OnFirstFailureAsyncMock.Object(result));
                })
                .WithMinimalInterval()
                .SetIterationsCount(IterationsCount)
                .Build();
            Warden = WardenInstance.Create(WardenConfiguration);
        };

        Because of = async () => await Warden.StartAsync().Await().AsTask;

        It should_invoke_execute_async_method_ten_times = () => WatcherMock.Verify(x => x.ExecuteAsync(), Times.Exactly(IterationsCount));
        It should_invoke_on_first_failure_hook_only_once = () => OnFirstFailureMock.Verify(x => x(Moq.It.IsAny<IWardenCheckResult>()), Times.Once);
        It should_invoke_on_first_failure_async_hook_only_once = () => OnFirstFailureAsyncMock.Verify(x => x(Moq.It.IsAny<IWardenCheckResult>()), Times.Once);
    }

    [Subject("Warden execution with watcher")]
    public class when_running_ten_iterations_and_processing_on_first_error_watcher_hooks_for_exception : Warden_specs
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
            WardenConfiguration = WardenConfiguration
                .Create()
                .AddWatcher(WatcherMock.Object, hooks =>
                {
                    hooks.OnFirstError(result => OnFirstErrorMock.Object(result));
                    hooks.OnFirstErrorAsync(result => OnFirstErrorAsyncMock.Object(result));
                })
                .WithMinimalInterval()
                .SetIterationsCount(IterationsCount)
                .Build();
            Warden = WardenInstance.Create(WardenConfiguration);
        };

        Because of = async () => await Warden.StartAsync().Await().AsTask;

        It should_invoke_execute_async_method_ten_times = () => WatcherMock.Verify(x => x.ExecuteAsync(), Times.Exactly(IterationsCount));
        It should_invoke_on_first_error_hook_only_once = () => OnFirstErrorMock.Verify(x => x(Moq.It.IsAny<Exception>()), Times.Once);
        It should_invoke_on_first_error_async_hook_only_once = () => OnFirstErrorAsyncMock.Verify(x => x(Moq.It.IsAny<Exception>()), Times.Once);
    }

    [Subject("Warden execution with watcher")]
    [Ignore("Not sure yet how to implement that test")]
    public class when_running_ten_iterations_and_processing_on_first_success_watcher_hooks_for_valid_result : Warden_specs
    {
        static Mock<IWatcher> WatcherMock { get; set; }
        static Mock<Action<IWardenCheckResult>> OnFirstSuccessMock { get; set; }
        static Mock<Func<IWardenCheckResult, Task>> OnFirstSuccessAsyncMock { get; set; }
        static int IterationsCount { get; set; }

        Establish context = () =>
        {
            IterationsCount = 10;
            OnFirstSuccessMock = new Mock<Action<IWardenCheckResult>>();
            OnFirstSuccessAsyncMock = new Mock<Func<IWardenCheckResult, Task>>();
            WatcherMock = new Mock<IWatcher>();
            WatcherMock.SetupGet(x => x.Name).Returns("Watcher mock");
            WatcherMock.Setup(x => x.ExecuteAsync()).ReturnsAsync(WatcherCheckResult.Create(WatcherMock.Object, true));
            WardenConfiguration = WardenConfiguration
                .Create()
                .AddWatcher(WatcherMock.Object, hooks =>
                {
                    hooks.OnFirstSuccess(result => OnFirstSuccessMock.Object(result));
                    hooks.OnFirstSuccessAsync(result => OnFirstSuccessAsyncMock.Object(result));
                })
                .WithMinimalInterval()
                .SetIterationsCount(IterationsCount)
                .Build();
            Warden = WardenInstance.Create(WardenConfiguration);
        };

        Because of = async () => await Warden.StartAsync().Await().AsTask;

        It should_invoke_execute_async_method_ten_times = () => WatcherMock.Verify(x => x.ExecuteAsync(), Times.Exactly(IterationsCount));
        It should_invoke_on_first_success_hook_only_once = () => OnFirstSuccessMock.Verify(x => x(Moq.It.IsAny<IWardenCheckResult>()), Times.Once);
        It should_invoke_on_first_success_async_hook_only_once = () => OnFirstSuccessAsyncMock.Verify(x => x(Moq.It.IsAny<IWardenCheckResult>()), Times.Once);
    }

    [Subject("Warden execution with watcher")]
    public class when_using_different_intervals_for_watchers : Warden_specs
    {
        static Mock<IWatcher> FirstWatcherMock { get; set; }
        static Mock<IWatcher> SecondWatcherMock { get; set; }
        static Mock<IWatcher> ThirdWatcherMock { get; set; }
        static TimeSpan FirstWatcherInterval;
        static TimeSpan SecondWatcherInterval;
        static TimeSpan ThirdWatcherInterval;

        Establish context = () =>
        {
            FirstWatcherMock = new Mock<IWatcher>();
            SecondWatcherMock = new Mock<IWatcher>();
            ThirdWatcherMock = new Mock<IWatcher>();
            FirstWatcherMock.SetupGet(x => x.Name).Returns("First watcher mock");
            SecondWatcherMock.SetupGet(x => x.Name).Returns("Second watcher mock");
            ThirdWatcherMock.SetupGet(x => x.Name).Returns("Third watcher mock");
            FirstWatcherInterval = TimeSpan.FromMilliseconds(10);
            SecondWatcherInterval = TimeSpan.FromMilliseconds(100);
            ThirdWatcherInterval = TimeSpan.FromSeconds(1);
            FirstWatcherMock.Setup(x => x.ExecuteAsync()).ReturnsAsync(WatcherCheckResult.Create(FirstWatcherMock.Object, true));
            SecondWatcherMock.Setup(x => x.ExecuteAsync()).ReturnsAsync(WatcherCheckResult.Create(SecondWatcherMock.Object, true));
            ThirdWatcherMock.Setup(x => x.ExecuteAsync()).ReturnsAsync(WatcherCheckResult.Create(ThirdWatcherMock.Object, true));
            WardenConfiguration = WardenConfiguration
                .Create()
                .AddWatcher(FirstWatcherMock.Object, interval: FirstWatcherInterval)
                .AddWatcher(SecondWatcherMock.Object, interval: SecondWatcherInterval)
                .AddWatcher(ThirdWatcherMock.Object, interval: ThirdWatcherInterval)
                .RunOnlyOnce()
                .Build();
            Warden = WardenInstance.Create(WardenConfiguration);
        };

        Because of = async () => await Warden.StartAsync().Await().AsTask;

        It should_invoke_execute_async_method_hundred_times_for_first_watcher = () => FirstWatcherMock.Verify(x => x.ExecuteAsync(), Times.Exactly(100));
        It should_invoke_execute_async_method_ten_times_for_second_watcher = () => SecondWatcherMock.Verify(x => x.ExecuteAsync(), Times.Exactly(10));
        It should_invoke_execute_async_method_once_for_third_watcher = () => ThirdWatcherMock.Verify(x => x.ExecuteAsync(), Times.Exactly(1));
    }
}