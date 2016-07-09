using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Warden.Watchers;
using Warden.Watchers.Port;

namespace Warden.Tests.Watchers.Port
{
    public class PortWatcher_specs
    {
        protected static PortWatcher Watcher { get; set; }
        protected static PortWatcherConfiguration Configuration { get; set; }
        protected static IWatcherCheckResult CheckResult { get; set; }
        protected static PortWatcherCheckResult PortWatcherCheckResult { get; set; }
        protected static Exception Exception { get; set; }
    }

    [Subject("Port watcher initialization")]
    public class when_initializing_with_invalid_hostname : PortWatcher_specs
    {
        private const string InvalidHostname = "http://www.google.pl";
        private const int Port = 80;

        Establish context = () => { };

        Because of = () =>
        {
            Exception = Catch.Exception(() =>
                Configuration = PortWatcherConfiguration
                    .Create(InvalidHostname, Port)
                    .Build());
        };

        It should_fail = () => Exception.Should().BeOfType<ArgumentException>();
        It should_have_a_specific_reason = () => Exception.Message.Should().StartWith("The hostname should not contain protocol.");

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

    [Subject("Port watcher initialization")]
    public class when_initializing_with_invalid_port : PortWatcher_specs
    {
        private const string Hostname = "www.google.pl";
        private const int InvalidPort = -1;

        Establish context = () => { };

        Because of = () =>
        {
            Exception = Catch.Exception(() =>
                Configuration = PortWatcherConfiguration
                    .Create(Hostname, InvalidPort)
                    .Build());
        };

        It should_fail = () => Exception.Should().BeOfType<ArgumentException>();
        It should_have_a_specific_reason = () => Exception.Message.Should().StartWith("Port number can not be less than 0.");

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

    [Subject("Port watcher initialization")]
    public class when_trying_to_provide_null_dns_resolver : PortWatcher_specs
    {
        private static readonly string TestHostname = "website.com";
        private static readonly int TestPort = 80;

        Establish context = () =>
        {
            Configuration = PortWatcherConfiguration
                .Create(TestHostname, TestPort)
                .WithDnsResolverProvider(() => null)
                .Build();
            Watcher = PortWatcher.Create("Port Watcher", Configuration);
        };

        Because of = () =>
        {
            Exception = Catch.Exception(() => Watcher.ExecuteAsync().Await().AsTask.Result);
        };

        It should_fail = () => Exception.Should().BeOfType<WatcherException>();

        //TODO: Remove when MSpec works with DNX 
        [Test]
        public void RunTest()
        {
            context();
            of();
            should_fail();
        }
    }

    [Subject("Port watcher initialization")]
    public class when_trying_to_provide_null_tcp_client : PortWatcher_specs
    {
        private static readonly string TestHostname = "website.com";
        private static readonly int TestPort = 80;

        Establish context = () => { };

        Because of = () =>
        {
            Configuration = PortWatcherConfiguration
                .Create(TestHostname, TestPort)
                .WithTcpClientProvider(() => null)
                .Build();
            Watcher = PortWatcher.Create("Port Watcher", Configuration);
        };

        It should_fail = () => Exception.Should().BeOfType<WatcherException>();

        //TODO: Remove when MSpec works with DNX 
        [Test]
        public void RunTest()
        {
            context();
            of();
            should_fail();
        }
    }

    [Subject("Port watcher initialization")]
    public class when_trying_to_provide_null_pinger_provider : PortWatcher_specs
    {
        private static readonly string TestHostname = "website.com";
        private static readonly int TestPort = 80;

        Establish context = () => { };

        Because of = () =>
        {
            Configuration = PortWatcherConfiguration
                .Create(TestHostname)
                .WithPingerProvider(() => null)
                .Build();
            Watcher = PortWatcher.Create("Ping Watcher", Configuration);
        };

        It should_fail = () => Exception.Should().BeOfType<WatcherException>();

