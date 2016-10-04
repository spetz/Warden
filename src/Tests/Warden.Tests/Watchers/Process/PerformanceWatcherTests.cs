using System;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Warden.Watchers;
using Warden.Watchers.Process;
using Machine.Specifications;
using It = Machine.Specifications.It;

namespace Warden.Tests.Watchers.Process
{
    public class ProcessWatcher_specs
    {
        protected static ProcessWatcher Watcher { get; set; }
        protected static ProcessWatcherConfiguration Configuration { get; set; }
        protected static IWatcherCheckResult CheckResult { get; set; }
        protected static ProcessWatcherCheckResult ProcessCheckResult { get; set; }
        protected static Exception Exception { get; set; }
        protected static int ProcessId { get; set; } = 1;
        protected static string ProcessName { get; set; } = "Process";
        protected static bool ProcessResponding { get; set; } = true;
        protected static bool ProcessExists { get; set; } = true;
    }

    [Subject("Process watcher initialization")]
    public class when_initializing_without_configuration : ProcessWatcher_specs
    {
        Establish context = () => Configuration = null;

        Because of = () => Exception = Catch.Exception((() => Watcher = ProcessWatcher.Create("test", Configuration)));

        It should_fail = () => Exception.Should().BeOfType<ArgumentNullException>();
        It should_have_a_specific_reason = () => Exception.Message.Should().Contain("Process Watcher configuration has not been provided.");
    }

    [Subject("Process watcher execution")]
    public class when_invoking_execute_async_method_with_valid_configuration : ProcessWatcher_specs
    {
        static Mock<IProcessService> ProcessMock;
        static ProcessInfo ProcessInfo;

        Establish context = () =>
        {
            ProcessMock = new Mock<IProcessService>();
            ProcessInfo = ProcessInfo.Create(ProcessId, ProcessName, null, ProcessExists, ProcessResponding);
            ProcessMock.Setup(x =>
                x.GetProcessInfoAsync(Moq.It.IsAny<string>(), null))
                .ReturnsAsync(ProcessInfo);

            Configuration = ProcessWatcherConfiguration
                .Create(ProcessName)
                .WithProcessServiceProvider(() => ProcessMock.Object)
                .Build();
            Watcher = ProcessWatcher.Create("Process watcher", Configuration);
        };

        Because of = async () =>
        {
            CheckResult = await Watcher.ExecuteAsync().Await().AsTask;
            ProcessCheckResult = CheckResult as ProcessWatcherCheckResult;
        };

        It should_invoke_process_get_process_info_async_method_only_once = () =>
            ProcessMock.Verify(x => x.GetProcessInfoAsync(Moq.It.IsAny<string>(), null), Times.Once);

        It should_have_valid_check_result = () => CheckResult.IsValid.Should().BeTrue();
        It should_have_check_result_of_type_process = () => ProcessCheckResult.Should().NotBeNull();

        It should_have_set_values_in_process_check_result = () =>
        {
            ProcessCheckResult.WatcherName.Should().NotBeEmpty();
            ProcessCheckResult.WatcherType.Should().NotBeNull();
            ProcessCheckResult.ProcessInfo.Should().NotBeNull();
            ProcessCheckResult.ProcessInfo.Id.ShouldBeEquivalentTo(ProcessId);
            ProcessCheckResult.ProcessInfo.Name.ShouldBeEquivalentTo(ProcessName);
            ProcessCheckResult.ProcessInfo.Exists.ShouldBeEquivalentTo(ProcessExists);
            ProcessCheckResult.ProcessInfo.Responding.ShouldBeEquivalentTo(ProcessResponding);
        };
    }

    [Subject("Process watcher execution")]
    public class when_invoking_execute_async_method_with_skip_Responding_validation_enabled : ProcessWatcher_specs
    {
        static Mock<IProcessService> ProcessMock;
        static ProcessInfo ProcessInfo;

        Establish context = () =>
        {
            ProcessMock = new Mock<IProcessService>();
            ProcessInfo = ProcessInfo.Create(ProcessId, ProcessName, null, ProcessExists, ProcessResponding);
            ProcessMock.Setup(x =>
                x.GetProcessInfoAsync(Moq.It.IsAny<string>(), null))
                .ReturnsAsync(ProcessInfo);

            Configuration = ProcessWatcherConfiguration
                .Create(ProcessName)
                .DoesNotHaveToBeResponding()
                .WithProcessServiceProvider(() => ProcessMock.Object)
                .Build();
            Watcher = ProcessWatcher.Create("Process watcher", Configuration);
        };

        Because of = async () =>
        {
            CheckResult = await Watcher.ExecuteAsync().Await().AsTask;
            ProcessCheckResult = CheckResult as ProcessWatcherCheckResult;
        };

        It should_invoke_process_get_process_info_async_method_only_once = () =>
            ProcessMock.Verify(x => x.GetProcessInfoAsync(Moq.It.IsAny<string>(), null), Times.Once);

        It should_have_valid_check_result = () => CheckResult.IsValid.Should().BeTrue();
        It should_have_check_result_of_type_process = () => ProcessCheckResult.Should().NotBeNull();

