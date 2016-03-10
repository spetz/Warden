using System;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Sentry.Core;

namespace Sentry.Tests.Unit
{
    [Specification]
    public class Sentry_specs : SpecificationBase
    {
        protected ISentry Sentry { get; set; }
        protected SentryConfiguration SentryConfiguration { get; set; }
    }

    [Specification]
    public class when_sentry_is_being_initialized_without_configuration : Sentry_specs
    {
        protected override async Task EstablishContext()
        {
            await base.EstablishContext();
            ExceptionExpected = true;
        }

        protected override async Task BecauseOf()
        {
            await base.BecauseOf();
            SentryConfiguration = null;
            Sentry = new Sentry(SentryConfiguration);
        }

        [Then]
        public void then_exception_should_be_thrown()
        {
            ExceptionThrown.Should().BeAssignableTo<ArgumentNullException>();
            ExceptionThrown.Message.Should().StartWithEquivalent("Sentry configuration has not been provided.");
        }
    }

    [Specification]
    public class when_sentry_is_being_initialized_with_configuration : Sentry_specs
    {
        protected override async Task EstablishContext()
        {
            await base.EstablishContext();
        }

        protected override async Task BecauseOf()
        {
            await base.BecauseOf();
            SentryConfiguration = SentryConfiguration.Create().Build();
            Sentry = new Sentry(SentryConfiguration);
        }

        [Then]
        public void then_new_sentry_instance_should_be_created()
        {
            Sentry.Should().NotBeNull();
        }
    }

    [Specification]
    public class when_sentry_has_a_watcher_and_is_setup_to_run_only_once : Sentry_specs
    {
        protected Mock<IWatcher> WatcherMock { get; set; }

        protected override async Task EstablishContext()
        {
            await base.EstablishContext();
            WatcherMock = new Mock<IWatcher>();
            WatcherMock.SetupGet(x => x.Name).Returns("Watcher mock");
            SentryConfiguration = SentryConfiguration
                .Create()
                .AddWatcher(WatcherMock.Object)
                .RunOnlyOnce()
                .Build();
            Sentry = new Sentry(SentryConfiguration);
        }

        protected override async Task BecauseOf()
        {
            await base.BecauseOf();
            await Sentry.StartAsync();
        }

        [Then]
        public void then_watcher_execute_async_method_should_be_executed_only_once()
        {
            WatcherMock.Verify(x => x.ExecuteAsync(), Times.Once);
        }
    }

    [Specification]
    public class when_sentry_has_a_watcher_and_is_setup_to_run_ten_thousand_times : Sentry_specs
    {
        protected Mock<IWatcher> WatcherMock { get; set; }
        protected int IterationsCount { get; set; }

        protected override async Task EstablishContext()
        {
            await base.EstablishContext();
            IterationsCount = 10000;
            WatcherMock = new Mock<IWatcher>();
            WatcherMock.SetupGet(x => x.Name).Returns("Watcher mock");
            SentryConfiguration = SentryConfiguration
                .Create()
                .AddWatcher(WatcherMock.Object)
                .SetIterationsCount(IterationsCount)
                .WithoutIterationDelay()
                .Build();
            Sentry = new Sentry(SentryConfiguration);
        }

        protected override async Task BecauseOf()
        {
            await base.BecauseOf();
            await Sentry.StartAsync();
        }

        [Then]
        public void then_watcher_execute_async_method_should_be_executed_ten_thousand_times()
        {
            WatcherMock.Verify(x => x.ExecuteAsync(), Times.Exactly(IterationsCount));
        }
    }

    [Specification]
    public class when_sentry_has_a_watcher_and_hooks_and_is_setup_to_run_ten_thousand_times : Sentry_specs
    {
        protected Mock<IWatcher> WatcherMock { get; set; }
        protected Mock<Action<IWatcherCheck>> OnStartMock { get; set; }
        protected Mock<Func<IWatcherCheck, Task>> OnStartAsyncMock { get; set; }
        protected Mock<Action<ISentryCheckResult>> OnSuccessMock { get; set; }
        protected Mock<Func<ISentryCheckResult, Task>> OnSuccessAsyncMock { get; set; }
        protected Mock<Action<ISentryCheckResult>> OnCompletedMock { get; set; }
        protected Mock<Func<ISentryCheckResult, Task>> OnCompletedAsyncMock { get; set; }
        protected int IterationsCount { get; set; }

