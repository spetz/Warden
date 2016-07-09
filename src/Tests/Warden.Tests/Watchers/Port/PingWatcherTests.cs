using System;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Warden.Core;
using Warden.Watchers;

namespace Warden.Tests.Watchers.Ping
{
    using System.Net.NetworkInformation;
    using global::Warden.Watchers.Port;

    public class PingWatcher_specs
    {
        protected static PingWatcher Watcher { get; set; }
        protected static PingWatcherConfiguration Configuration { get; set; }
        protected static IWatcherCheckResult CheckResult { get; set; }
        protected static PingWatcherCheckResult WebWatcherCheckResult { get; set; }
        protected static Exception Exception { get; set; }
    }

    [Subject("Ping watcher initialization")]
    public class when_initializing_with_invalid_hostname : PingWatcher_specs
    {
        private const string InvalidHostname = "http://www.google.pl";

        Establish context = () => { };

        Because of = () =>
        {
            Exception = Catch.Exception(() =>
                Configuration = PingWatcherConfiguration
                    .Create(InvalidHostname)
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
    
    [Subject("Ping watcher initialization")]
    public class when_trying_to_provide_null_dns_resolver : PingWatcher_specs
    {
        private static readonly string TestHostname = "website.com";

        Establish context = () =>
        {
            Configuration = PingWatcherConfiguration
                .Create(TestHostname)
                .WithDnsResolver(() => null)
                .Build();
            Watcher = PingWatcher.Create("Ping Watcher", Configuration);
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

    [Subject("Ping watcher initialization")]
    public class when_trying_to_provide_null_ping_provider : PingWatcher_specs
    {
        private static readonly string TestHostname = "website.com";

        Establish context = () => { };

        Because of = () =>
        {
            Configuration = PingWatcherConfiguration
                .Create(TestHostname)
                .WithPingProvider(() => null)
                .Build();
            Watcher = PingWatcher.Create("Ping Watcher", Configuration);
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

    [Subject("Ping watcher execution")]
    public class when_server_opens_connection_watcher_result_is_valid : PingWatcher_specs
    {
        private static Mock<IPingProvider> PingProvider;
        private static Mock<IDnsResolver> DnsResolverProvider;

        private static readonly string TestHostname = "website.com";
        private static readonly IPAddress TestIpAddress = new IPAddress(0x2414188f);

        Establish context = () =>
        {
            PingProvider = new Mock<IPingProvider>();
            PingProvider.Setup(tcp => tcp.PingAsync(TestIpAddress, null))
                .Returns(() => Task.FromResult(IPStatus.Success));
            DnsResolverProvider = new Mock<IDnsResolver>();
            DnsResolverProvider.Setup(dn => dn.GetIpAddress(TestHostname))
                .Returns((string ip) => TestIpAddress);

            Configuration = PingWatcherConfiguration
                .Create(TestHostname)
                .WithPingProvider(() => PingProvider.Object)
                .WithDnsResolver(() => DnsResolverProvider.Object)
                .Build();
            Watcher = PingWatcher.Create("Ping Watcher", Configuration);
        };

        Because of = async () =>
        {
            CheckResult = await Watcher.ExecuteAsync().Await().AsTask;
            WebWatcherCheckResult = CheckResult as PingWatcherCheckResult;
        };

        It should_have_valid_check_result = () => CheckResult.IsValid.Should().BeTrue();

        It should_invoke_ping_send_async_method_only_once =
            () => PingProvider.Verify(ping => ping.PingAsync(TestIpAddress, null), Times.Once);

        It should_have_a_specific_description = () => CheckResult.Description.Should().StartWith("Successfully connected to the hostname");

        //TODO: Remove when MSpec works with DNX 
        [Test]
        public void RunTest()
        {
            context();
            of();
            should_have_valid_check_result();
            should_invoke_ping_send_async_method_only_once();
            should_have_a_specific_description();
        }
    }

    [Subject("PingWatcher execution")]
    public class when_server_refuse_connection_watcher_result_is_not_valid : PingWatcher_specs
    {
        private static Mock<IPingProvider> PingProvider;
        private static Mock<IDnsResolver> DnsResolverProvider;

        private static readonly string TestHostname = "website.com";
        private static readonly IPAddress TestIpAddress = new IPAddress(0x2414188f);

        Establish context = () =>
        {
            PingProvider = new Mock<IPingProvider>();
            PingProvider.Setup(ping => ping.PingAsync(TestIpAddress, null))
                .Returns(() => Task.FromResult(IPStatus.DestinationHostUnreachable));
            DnsResolverProvider = new Mock<IDnsResolver>();
            DnsResolverProvider.Setup(dn => dn.GetIpAddress(TestHostname))
                .Returns((string ip) => TestIpAddress);

            Configuration = PingWatcherConfiguration
                .Create(TestHostname)
                .WithPingProvider(() => PingProvider.Object)
                .WithDnsResolver(() => DnsResolverProvider.Object)
                .Build();
            Watcher = PingWatcher.Create("Ping Watcher", Configuration);
        };

        Because of = async () =>
        {
            CheckResult = await Watcher.ExecuteAsync().Await().AsTask;
            WebWatcherCheckResult = CheckResult as PingWatcherCheckResult;
        };

        It should_have_valid_check_result = () => CheckResult.IsValid.Should().BeFalse();

        It should_invoke_ping_connect_async_method_only_once =
            () => PingProvider.Verify(ping => ping.PingAsync(TestIpAddress, null), Times.Once);

        It should_have_a_specific_reason = () => CheckResult.Description.Should().Contain("reachable");

        //TODO: Remove when MSpec works with DNX 
        [Test]
        public void RunTest()
        {
            context();
            of();
            should_have_valid_check_result();
            should_invoke_ping_connect_async_method_only_once();
            should_have_a_specific_reason();
        }
    }

    [Subject("PingWatcher execution")]
    public class when_host_name_cannot_be_resoveld_check_is_not_valid : PingWatcher_specs
    {
        private static Mock<IDnsResolver> DnsResolverProvider;

        private static readonly string TestHostname = "website.com";

        Establish context = () =>
        {

            DnsResolverProvider = new Mock<IDnsResolver>();
            DnsResolverProvider.Setup(dn => dn.GetIpAddress(TestHostname))
                .Returns((string ipAddress) => null);

            Configuration = PingWatcherConfiguration
                .Create(TestHostname)
                .WithDnsResolver(() => DnsResolverProvider.Object)
                .Build();
            Watcher = PingWatcher.Create("Ping Watcher", Configuration);
        };

        Because of = async () =>
        {
            CheckResult = await Watcher.ExecuteAsync().Await().AsTask;
            WebWatcherCheckResult = CheckResult as PingWatcherCheckResult;
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