        It should_have_set_values_in_process_check_result = () =>
        {
            ProcessCheckResult.WatcherName.Should().NotBeEmpty();
            ProcessCheckResult.WatcherType.Should().NotBeNull();
            ProcessCheckResult.ProcessInfo.Should().NotBeNull();
            ProcessCheckResult.ProcessInfo.Id.ShouldBeEquivalentTo(ProcessId);
            ProcessCheckResult.ProcessInfo.Name.ShouldBeEquivalentTo(ProcessName);
            ProcessCheckResult.ProcessInfo.Exists.ShouldBeEquivalentTo(ProcessExists);
            ProcessCheckResult.ProcessInfo.Responding.ShouldBeEquivalentTo(ProcessResponding);
        };
    }

    [Subject("Process watcher execution")]
    public class when_invoking_ensure_predicate_that_is_valid : ProcessWatcher_specs
    {
        static Mock<IProcessService> ProcessMock;
        static ProcessInfo ProcessInfo;

        Establish context = () =>
        {
            ProcessMock = new Mock<IProcessService>();
            ProcessInfo = ProcessInfo.Create(ProcessId, ProcessName, null, ProcessExists, ProcessResponding);
            ProcessMock.Setup(x =>
                x.GetProcessInfoAsync(Moq.It.IsAny<string>(), null))
                .ReturnsAsync(ProcessInfo);

            Configuration = ProcessWatcherConfiguration
                .Create(ProcessName)
                .EnsureThat(info => info.Id == ProcessId
                                    && info.Name == ProcessName &&
                                    info.Exists == ProcessExists &&
                                    info.Responding == ProcessResponding)
                .WithProcessServiceProvider(() => ProcessMock.Object)
                .Build();
            Watcher = ProcessWatcher.Create("Process watcher", Configuration);
        };

        Because of = async () =>
        {
            CheckResult = await Watcher.ExecuteAsync().Await().AsTask;
            ProcessCheckResult = CheckResult as ProcessWatcherCheckResult;
        };

        It should_invoke_process_get_process_info_async_method_only_once = () =>
            ProcessMock.Verify(x => x.GetProcessInfoAsync(Moq.It.IsAny<string>(), null), Times.Once);

        It should_have_valid_check_result = () => CheckResult.IsValid.Should().BeTrue();
        It should_have_check_result_of_type_process = () => ProcessCheckResult.Should().NotBeNull();

        It should_have_set_values_in_process_check_result = () =>
        {
            ProcessCheckResult.WatcherName.Should().NotBeEmpty();
            ProcessCheckResult.WatcherType.Should().NotBeNull();
            ProcessCheckResult.ProcessInfo.Should().NotBeNull();
            ProcessCheckResult.ProcessInfo.Id.ShouldBeEquivalentTo(ProcessId);
            ProcessCheckResult.ProcessInfo.Name.ShouldBeEquivalentTo(ProcessName);
            ProcessCheckResult.ProcessInfo.Exists.ShouldBeEquivalentTo(ProcessExists);
            ProcessCheckResult.ProcessInfo.Responding.ShouldBeEquivalentTo(ProcessResponding);
        };
    }

    [Subject("Process watcher execution")]
    public class when_invoking_ensure_async_predicate_that_is_valid : ProcessWatcher_specs
    {
        static Mock<IProcessService> ProcessMock;
        static ProcessInfo ProcessInfo;

        Establish context = () =>
        {
            ProcessMock = new Mock<IProcessService>();
            ProcessInfo = ProcessInfo.Create(ProcessId, ProcessName, null, ProcessExists, ProcessResponding);
            ProcessMock.Setup(x =>
                x.GetProcessInfoAsync(Moq.It.IsAny<string>(), null))
                .ReturnsAsync(ProcessInfo);

            Configuration = ProcessWatcherConfiguration
                .Create(ProcessName)
                .EnsureThatAsync(info => Task.Factory.StartNew(() => info.Id == ProcessId
                                                                     && info.Name == ProcessName &&
                                                                     info.Exists == ProcessExists &&
                                                                     info.Responding == ProcessResponding))
                .WithProcessServiceProvider(() => ProcessMock.Object)
                .Build();
            Watcher = ProcessWatcher.Create("Process watcher", Configuration);
        };

        Because of = async () =>
        {
            CheckResult = await Watcher.ExecuteAsync().Await().AsTask;
            ProcessCheckResult = CheckResult as ProcessWatcherCheckResult;
        };

        It should_invoke_process_get_process_info_async_method_only_once = () =>
            ProcessMock.Verify(x => x.GetProcessInfoAsync(Moq.It.IsAny<string>(), null), Times.Once);

        It should_have_valid_check_result = () => CheckResult.IsValid.Should().BeTrue();
        It should_have_check_result_of_type_process = () => ProcessCheckResult.Should().NotBeNull();

