using System;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Warden.Watchers;
using Warden.Watchers.Performance;

namespace Warden.Tests.Watchers.Performance
{
    public class PerformanceWatcher_specs
    {
        protected static PerformanceWatcher Watcher { get; set; }
        protected static PerformanceWatcherConfiguration Configuration { get; set; }
        protected static IWatcherCheckResult CheckResult { get; set; }
        protected static PerformanceWatcherCheckResult PerformanceCheckResult { get; set; }
        protected static Exception Exception { get; set; }
        protected static int CpuUsage { get; set; } = 10;
        protected static int RamUsage { get; set; } = 1000;
    }

    [Subject("Performance watcher initialization")]
    public class when_initializing_without_configuration : PerformanceWatcher_specs
    {
        Establish context = () => Configuration = null;

        Because of = () => Exception = Catch.Exception((() => Watcher = PerformanceWatcher.Create("test", Configuration)));

        It should_fail = () => Exception.Should().BeOfType<ArgumentNullException>();
        It should_have_a_specific_reason = () => Exception.Message.Should().Contain("Performance Watcher configuration has not been provided.");

        //TODO: Remove when MSpec works with DNX 
        [Test]
        public void RunTest()
        {
            context();
            of();
            should_fail();
            should_have_a_specific_reason();
        }
    }

    [Subject("Performance watcher execution")]
    public class when_invoking_execute_async_method_with_valid_configuration : PerformanceWatcher_specs
    {
        static Mock<IPerformance> PerformanceMock;
        static ResourceUsage ResourceUsage;

        Establish context = () =>
        {
            PerformanceMock = new Mock<IPerformance>();
            ResourceUsage = ResourceUsage.Create(CpuUsage, RamUsage);
            PerformanceMock.Setup(x =>
                x.GetResourceUsageAsync())
                .ReturnsAsync(ResourceUsage);

            Configuration = PerformanceWatcherConfiguration
                .Create()
                .WithPerformanceProvider(() => PerformanceMock.Object)
                .Build();
            Watcher = PerformanceWatcher.Create("Performance watcher", Configuration);
        };

        Because of = async () =>
        {
            CheckResult = await Watcher.ExecuteAsync().Await().AsTask;
            PerformanceCheckResult = CheckResult as PerformanceWatcherCheckResult;
        };

        It should_invoke_performance_get_resource_usage_async_method_only_once = () =>
            PerformanceMock.Verify(x => x.GetResourceUsageAsync(), Times.Once);

        It should_have_valid_check_result = () => CheckResult.IsValid.Should().BeTrue();
        It should_have_check_result_of_type_performance = () => PerformanceCheckResult.Should().NotBeNull();

        It should_have_set_values_in_performance_check_result = () =>
        {
            PerformanceCheckResult.WatcherName.Should().NotBeEmpty();
            PerformanceCheckResult.WatcherType.Should().NotBeNull();
            PerformanceCheckResult.ResourceUsage.Should().NotBeNull();
            PerformanceCheckResult.ResourceUsage.Cpu.ShouldBeEquivalentTo(CpuUsage);
            PerformanceCheckResult.ResourceUsage.Ram.ShouldBeEquivalentTo(RamUsage);
        };

        //TODO: Remove when MSpec works with DNX 
        [Test]
        public void RunTest()
        {
            context();
            of();
            should_invoke_performance_get_resource_usage_async_method_only_once();
            should_have_valid_check_result();
            should_have_check_result_of_type_performance();
            should_have_set_values_in_performance_check_result();
        }
    }

    [Subject("Performance watcher execution")]
    public class when_invoking_ensure_predicate_that_is_valid : PerformanceWatcher_specs
    {
        static Mock<IPerformance> PerformanceMock;
        static ResourceUsage ResourceUsage;

        Establish context = () =>
        {
            PerformanceMock = new Mock<IPerformance>();
            ResourceUsage = ResourceUsage.Create(CpuUsage, RamUsage);
            PerformanceMock.Setup(x =>
                x.GetResourceUsageAsync())
                .ReturnsAsync(ResourceUsage);

            Configuration = PerformanceWatcherConfiguration
                .Create()
                .EnsureThat(usage => usage.Cpu == CpuUsage && usage.Ram == RamUsage )
                .WithPerformanceProvider(() => PerformanceMock.Object)
                .Build();
            Watcher = PerformanceWatcher.Create("Performance watcher", Configuration);
        };