        //TODO: Remove when MSpec works with DNX 
        [Test]
        public void RunTest()
        {
            context();
            of();
            should_fail();
        }
    }

    [Subject("Port watcher execution")]
    public class when_server_accepts_connection : PortWatcher_specs
    {
        private static Mock<ITcpClient> TcpClientMock;
        private static Mock<IDnsResolver> DnsResolverMock;
        private static Mock<IPinger> PingerMock;

        private static readonly string TestHostname = "website.com";
        private static readonly IPAddress TestIpAddress = new IPAddress(0x2414188f);
        private static readonly int TestPort = 80;

        Establish context = () =>
        {
            TcpClientMock = new Mock<ITcpClient>();
            TcpClientMock.Setup(tcp => tcp.ConnectAsync(Moq.It.IsAny<IPAddress>(),
                Moq.It.IsAny<int>(), Moq.It.IsAny<TimeSpan?>()))
                .Returns(Task.FromResult(string.Empty));
            TcpClientMock.Setup(tcp => tcp.IsConnected).Returns(() => true);
            PingerMock = new Mock<IPinger>();
            PingerMock.Setup(x => x.PingAsync(Moq.It.IsAny<IPAddress>(), Moq.It.IsAny<TimeSpan?>()))
            .ReturnsAsync(IPStatus.Success);
            DnsResolverMock = new Mock<IDnsResolver>();
            DnsResolverMock.Setup(dn => dn.GetIpAddress(Moq.It.IsAny<string>()))
                .Returns((string ip) => TestIpAddress);

            Configuration = PortWatcherConfiguration
                .Create(TestHostname, TestPort)
                .WithTcpClientProvider(() => TcpClientMock.Object)
                .WithDnsResolverProvider(() => DnsResolverMock.Object)
                .WithPingerProvider(() => PingerMock.Object)
                .WithTimeout(TimeSpan.FromSeconds(1))
                .Build();
            Watcher = PortWatcher.Create("Port Watcher", Configuration);
        };

        Because of = async () =>
        {
            CheckResult = await Watcher.ExecuteAsync().Await().AsTask;
            PortWatcherCheckResult = CheckResult as PortWatcherCheckResult;
        };

        It should_have_valid_check_result = () => CheckResult.IsValid.Should().BeTrue();

        It should_invoke_tcp_client_connect_async_method_only_once =
            () => TcpClientMock.Verify(tcp => tcp.ConnectAsync(Moq.It.IsAny<IPAddress>(), Moq.It.IsAny<int>(), Moq.It.IsAny<TimeSpan?>()), Times.Once);

        It should_have_a_specific_description = () => CheckResult.Description.Should().StartWith("Successfully connected to the hostname");

        //TODO: Remove when MSpec works with DNX 
        [Test]
        public void RunTest()
        {
            context();
            of();
            should_have_valid_check_result();
            should_invoke_tcp_client_connect_async_method_only_once();
            should_have_a_specific_description();
        }
    }

    [Subject("Port watcher execution")]
    public class when_server_refuses_connection : PortWatcher_specs
    {
        private static Mock<ITcpClient> TcpClientMock;
        private static Mock<IDnsResolver> DnsResolverMock;
        private static Mock<IPinger> PingerMock;

        private static readonly string TestHostname = "website.com";
        private static readonly IPAddress TestIpAddress = new IPAddress(0x2414188f);
        private static readonly int TestPort = 80;

