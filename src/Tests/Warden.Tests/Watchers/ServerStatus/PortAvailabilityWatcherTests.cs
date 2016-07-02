namespace Warden.Tests.Watchers.ServerStatus
{
    using System;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using FluentAssertions;
    using global::Warden.Core;
    using global::Warden.Watchers;
    using global::Warden.Watchers.ServerStatus;
    using global::Warden.Watchers.Web;
    using Moq;
    using NUnit.Framework;
    using It = global::Warden.Tests.It;

    public class PortAvailabilityWatcher_specs
    {
        protected static PortAvailabilityWatcher Watcher { get; set; }
        protected static PortAvailabilityConfiguration Configuration { get; set; }
        protected static IWatcherCheckResult CheckResult { get; set; }
        protected static PortAvailabilityCheckResult WebCheckResult { get; set; }
        protected static Exception Exception { get; set; }
    }

    [Subject("PortAvailabilityWatcher execution")]
    public class when_server_opens_connection_watcher_result_is_valid : PortAvailabilityWatcher_specs
    {
        private static Mock<ITcpClient> TcpClientProvider;
        private static Mock<IDnsResolver> DnsResolverProvider;

        private static readonly string TestHostname = "website.com";
        private static readonly IPAddress TestIpAddress = new IPAddress(0x2414188f);
        private static readonly int TestPort = 80;

        Establish context = () =>
        {
            TcpClientProvider = new Mock<ITcpClient>();
            TcpClientProvider.Setup(tcp => tcp.ConnectAsync(TestIpAddress, TestPort, null))
                .Returns(() => Task.FromResult(""));
            TcpClientProvider.Setup(tcp => tcp.IsConnected).Returns(() => true);

            DnsResolverProvider = new Mock<IDnsResolver>();
            DnsResolverProvider.Setup(dn => dn.GetIp(TestHostname))
                .Returns((string ip) => TestIpAddress);

            Configuration = PortAvailabilityConfiguration
                .Create(TestHostname)
                .ForPort(TestPort)
                .WithTcpClientProvider(() => TcpClientProvider.Object)
                .WithDnsResolver(() => DnsResolverProvider.Object)
                .Build();
            Watcher = PortAvailabilityWatcher.Create("Port availability", Configuration);
        };

        Because of = async () =>
        {
            CheckResult = await Watcher.ExecuteAsync().Await().AsTask;
            WebCheckResult = CheckResult as PortAvailabilityCheckResult;
        };

        private It should_have_valid_check_result = () => CheckResult.IsValid.Should().BeTrue();

        private It shoud_connect_be_called_with_valid_parameters =
            () => TcpClientProvider.Verify(tcp => tcp.ConnectAsync(TestIpAddress, TestPort, null), Times.Once);

        private It should_description_contain_connected_phrase = () => CheckResult.Description.Should().StartWith("Connected");


        //TODO: Remove when MSpec works with DNX 
        [Test]
        public void RunTest()
        {
            context();
            of();
            should_have_valid_check_result();
            shoud_connect_be_called_with_valid_parameters();
            should_description_contain_connected_phrase();
        }
    }

    [Subject("PortAvailabilityWatcher execution")]
    public class when_server_refuse_connection_watcher_result_is_not_valid : PortAvailabilityWatcher_specs
    {
        private static Mock<ITcpClient> TcpClientProvider;
        private static Mock<IDnsResolver> DnsResolverProvider;

        private static readonly string TestHostname = "website.com";
        private static readonly IPAddress TestIpAddress = new IPAddress(0x2414188f);
        private static readonly int TestPort = 80;

        Establish context = () =>
        {
            TcpClientProvider = new Mock<ITcpClient>();
            TcpClientProvider.Setup(tcp => tcp.ConnectAsync(TestIpAddress, TestPort, null))
                .Returns(() => Task.FromResult(""));
            TcpClientProvider.Setup(tcp => tcp.IsConnected).Returns(() => false);

            DnsResolverProvider = new Mock<IDnsResolver>();
            DnsResolverProvider.Setup(dn => dn.GetIp(TestHostname))
                .Returns((string ip) => TestIpAddress);

            Configuration = PortAvailabilityConfiguration
                .Create(TestHostname)
                .ForPort(TestPort)
                .WithTcpClientProvider(() => TcpClientProvider.Object)
                .WithDnsResolver(() => DnsResolverProvider.Object)
                .Build();
            Watcher = PortAvailabilityWatcher.Create("Port availability", Configuration);
        };

        Because of = async () =>
        {
            CheckResult = await Watcher.ExecuteAsync().Await().AsTask;
            WebCheckResult = CheckResult as PortAvailabilityCheckResult;
        };

        private It should_have_valid_check_result = () => CheckResult.IsValid.Should().BeFalse();

        private It shoud_connect_be_called_with_valid_parameters =
            () => TcpClientProvider.Verify(tcp => tcp.ConnectAsync(TestIpAddress, TestPort, null), Times.Once);

        private It should_description_contain_unable_phrase = () => CheckResult.Description.Should().StartWith("Unable");


        //TODO: Remove when MSpec works with DNX 
        [Test]
        public void RunTest()
        {
            context();
            of();
            should_have_valid_check_result();
            shoud_connect_be_called_with_valid_parameters();
            should_description_contain_unable_phrase();
        }
    }

    [Subject("PortAvailabilityWatcher execution")]
    public class when_host_name_cannot_be_resoveld_check_is_not_valid : PortAvailabilityWatcher_specs
    {
        private static Mock<IDnsResolver> DnsResolverProvider;

        private static readonly string TestHostname = "website.com";
        private static readonly int TestPort = 80;

        Establish context = () =>
        {

            DnsResolverProvider = new Mock<IDnsResolver>();
            DnsResolverProvider.Setup(dn => dn.GetIp(TestHostname))
                .Returns((string ip) => null);

            Configuration = PortAvailabilityConfiguration
                .Create(TestHostname)
                .ForPort(TestPort)
                .WithDnsResolver(() => DnsResolverProvider.Object)
                .Build();
            Watcher = PortAvailabilityWatcher.Create("Port availability", Configuration);
        };

        Because of = async () =>
        {
            CheckResult = await Watcher.ExecuteAsync().Await().AsTask;
            WebCheckResult = CheckResult as PortAvailabilityCheckResult;
        };

        private It should_have_valid_check_result = () => CheckResult.IsValid.Should().BeFalse();
        private It should_description_contain_unable_phrase = () => CheckResult.Description.Should().BeEquivalentTo("Could not resolve host.");


        //TODO: Remove when MSpec works with DNX 
        [Test]
        public void RunTest()
        {
            context();
            of();
            should_have_valid_check_result();
            should_description_contain_unable_phrase();
        }
    }

    [Subject("PortAvailabilityWatcher execution")]
    public class when_tcp_client_provder_is_null_check_throws_exception : PortAvailabilityWatcher_specs
    {
        private static readonly string TestHostname = "website.com";
        private static readonly int TestPort = 80;

        Establish context = () =>
        {
            Configuration = PortAvailabilityConfiguration
                .Create(TestHostname)
                .ForPort(TestPort)
                .WithTcpClientProvider(() => null)
                .Build();
            Watcher = PortAvailabilityWatcher.Create("Port availability", Configuration);
        };

        Because of = () =>
        {
            Exception = Catch.Exception(() => Watcher.ExecuteAsync().Await().AsTask.Result);
        };

        private It should_fail = () => Exception.Should().BeOfType<WardenException>();


        //TODO: Remove when MSpec works with DNX 
        [Test]
        public void RunTest()
        {
            context();
            of();
            should_fail();
        }
    }

    [Subject("PortAvailabilityWatcher execution")]
    public class when_dns_resolver_provder_is_null_check_throws_exception : PortAvailabilityWatcher_specs
    {
        private static readonly string TestHostname = "website.com";
        private static readonly int TestPort = 80;

        Establish context = () =>
        {
            Configuration = PortAvailabilityConfiguration
                .Create(TestHostname)
                .ForPort(TestPort)
                .WithDnsResolver(() => null)
                .Build();
            Watcher = PortAvailabilityWatcher.Create("Port availability", Configuration);
        };

        Because of = () =>
        {
            Exception = Catch.Exception(() => Watcher.ExecuteAsync().Await().AsTask.Result);
        };

        private It should_fail = () => Exception.Should().BeOfType<WardenException>();


        //TODO: Remove when MSpec works with DNX 
        [Test]
        public void RunTest()
        {
            context();
            of();
            should_fail();
        }
    }
}
