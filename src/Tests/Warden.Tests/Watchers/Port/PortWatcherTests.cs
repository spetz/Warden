using System;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Warden.Core;
using Warden.Watchers;
using Warden.Watchers.Port;

namespace Warden.Tests.Watchers.Port
{
    public class PortWatcher_specs
    {
        protected static PortWatcher Watcher { get; set; }
        protected static PortWatcherConfiguration Configuration { get; set; }
        protected static IWatcherCheckResult CheckResult { get; set; }
        protected static PortWatcherCheckResult WebWatcherCheckResult { get; set; }
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
        private const int InvalidPort = 0;

        Establish context = () => { };

        Because of = () =>
        {
            Exception = Catch.Exception(() =>
                Configuration = PortWatcherConfiguration
                    .Create(Hostname, InvalidPort)
                    .Build());
        };

        It should_fail = () => Exception.Should().BeOfType<ArgumentException>();
        It should_have_a_specific_reason = () => Exception.Message.Should().StartWith("Port number can not be less than 1.");

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
                .WithDnsResolver(() => null)
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

    [Subject("Port watcher execution")]
    public class when_server_opens_connection_watcher_result_is_valid : PortWatcher_specs
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
            DnsResolverProvider.Setup(dn => dn.GetIpAddress(TestHostname))
                .Returns((string ip) => TestIpAddress);

            Configuration = PortWatcherConfiguration
                .Create(TestHostname, TestPort)
                .WithTcpClientProvider(() => TcpClientProvider.Object)
                .WithDnsResolver(() => DnsResolverProvider.Object)
                .Build();
            Watcher = PortWatcher.Create("Port Watcher", Configuration);
        };

        Because of = async () =>
        {
            CheckResult = await Watcher.ExecuteAsync().Await().AsTask;
            WebWatcherCheckResult = CheckResult as PortWatcherCheckResult;
        };

        It should_have_valid_check_result = () => CheckResult.IsValid.Should().BeTrue();

        It should_invoke_tcp_client_connect_async_method_only_once =
            () => TcpClientProvider.Verify(tcp => tcp.ConnectAsync(TestIpAddress, TestPort, null), Times.Once);

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

    [Subject("PortWatcher execution")]
    public class when_server_refuse_connection_watcher_result_is_not_valid : PortWatcher_specs
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
            DnsResolverProvider.Setup(dn => dn.GetIpAddress(TestHostname))
                .Returns((string ip) => TestIpAddress);

            Configuration = PortWatcherConfiguration
                .Create(TestHostname, TestPort)
                .WithTcpClientProvider(() => TcpClientProvider.Object)
                .WithDnsResolver(() => DnsResolverProvider.Object)
                .Build();
            Watcher = PortWatcher.Create("Port Watcher", Configuration);
        };

        Because of = async () =>
        {
            CheckResult = await Watcher.ExecuteAsync().Await().AsTask;
            WebWatcherCheckResult = CheckResult as PortWatcherCheckResult;
        };

        It should_have_valid_check_result = () => CheckResult.IsValid.Should().BeFalse();

        It should_invoke_tcp_client_connect_async_method_only_once =
            () => TcpClientProvider.Verify(tcp => tcp.ConnectAsync(TestIpAddress, TestPort, null), Times.Once);

        It should_have_a_specific_reason = () => CheckResult.Description.Should().StartWith("Unable");

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

    [Subject("PortWatcher execution")]
    public class when_host_name_cannot_be_resoveld_check_is_not_valid : PortWatcher_specs
    {
        private static Mock<IDnsResolver> DnsResolverProvider;

        private static readonly string TestHostname = "website.com";
        private static readonly int TestPort = 80;

        Establish context = () =>
        {

            DnsResolverProvider = new Mock<IDnsResolver>();
            DnsResolverProvider.Setup(dn => dn.GetIpAddress(TestHostname))
                .Returns((string ipAddress) => null);

            Configuration = PortWatcherConfiguration
                .Create(TestHostname, TestPort)
                .WithDnsResolver(() => DnsResolverProvider.Object)
                .Build();
            Watcher = PortWatcher.Create("Port Watcher", Configuration);
        };

        Because of = async () =>
        {
            CheckResult = await Watcher.ExecuteAsync().Await().AsTask;
            WebWatcherCheckResult = CheckResult as PortWatcherCheckResult;
        };

        It should_have_valid_check_result = () => CheckResult.IsValid.Should().BeFalse();
        It should_have_a_specific_reason = () => CheckResult.Description.Should().StartWith("Could not resolve hostname");

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