        Establish context = () =>
        {
            TcpClientMock = new Mock<ITcpClient>();
            TcpClientMock.Setup(tcp => tcp.ConnectAsync(Moq.It.IsAny<IPAddress>(),
                Moq.It.IsAny<int>(), Moq.It.IsAny<TimeSpan?>()))
                .Returns(Task.FromResult(string.Empty));
            TcpClientMock.Setup(tcp => tcp.IsConnected).Returns(() => false);
            DnsResolverMock = new Mock<IDnsResolver>();
            DnsResolverMock.Setup(dn => dn.GetIpAddress(Moq.It.IsAny<string>()))
                .Returns(TestIpAddress);
            PingerMock = new Mock<IPinger>();
            PingerMock.Setup(x => x.PingAsync(Moq.It.IsAny<IPAddress>(), Moq.It.IsAny<TimeSpan?>()))
            .ReturnsAsync(IPStatus.Unknown);

            Configuration = PortWatcherConfiguration
                .Create(TestHostname, TestPort)
                .WithTcpClientProvider(() => TcpClientMock.Object)
                .WithDnsResolverProvider(() => DnsResolverMock.Object)
                .WithPingerProvider(() => PingerMock.Object)
                .Build();
            Watcher = PortWatcher.Create("Port Watcher", Configuration);
        };

        Because of = async () =>
        {
            CheckResult = await Watcher.ExecuteAsync().Await().AsTask;
            PortWatcherCheckResult = CheckResult as PortWatcherCheckResult;
        };

        It should_have_valid_check_result = () => CheckResult.IsValid.Should().BeFalse();

        It should_invoke_tcp_client_connect_async_method_only_once =
            () => TcpClientMock.Verify(tcp => tcp.ConnectAsync(Moq.It.IsAny<IPAddress>(), Moq.It.IsAny<int>(), Moq.It.IsAny<TimeSpan?>()), Times.Once);

        It should_have_a_specific_reason = () => CheckResult.Description.Should().StartWith("Could not resolve the hostname");

        //TODO: Remove when MSpec works with DNX 
        [Test]
        public void RunTest()
        {
            context();
            of();
            should_have_valid_check_result();
            should_invoke_tcp_client_connect_async_method_only_once();
            should_have_a_specific_reason();
        }
    }

    [Subject("Port watcher execution")]
    public class when_hostname_cannot_be_resolved : PortWatcher_specs
    {
        private static Mock<ITcpClient> TcpClientMock;
        private static Mock<IDnsResolver> DnsResolverMock;
        private static Mock<IPinger> PingerMock;
        private static readonly string TestHostname = "website.com";
        private static readonly int TestPort = 80;

        Establish context = () =>
        {
            DnsResolverMock = new Mock<IDnsResolver>();
            DnsResolverMock.Setup(dn => dn.GetIpAddress(Moq.It.IsAny<string>()))
                .Returns(IPAddress.None);
            TcpClientMock = new Mock<ITcpClient>();
            TcpClientMock.Setup(tcp => tcp.ConnectAsync(Moq.It.IsAny<IPAddress>(),
                Moq.It.IsAny<int>(), Moq.It.IsAny<TimeSpan?>()))
                .Returns(Task.FromResult(string.Empty));
            TcpClientMock.Setup(tcp => tcp.IsConnected).Returns(() => false);
            PingerMock = new Mock<IPinger>();
            PingerMock.Setup(x => x.PingAsync(Moq.It.IsAny<IPAddress>(), Moq.It.IsAny<TimeSpan?>()))
            .ReturnsAsync(IPStatus.Success);

            Configuration = PortWatcherConfiguration
                .Create(TestHostname, TestPort)
                .WithTcpClientProvider(() => TcpClientMock.Object)
                .WithDnsResolverProvider(() => DnsResolverMock.Object)
                .WithPingerProvider(() => PingerMock.Object)
                .Build();
            Watcher = PortWatcher.Create("Port Watcher", Configuration);
        };

        Because of = async () =>
        {
            CheckResult = await Watcher.ExecuteAsync().Await().AsTask;
            PortWatcherCheckResult = CheckResult as PortWatcherCheckResult;
        };

        It should_have_valid_check_result = () => CheckResult.IsValid.Should().BeFalse();
        It should_have_a_specific_reason = () => CheckResult.Description.Should().StartWith("Could not resolve the hostname");

        //TODO: Remove when MSpec works with DNX 
        [Test]
        public void RunTest()
        {
            context();
            of();
            should_have_valid_check_result();
            should_have_a_specific_reason();
        }      
    }
}