        Because of = async () =>
        {
            CheckResult = await Watcher.ExecuteAsync().Await().AsTask;
            PerformanceCheckResult = CheckResult as PerformanceWatcherCheckResult;
        };

        It should_invoke_performance_get_resource_usage_async_method_only_once = () =>
            PerformanceMock.Verify(x => x.GetResourceUsageAsync(), Times.Once);

        It should_have_valid_check_result = () => CheckResult.IsValid.Should().BeTrue();
        It should_have_check_result_of_type_performance = () => PerformanceCheckResult.Should().NotBeNull();

        It should_have_set_values_in_performance_check_result = () =>
        {
            PerformanceCheckResult.WatcherName.Should().NotBeEmpty();
            PerformanceCheckResult.WatcherType.Should().NotBeNull();
            PerformanceCheckResult.ResourceUsage.Should().NotBeNull();
            PerformanceCheckResult.ResourceUsage.Cpu.ShouldBeEquivalentTo(CpuUsage);
            PerformanceCheckResult.ResourceUsage.Ram.ShouldBeEquivalentTo(RamUsage);
        };

        //TODO: Remove when MSpec works with DNX 
        [Test]
        public void RunTest()
        {
            context();
            of();
            should_invoke_performance_get_resource_usage_async_method_only_once();
            should_have_valid_check_result();
            should_have_check_result_of_type_performance();
            should_have_set_values_in_performance_check_result();
        }
    }

    [Subject("Performance watcher execution")]
    public class when_invoking_ensure_async_predicate_that_is_valid : PerformanceWatcher_specs
    {
        static Mock<IPerformance> PerformanceMock;
        static ResourceUsage ResourceUsage;

        Establish context = () =>
        {
            PerformanceMock = new Mock<IPerformance>();
            ResourceUsage = ResourceUsage.Create(CpuUsage, RamUsage);
            PerformanceMock.Setup(x =>
                x.GetResourceUsageAsync())
                .ReturnsAsync(ResourceUsage);

            Configuration = PerformanceWatcherConfiguration
                .Create()
                .EnsureThatAsync(usage => Task.Factory.StartNew(() => usage.Cpu == CpuUsage && usage.Ram == RamUsage))
                .WithPerformanceProvider(() => PerformanceMock.Object)
                .Build();
            Watcher = PerformanceWatcher.Create("Performance watcher", Configuration);
        };

        Because of = async () =>
        {
            CheckResult = await Watcher.ExecuteAsync().Await().AsTask;
            PerformanceCheckResult = CheckResult as PerformanceWatcherCheckResult;
        };

        It should_invoke_performance_get_resource_usage_async_method_only_once = () =>
            PerformanceMock.Verify(x => x.GetResourceUsageAsync(), Times.Once);

        It should_have_valid_check_result = () => CheckResult.IsValid.Should().BeTrue();
        It should_have_check_result_of_type_performance = () => PerformanceCheckResult.Should().NotBeNull();

        It should_have_set_values_in_performance_check_result = () =>
        {
            PerformanceCheckResult.WatcherName.Should().NotBeEmpty();
            PerformanceCheckResult.WatcherType.Should().NotBeNull();
            PerformanceCheckResult.ResourceUsage.Should().NotBeNull();
            PerformanceCheckResult.ResourceUsage.Cpu.ShouldBeEquivalentTo(CpuUsage);
            PerformanceCheckResult.ResourceUsage.Ram.ShouldBeEquivalentTo(RamUsage);
        };

        //TODO: Remove when MSpec works with DNX 
        [Test]
        public void RunTest()
        {
            context();
            of();
            should_invoke_performance_get_resource_usage_async_method_only_once();
            should_have_valid_check_result();
            should_have_check_result_of_type_performance();
            should_have_set_values_in_performance_check_result();
        }
    }

    [Subject("Performance watcher execution")]
    public class when_invoking_ensure_predicate_that_is_invalid : PerformanceWatcher_specs
    {
        static Mock<IPerformance> PerformanceMock;
        static ResourceUsage ResourceUsage;

