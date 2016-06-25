using System;
using System.Collections.Generic;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Warden.Core;
using Warden.Integrations.HttpApi;

namespace Warden.Tests.Integrations.HttpApi
{
    public class HttpApiIntegration_specs
    {
        protected static HttpApiIntegration Integration { get; set; }
        protected static HttpApiIntegrationConfiguration Configuration { get; set; }
        protected static Exception Exception { get; set; }
        protected static string ApiUrl = "http://my-api.com";
        protected static string Endpoint = "/endpoint";
        protected static string ApiKey = "api-key";
        protected static string OrganizationId = Guid.NewGuid().ToString();
    }

    [Subject("HttpApi integration initialization")]
    public class when_initializing_without_configuration : HttpApiIntegration_specs
    {
        Establish context = () => Configuration = null;

        Because of = () => Exception = Catch.Exception(() => Integration = HttpApiIntegration.Create(Configuration));

        It should_fail = () => Exception.Should().BeOfType<ArgumentNullException>();

        It should_have_a_specific_reason =
            () => Exception.Message.Should().Contain("HTTP API Integration configuration has not been provided.");

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

    [Subject("HttpApi integration initialization")]
    public class when_initializing_with_invalid_url : HttpApiIntegration_specs
    {
        Establish context = () => { };

        Because of = () => Exception = Catch.Exception(() => Configuration = HttpApiIntegrationConfiguration
            .Create("Invalid")
            .Build());

        It should_fail = () => Exception.Should().BeOfType<UriFormatException>();

        //TODO: Remove when MSpec works with DNX 
        [Test]
        public void RunTest()
        {
            context();
            of();
            should_fail();
        }
    }

    [Subject("HttpApi integration execution")]
    public class when_invoking_post_async_method_with_valid_configuration : HttpApiIntegration_specs
    {
        static Mock<IHttpService> HttpServiceMock;

        Establish context = () =>
        {
            HttpServiceMock = new Mock<IHttpService>();
            Configuration = HttpApiIntegrationConfiguration
                .Create(ApiUrl)
                .WithHttpServiceProvider(() => HttpServiceMock.Object)
                .Build();
            Integration = HttpApiIntegration.Create(Configuration);
        };

        Because of = async () => await Integration.PostAsync(new {}).Await().AsTask;

        It should_invoke_post_async_method_only_once = () => HttpServiceMock.Verify(x =>
            x.PostAsync(Moq.It.IsAny<string>(), Moq.It.IsAny<string>(), Moq.It.IsAny<IDictionary<string, string>>(),
                Moq.It.IsAny<TimeSpan?>(), Moq.It.IsAny<bool>()), Times.Once);

        //TODO: Remove when MSpec works with DNX 
        [Test]
        public void RunTest()
        {
            context();
            of();
            should_invoke_post_async_method_only_once();
        }
    }

    [Subject("HttpApi integration execution")]
    public class when_invoking_post_iteration_to_warden_panel_async_method_with_null_iteration :
        HttpApiIntegration_specs
    {
        static Mock<IHttpService> HttpServiceMock;

        Establish context = () =>
        {
            HttpServiceMock = new Mock<IHttpService>();
            Configuration = HttpApiIntegrationConfiguration
                .Create(ApiUrl)
                .WithHttpServiceProvider(() => HttpServiceMock.Object)
                .Build();
            Integration = HttpApiIntegration.Create(Configuration);
        };

        Because of = () => Exception =
            Catch.Exception(() => Integration.PostIterationToWardenPanelAsync(null).Await().AsTask);

        It should_fail = () => Exception.Should().BeOfType<ArgumentNullException>();

        It should_have_a_specific_reason = () => Exception.Message.Should().Contain("Warden iteration can not be null.");

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

    [Subject("HttpApi integration execution")]
    public class when_invoking_post_iteration_to_warden_panel_async_method_with_empty_warden_name :
        HttpApiIntegration_specs
    {
        static Mock<IHttpService> HttpServiceMock;
        static Mock<IWardenIteration> WardenIterationMock;

        Establish context = () =>
        {
            WardenIterationMock = new Mock<IWardenIteration>();
            HttpServiceMock = new Mock<IHttpService>();
            Configuration = HttpApiIntegrationConfiguration
                .Create(ApiUrl)
                .WithHttpServiceProvider(() => HttpServiceMock.Object)
                .Build();
            Integration = HttpApiIntegration.Create(Configuration);
        };

        Because of = () => Exception =
            Catch.Exception(() => Integration.PostIterationToWardenPanelAsync(WardenIterationMock.Object).Await().AsTask);

        It should_fail = () => Exception.Should().BeOfType<ArgumentException>();

        It should_have_a_specific_reason = () => Exception.Message.Should().Contain("Warden name can not be empty.");

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

    [Subject("HttpApi integration execution")]
    public class when_invoking_post_iteration_to_warden_panel_async_method_with_valid_iteration :
        HttpApiIntegration_specs
    {
        static Mock<IWardenIteration> WardenIterationMock;
        static Mock<IHttpService> HttpServiceMock;

        Establish context = () =>
        {
            WardenIterationMock = new Mock<IWardenIteration>();
            WardenIterationMock.Setup(x => x.WardenName).Returns("Warden");
            HttpServiceMock = new Mock<IHttpService>();
            Configuration = HttpApiIntegrationConfiguration
                .Create(ApiUrl)
                .WithHttpServiceProvider(() => HttpServiceMock.Object)
                .Build();
            Integration = HttpApiIntegration.Create(Configuration);
        };

        Because of = async () =>
        {
            await Integration.PostIterationToWardenPanelAsync(WardenIterationMock.Object).Await().AsTask;
        };

        It should_invoke_post_async_method_only_once = () => HttpServiceMock.Verify(x =>
            x.PostAsync(Moq.It.IsAny<string>(), Moq.It.IsAny<string>(), Moq.It.IsAny<IDictionary<string, string>>(),
                Moq.It.IsAny<TimeSpan?>(), Moq.It.IsAny<bool>()), Times.Once);

        //TODO: Remove when MSpec works with DNX 
        [Test]
        public void RunTest()
        {
            context();
            of();
            should_invoke_post_async_method_only_once();
        }
    }
}