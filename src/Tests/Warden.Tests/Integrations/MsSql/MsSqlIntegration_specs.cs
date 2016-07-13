using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Warden.Integrations;
using Warden.Integrations.MsSql;
using Warden.Watchers;
using Warden.Watchers.MsSql;

namespace Warden.Tests.Integrations.MsSql
{
    public class MsSqlIntegration_specs
    {
        protected static MsSqlIntegration Integration { get; set; }
        protected static MsSqlIntegrationConfiguration Configuration { get; set; }
        protected static Exception Exception { get; set; }

        protected static string ConnectionString =
            @"Data Source=.\sqlexpress;Initial Catalog=MyDatabase;Integrated Security=True";
    }

    [Subject("MS SQL integration initialization")]
    public class when_initializing_without_configuration : MsSqlIntegration_specs
    {
        Establish context = () => Configuration = null;

        Because of = () => Exception = Catch.Exception((Action) (() => Integration = MsSqlIntegration.Create(Configuration)));

        It should_fail = () => Exception.Should().BeOfType<ArgumentNullException>();

        It should_have_a_specific_reason =
            () => Exception.Message.Should().Contain("MS SQL integration configuration has not been provided.");

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

    [Subject("MS SQL integration initialization")]
    public class when_initializing_with_invalid_connection_string : MsSqlIntegration_specs
    {
        Establish context = () => { };

        Because of = () => Exception = Catch.Exception(() => Configuration = MsSqlIntegrationConfiguration
            .Create("invalid")
            .Build());

        It should_fail = () => Exception.Should().BeOfType<ArgumentException>();

        It should_have_a_specific_reason =
            () => Exception.Message.Should().Contain("MS SQL connection string is invalid.");

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

    [Subject("MS SQL integration execution")]
    public class when_invoking_open_connection_that_fails : MsSqlIntegration_specs
    {
        static Mock<IMsSqlService> MsSqlServiceMock;
        static Mock<IDbConnection> DbConnectionMock;

        Establish context = () =>
        {
            MsSqlServiceMock = new Mock<IMsSqlService>();
            DbConnectionMock = new Mock<IDbConnection>();
            DbConnectionMock.Setup(x => x.Open()).Throws(new Exception("Error"));
            Configuration = MsSqlIntegrationConfiguration
                .Create(ConnectionString)
                .WithQuery("select * from users")
                .WithConnectionProvider(connectionString => DbConnectionMock.Object)
                .WithMsSqlServiceProvider(() => MsSqlServiceMock.Object)
                .Build();
            Integration = MsSqlIntegration.Create(Configuration);
        };

        Because of = () => Exception = Catch.Exception(() => Integration.ExecuteAsync(Moq.It.IsAny<string>()).Await());

        It should_invoke_open_method_only_once = () => DbConnectionMock.Verify(x => x.Open(), Times.Once);
        It should_fail = () => Exception.Should().BeOfType<IntegrationException>();

        //TODO: Remove when MSpec works with DNX 
        [Test]
        public void RunTest()
        {
            context();
            of();
            should_invoke_open_method_only_once();
            should_fail();
        }
    }

    [Subject("MS SQL integration execution")]
    public class when_invoking_query_async_that_fails : MsSqlIntegration_specs
    {
        static Mock<IMsSqlService> MsSqlServiceMock;
        static Mock<IDbConnection> DbConnectionMock;

        Establish context = () =>
        {
            MsSqlServiceMock = new Mock<IMsSqlService>();
            DbConnectionMock = new Mock<IDbConnection>();
            MsSqlServiceMock.Setup(x => x.QueryAsync<dynamic>(Moq.It.IsAny<IDbConnection>(), Moq.It.IsAny<string>(),
                Moq.It.IsAny<IDictionary<string, object>>(), Moq.It.IsAny<TimeSpan?>()))
                .ThrowsAsync(new Exception("Error"));
            Configuration = MsSqlIntegrationConfiguration
                .Create(ConnectionString)
                .WithQuery("select * from users")
                .WithConnectionProvider(connectionString => DbConnectionMock.Object)
                .WithMsSqlServiceProvider(() => MsSqlServiceMock.Object)
                .Build();
            Integration = MsSqlIntegration.Create(Configuration);
        };

        Because of = () => Exception = Catch.Exception(() => Integration.QueryAsync<dynamic>(Moq.It.IsAny<string>()).Await());

        It should_invoke_open_method_only_once = () => DbConnectionMock.Verify(x => x.Open(), Times.Once);
        It should_fail = () => Exception.Should().BeOfType<IntegrationException>();


        //TODO: Remove when MSpec works with DNX 
        [Test]
        public void RunTest()
        {
            context();
            of();
            should_invoke_open_method_only_once();
            should_fail();
        }
    }

    [Subject("MS SQL integration execution")]
    public class when_invoking_execute_async_that_fails : MsSqlIntegration_specs
    {
        static Mock<IMsSqlService> MsSqlServiceMock;
        static Mock<IDbConnection> DbConnectionMock;

        Establish context = () =>
        {
            MsSqlServiceMock = new Mock<IMsSqlService>();
            DbConnectionMock = new Mock<IDbConnection>();
            MsSqlServiceMock.Setup(x => x.ExecuteAsync(Moq.It.IsAny<IDbConnection>(), Moq.It.IsAny<string>(),
                Moq.It.IsAny<IDictionary<string, object>>(), Moq.It.IsAny<TimeSpan?>()))
                .ThrowsAsync(new Exception("Error"));
            Configuration = MsSqlIntegrationConfiguration
                .Create(ConnectionString)
                .WithConnectionProvider(connectionString => DbConnectionMock.Object)
                .WithMsSqlServiceProvider(() => MsSqlServiceMock.Object)
                .Build();
            Integration = MsSqlIntegration.Create(Configuration);
        };

        Because of = () => Exception = Catch.Exception(() => Integration.ExecuteAsync(Moq.It.IsAny<string>()).Await());

        It should_invoke_open_method_only_once = () => DbConnectionMock.Verify(x => x.Open(), Times.Once);
        It should_fail = () => Exception.Should().BeOfType<IntegrationException>();

        //TODO: Remove when MSpec works with DNX 
        [Test]
        public void RunTest()
        {
            context();
            of();
            should_invoke_open_method_only_once();
            should_fail();
        }
    }
}