        Establish context = () =>
        {
            PerformanceMock = new Mock<IPerformance>();
            ResourceUsage = ResourceUsage.Create(CpuUsage, RamUsage);
            PerformanceMock.Setup(x =>
                x.GetResourceUsageAsync())
                .ReturnsAsync(ResourceUsage);
            Configuration = PerformanceWatcherConfiguration
                .Create()
                .EnsureThat(usage => usage.Cpu != CpuUsage && usage.Ram != RamUsage)
                .WithPerformanceProvider(() => PerformanceMock.Object)
                .Build();
            Watcher = PerformanceWatcher.Create("Performance watcher", Configuration);
        };

        Because of = async () =>
        {
            CheckResult = await Watcher.ExecuteAsync().Await().AsTask;
            PerformanceCheckResult = CheckResult as PerformanceWatcherCheckResult;
        };

        It should_invoke_performance_get_resource_usage_async_method_only_once = () =>
            PerformanceMock.Verify(x => x.GetResourceUsageAsync(), Times.Once);

        It should_have_invalid_check_result = () => CheckResult.IsValid.Should().BeFalse();
        It should_have_check_result_of_type_performance = () => PerformanceCheckResult.Should().NotBeNull();

        It should_have_set_values_in_performance_check_result = () =>
        {
            PerformanceCheckResult.WatcherName.Should().NotBeEmpty();
            PerformanceCheckResult.WatcherType.Should().NotBeNull();
            PerformanceCheckResult.ResourceUsage.Should().NotBeNull();
            PerformanceCheckResult.ResourceUsage.Cpu.ShouldBeEquivalentTo(CpuUsage);
            PerformanceCheckResult.ResourceUsage.Ram.ShouldBeEquivalentTo(RamUsage);
        };

        //TODO: Remove when MSpec works with DNX 
        [Test]
        public void RunTest()
        {
            context();
            of();
            should_invoke_performance_get_resource_usage_async_method_only_once();
            should_have_invalid_check_result();
            should_have_check_result_of_type_performance();
            should_have_set_values_in_performance_check_result();
        }
    }

    [Subject("Performance watcher execution")]
    public class when_invoking_ensure_async_predicate_that_is_invalid : PerformanceWatcher_specs
    {
        static Mock<IPerformance> PerformanceMock;
        static ResourceUsage ResourceUsage;

        Establish context = () =>
        {
            PerformanceMock = new Mock<IPerformance>();
            ResourceUsage = ResourceUsage.Create(CpuUsage, RamUsage);
            PerformanceMock.Setup(x =>
                x.GetResourceUsageAsync())
                .ReturnsAsync(ResourceUsage);
            Configuration = PerformanceWatcherConfiguration
                .Create()
                .EnsureThatAsync(usage => Task.Factory.StartNew(() => usage.Cpu != CpuUsage && usage.Ram != RamUsage))
                .WithPerformanceProvider(() => PerformanceMock.Object)
                .Build();
            Watcher = PerformanceWatcher.Create("Performance watcher", Configuration);
        };

        Because of = async () =>
        {
            CheckResult = await Watcher.ExecuteAsync().Await().AsTask;
            PerformanceCheckResult = CheckResult as PerformanceWatcherCheckResult;
        };

        It should_invoke_performance_get_resource_usage_async_method_only_once = () =>
            PerformanceMock.Verify(x => x.GetResourceUsageAsync(), Times.Once);

        It should_have_invalid_check_result = () => CheckResult.IsValid.Should().BeFalse();
        It should_have_check_result_of_type_performance = () => PerformanceCheckResult.Should().NotBeNull();

        It should_have_set_values_in_performance_check_result = () =>
        {
            PerformanceCheckResult.WatcherName.Should().NotBeEmpty();
            PerformanceCheckResult.WatcherType.Should().NotBeNull();
            PerformanceCheckResult.ResourceUsage.Should().NotBeNull();
            PerformanceCheckResult.ResourceUsage.Cpu.ShouldBeEquivalentTo(CpuUsage);
            PerformanceCheckResult.ResourceUsage.Ram.ShouldBeEquivalentTo(RamUsage);
        };

        //TODO: Remove when MSpec works with DNX 
        [Test]
        public void RunTest()
        {
            context();
            of();
            should_invoke_performance_get_resource_usage_async_method_only_once();
            should_have_invalid_check_result();
            should_have_check_result_of_type_performance();
            should_have_set_values_in_performance_check_result();
        }
    }
}