        protected override async Task EstablishContext()
        {
            await base.EstablishContext();
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
        }

        protected override async Task BecauseOf()
        {
            await base.BecauseOf();
            await Sentry.StartAsync();
        }

        [Then]
        public void then_watcher_execute_async_method_and_hooks_should_be_executed_one_thousand_times()
        {
            WatcherMock.Verify(x => x.ExecuteAsync(), Times.Exactly(IterationsCount));
            OnStartMock.Verify(x => x(It.IsAny<IWatcherCheck>()), Times.Exactly(IterationsCount));
            OnStartAsyncMock.Verify(x => x(It.IsAny<IWatcherCheck>()), Times.Exactly(IterationsCount));
            OnSuccessMock.Verify(x => x(It.IsAny<ISentryCheckResult>()), Times.Exactly(IterationsCount));
            OnSuccessAsyncMock.Verify(x => x(It.IsAny<ISentryCheckResult>()), Times.Exactly(IterationsCount));
            OnCompletedMock.Verify(x => x(It.IsAny<ISentryCheckResult>()), Times.Exactly(IterationsCount));
            OnCompletedAsyncMock.Verify(x => x(It.IsAny<ISentryCheckResult>()), Times.Exactly(IterationsCount));
        }
    }

    public class when_sentry_has_iteration_hooks_setup : Sentry_specs
    {
        protected Mock<IWatcher> WatcherMock { get; set; }
        protected Mock<Action> OnStartMock { get; set; }
        protected Mock<Func<Task>> OnStartAsyncMock { get; set; }
        protected Mock<Action<long>> OnIterationStartMock { get; set; }
        protected Mock<Func<long,Task>> OnIterationStartAsyncMock { get; set; }
        protected Mock<Action<ISentryIteration>> OnIterationCompletedMock { get; set; }
        protected Mock<Func<ISentryIteration, Task>> OnIterationCompletedAsyncMock { get; set; }

        protected override async Task EstablishContext()
        {
            await base.EstablishContext();
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
        }

        protected override async Task BecauseOf()
        {
            await base.BecauseOf();
            await Sentry.StartAsync();
        }

        [Then]
        public void then_watcher_hook_methods_should_be_executed_only_once()
        {
            WatcherMock.Verify(x => x.ExecuteAsync(), Times.Once);
            OnStartMock.Verify(x => x(), Times.Once);
            OnStartAsyncMock.Verify(x => x(), Times.Once);
            OnIterationStartMock.Verify(x => x(It.IsAny<long>()), Times.Once);
            OnIterationStartAsyncMock.Verify(x => x(It.IsAny<long>()), Times.Once);
            OnIterationCompletedMock.Verify(x => x(It.IsAny<ISentryIteration>()), Times.Once);
            OnIterationCompletedAsyncMock.Verify(x => x(It.IsAny<ISentryIteration>()), Times.Once);
        }
    }

    public class when_sentry_has_a_valid_watcher_that_returns_valid_result_and_has_hooks_setup : Sentry_specs
    {
        protected Mock<IWatcher> WatcherMock { get; set; }
        protected Mock<Action<IWatcherCheck>> OnStartMock { get; set; }
        protected Mock<Func<IWatcherCheck, Task>> OnStartAsyncMock { get; set; }
        protected Mock<Action<ISentryCheckResult>> OnSuccessMock { get; set; }
        protected Mock<Func<ISentryCheckResult, Task>> OnSuccessAsyncMock { get; set; }
        protected Mock<Action<ISentryCheckResult>> OnCompletedMock { get; set; }
        protected Mock<Func<ISentryCheckResult, Task>> OnCompletedAsyncMock { get; set; }

        protected override async Task EstablishContext()
        {
            await base.EstablishContext();
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
        }

        protected override async Task BecauseOf()
        {
            await base.BecauseOf();
            await Sentry.StartAsync();
        }