        It should_have_set_values_in_process_check_result = () =>
        {
            ProcessCheckResult.WatcherName.Should().NotBeEmpty();
            ProcessCheckResult.WatcherType.Should().NotBeNull();
            ProcessCheckResult.ProcessInfo.Should().NotBeNull();
            ProcessCheckResult.ProcessInfo.Id.ShouldBeEquivalentTo(ProcessId);
            ProcessCheckResult.ProcessInfo.Name.ShouldBeEquivalentTo(ProcessName);
            ProcessCheckResult.ProcessInfo.Exists.ShouldBeEquivalentTo(ProcessExists);
            ProcessCheckResult.ProcessInfo.Responding.ShouldBeEquivalentTo(ProcessResponding);
        };
    }

    [Subject("Process watcher execution")]
    public class when_invoking_ensure_predicate_that_is_invalid : ProcessWatcher_specs
    {
        static Mock<IProcessService> ProcessMock;
        static ProcessInfo ProcessInfo;

        Establish context = () =>
        {
            ProcessMock = new Mock<IProcessService>();
            ProcessInfo = ProcessInfo.Create(ProcessId, ProcessName, null, false, false);
            ProcessMock.Setup(x =>
                x.GetProcessInfoAsync(Moq.It.IsAny<string>(), null))
                .ReturnsAsync(ProcessInfo);
            Configuration = ProcessWatcherConfiguration
                .Create(ProcessName)
                .EnsureThat(info => info.Id == ProcessId
                                    && info.Name == ProcessName &&
                                    info.Exists &&
                                    info.Responding)
                .WithProcessServiceProvider(() => ProcessMock.Object)
                .Build();
            Watcher = ProcessWatcher.Create("Process watcher", Configuration);
        };

        Because of = async () =>
        {
            CheckResult = await Watcher.ExecuteAsync().Await().AsTask;
            ProcessCheckResult = CheckResult as ProcessWatcherCheckResult;
        };

        It should_invoke_process_get_process_info_async_method_only_once = () =>
            ProcessMock.Verify(x => x.GetProcessInfoAsync(Moq.It.IsAny<string>(), null), Times.Once);

        It should_have_invalid_check_result = () => CheckResult.IsValid.Should().BeFalse();
        It should_have_check_result_of_type_process = () => ProcessCheckResult.Should().NotBeNull();

        It should_have_set_values_in_process_check_result = () =>
        {
            ProcessCheckResult.WatcherName.Should().NotBeEmpty();
            ProcessCheckResult.WatcherType.Should().NotBeNull();
            ProcessCheckResult.ProcessInfo.Should().NotBeNull();
            ProcessCheckResult.ProcessInfo.Id.ShouldBeEquivalentTo(ProcessId);
            ProcessCheckResult.ProcessInfo.Name.ShouldBeEquivalentTo(ProcessName);
            ProcessCheckResult.ProcessInfo.Exists.ShouldBeEquivalentTo(false);
            ProcessCheckResult.ProcessInfo.Responding.ShouldBeEquivalentTo(false);
        };
    }

    [Subject("Process watcher execution")]
    public class when_invoking_ensure_async_predicate_that_is_invalid : ProcessWatcher_specs
    {
        static Mock<IProcessService> ProcessMock;
        static ProcessInfo ProcessInfo;

        Establish context = () =>
        {
            ProcessMock = new Mock<IProcessService>();
            ProcessInfo = ProcessInfo.Create(ProcessId, ProcessName, null, false, false);
            ProcessMock.Setup(x =>
                x.GetProcessInfoAsync(Moq.It.IsAny<string>(), null))
                .ReturnsAsync(ProcessInfo);
            Configuration = ProcessWatcherConfiguration
                .Create(ProcessName)
                .EnsureThatAsync(info => Task.Factory.StartNew(() => info.Id == ProcessId
                                                                     && info.Name == ProcessName &&
                                                                     info.Exists &&
                                                                     info.Responding))
                .WithProcessServiceProvider(() => ProcessMock.Object)
                .Build();
            Watcher = ProcessWatcher.Create("Process watcher", Configuration);
        };

        Because of = async () =>
        {
            CheckResult = await Watcher.ExecuteAsync().Await().AsTask;
            ProcessCheckResult = CheckResult as ProcessWatcherCheckResult;
        };

        It should_invoke_process_get_process_info_async_method_only_once = () =>
            ProcessMock.Verify(x => x.GetProcessInfoAsync(Moq.It.IsAny<string>(), null), Times.Once);

        It should_have_invalid_check_result = () => CheckResult.IsValid.Should().BeFalse();
        It should_have_check_result_of_type_process = () => ProcessCheckResult.Should().NotBeNull();

        It should_have_set_values_in_process_check_result = () =>
        {
            ProcessCheckResult.WatcherName.Should().NotBeEmpty();
            ProcessCheckResult.WatcherType.Should().NotBeNull();
            ProcessCheckResult.ProcessInfo.Should().NotBeNull();
            ProcessCheckResult.ProcessInfo.Id.ShouldBeEquivalentTo(ProcessId);
            ProcessCheckResult.ProcessInfo.Name.ShouldBeEquivalentTo(ProcessName);
            ProcessCheckResult.ProcessInfo.Exists.ShouldBeEquivalentTo(false);
            ProcessCheckResult.ProcessInfo.Responding.ShouldBeEquivalentTo(false);
        };
    }
}