        [Then]
        public void then_watcher_hook_methods_should_be_executed_only_once()
        {
            WatcherMock.Verify(x => x.ExecuteAsync(), Times.Once);
            OnStartMock.Verify(x => x(It.IsAny<IWatcherCheck>()), Times.Once);
            OnStartAsyncMock.Verify(x => x(It.IsAny<IWatcherCheck>()), Times.Once);
            OnSuccessMock.Verify(x => x(It.IsAny<ISentryCheckResult>()), Times.Once);
            OnSuccessAsyncMock.Verify(x => x(It.IsAny<ISentryCheckResult>()), Times.Once);
            OnCompletedMock.Verify(x => x(It.IsAny<ISentryCheckResult>()), Times.Once);
            OnCompletedAsyncMock.Verify(x => x(It.IsAny<ISentryCheckResult>()), Times.Once);
        }
    }

    public class when_sentry_has_a_valid_watcher_that_returns_valid_result_and_has_global_watcher_hooks_setup : Sentry_specs
    {
        protected Mock<IWatcher> WatcherMock { get; set; }
        protected Mock<Action<IWatcherCheck>> OnStartMock { get; set; }
        protected Mock<Func<IWatcherCheck, Task>> OnStartAsyncMock { get; set; }
        protected Mock<Action<ISentryCheckResult>> OnSuccessMock { get; set; }
        protected Mock<Func<ISentryCheckResult, Task>> OnSuccessAsyncMock { get; set; }
        protected Mock<Action<ISentryCheckResult>> OnCompletedMock { get; set; }
        protected Mock<Func<ISentryCheckResult, Task>> OnCompletedAsyncMock { get; set; }

        protected override async Task EstablishContext()
        {
            await base.EstablishContext();
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
        }

        protected override async Task BecauseOf()
        {
            await base.BecauseOf();
            await Sentry.StartAsync();
        }

        [Then]
        public void then_watcher_hook_methods_should_be_executed_only_once()
        {
            WatcherMock.Verify(x => x.ExecuteAsync(), Times.Once);
            OnStartMock.Verify(x => x(It.IsAny<IWatcherCheck>()), Times.Once);
            OnStartAsyncMock.Verify(x => x(It.IsAny<IWatcherCheck>()), Times.Once);
            OnSuccessMock.Verify(x => x(It.IsAny<ISentryCheckResult>()), Times.Once);
            OnSuccessAsyncMock.Verify(x => x(It.IsAny<ISentryCheckResult>()), Times.Once);
            OnCompletedMock.Verify(x => x(It.IsAny<ISentryCheckResult>()), Times.Once);
            OnCompletedAsyncMock.Verify(x => x(It.IsAny<ISentryCheckResult>()), Times.Once);
        }
    }

    public class when_sentry_has_a_valid_watcher_that_returns_invalid_result_and_has_hooks_setup : Sentry_specs
    {
        protected Mock<IWatcher> WatcherMock { get; set; }
        protected Mock<Action<ISentryCheckResult>> OnFailureMock { get; set; }
        protected Mock<Func<ISentryCheckResult, Task>> OnFailureAsyncMock { get; set; }

        protected override async Task EstablishContext()
        {
            await base.EstablishContext();
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
        }

        protected override async Task BecauseOf()
        {
            await base.BecauseOf();
            await Sentry.StartAsync();
        }

        [Then]
        public void then_watcher_hooks_methods_should_be_executed_only_once()
        {
            WatcherMock.Verify(x => x.ExecuteAsync(), Times.Once);
            OnFailureMock.Verify(x => x(It.IsAny<ISentryCheckResult>()), Times.Once);
            OnFailureAsyncMock.Verify(x => x(It.IsAny<ISentryCheckResult>()), Times.Once);
        }
    }

    public class when_sentry_has_a_valid_watcher_that_throws_an_exception_and_has_hooks_setup : Sentry_specs
    {
        protected Mock<IWatcher> WatcherMock { get; set; }
        protected Mock<Action<Exception>> OnErrorMock { get; set; }
        protected Mock<Func<Exception, Task>> OnErrorAsyncMock { get; set; }

        protected override async Task EstablishContext()
        {
            await base.EstablishContext();
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
        }

        protected override async Task BecauseOf()
        {
            await base.BecauseOf();
            await Sentry.StartAsync();
        }

        [Then]
        public void then_watcher_hooks_method_should_be_executed_only_once()
        {
            WatcherMock.Verify(x => x.ExecuteAsync(), Times.Once);
            OnErrorMock.Verify(x => x(It.IsAny<Exception>()), Times.Once);
            OnErrorAsyncMock.Verify(x => x(It.IsAny<Exception>()), Times.Once);
        }
    }

    public class when_sentry_has_an_invalid_watcher_and_on_first_failure_hook_and_is_setup_to_run_ten_times : Sentry_specs
    {
        protected Mock<IWatcher> WatcherMock { get; set; }
        protected Mock<Action<ISentryCheckResult>> OnFirstFailureMock { get; set; }
        protected Mock<Func<ISentryCheckResult, Task>> OnFirstFailureAsyncMock { get; set; }
        protected int IterationsCount { get; set; }

        protected override async Task EstablishContext()
        {
            await base.EstablishContext();
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
        }

        protected override async Task BecauseOf()
        {
            await base.BecauseOf();
            await Sentry.StartAsync();
        }

        [Then]
        public void then_watcher_on_first_failure_hook_methods_should_be_executed_only_once()
        {
            WatcherMock.Verify(x => x.ExecuteAsync(), Times.Exactly(IterationsCount));
            OnFirstFailureMock.Verify(x => x(It.IsAny<ISentryCheckResult>()), Times.Once);
            OnFirstFailureAsyncMock.Verify(x => x(It.IsAny<ISentryCheckResult>()), Times.Once);
        }
    }

    public class when_sentry_has_a_watcher_that_throws_as_exception_and_on_first_failure_hook_and_is_setup_to_run_ten_times: Sentry_specs
    {
        protected Mock<IWatcher> WatcherMock { get; set; }
        protected Mock<Action<Exception>> OnFirstErrorMock { get; set; }
        protected Mock<Func<Exception, Task>> OnFirstErrorAsyncMock { get; set; }
        protected int IterationsCount { get; set; }

        protected override async Task EstablishContext()
        {
            await base.EstablishContext();
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
        }

        protected override async Task BecauseOf()
        {
            await base.BecauseOf();
            await Sentry.StartAsync();
        }

        [Then]
        public void then_watcher_on_first_error_hook_methods_should_be_executed_only_once()
        {
            WatcherMock.Verify(x => x.ExecuteAsync(), Times.Exactly(IterationsCount));
            OnFirstErrorMock.Verify(x => x(It.IsAny<Exception>()), Times.Once);
            OnFirstErrorAsyncMock.Verify(x => x(It.IsAny<Exception>()), Times.Once);
        }

        public class when_sentry_has_a_valid_watcher_and_on_first_success_hook_and_is_setup_to_run_ten_times : Sentry_specs
        {
            protected Mock<IWatcher> WatcherMock { get; set; }
            protected Mock<Action<ISentryCheckResult>> OnFirstSuccessMock { get; set; }
            protected Mock<Func<ISentryCheckResult, Task>> OnFirstSuccessAsyncMock { get; set; }
            protected int IterationsCount { get; set; }

            protected override async Task EstablishContext()
            {
                await base.EstablishContext();
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
                        hooks.OnFirstSuccess(exception => OnFirstSuccessMock.Object(exception));
                        hooks.OnFirstSuccessAsync(exception => OnFirstSuccessAsyncMock.Object(exception));
                    })
                    .WithoutIterationDelay()
                    .SetIterationsCount(IterationsCount)
                    .Build();
                Sentry = new Sentry(SentryConfiguration);
            }

            protected override async Task BecauseOf()
            {
                await base.BecauseOf();
                await Sentry.StartAsync();
            }

            [Then]
            public void then_watcher_on_first_success_hook_methods_should_be_never_executed()
            {
                WatcherMock.Verify(x => x.ExecuteAsync(), Times.Exactly(IterationsCount));
                OnFirstSuccessMock.Verify(x => x(It.IsAny<ISentryCheckResult>()), Times.Never);
                OnFirstSuccessAsyncMock.Verify(x => x(It.IsAny<ISentryCheckResult>()), Times.Never);
            }
        }
    }
}