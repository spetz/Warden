using System;
using System.Collections.Generic;
using FluentAssertions;
using Moq;
using Warden.Integrations.Cachet;
using Warden.Watchers;
using Machine.Specifications;
using It = Machine.Specifications.It;

namespace Warden.Tests.Integrations.Cachet
{
    public class CachetIntegration_specs
    {
        protected static CachetIntegration Integration { get; set; }
        protected static CachetIntegrationConfiguration Configuration { get; set; }
        protected static Exception Exception { get; set; }
        protected static string ApiUrl = "http://localhost/api/v1";
        protected static string AccessToken = "token";
        protected static string Username = "user";
        protected static string Password = "password";
    }

    [Subject("Cachet integration initialization")]
    public class when_initializing_without_configuration : CachetIntegration_specs
    {
        Establish context = () => Configuration = null;

        Because of = () => Exception = Catch.Exception(() => Integration = CachetIntegration.Create(Configuration));

        It should_fail = () => Exception.Should().BeOfType<ArgumentNullException>();

        It should_have_a_specific_reason =
            () => Exception.Message.Should().Contain("Cachet Integration configuration has not been provided.");
    }

    [Subject("Cachet integration initialization")]
    public class when_initializing_with_invalid_api_url_and_access_token : CachetIntegration_specs
    {
        Establish context = () => { };

        Because of = () => Exception = Catch.Exception(() => Configuration = CachetIntegrationConfiguration
            .Create(string.Empty, string.Empty)
            .Build());

        It should_fail = () => Exception.Should().BeOfType<ArgumentException>();

        It should_have_a_specific_reason =  () => Exception.Message.Should().Contain("API URL can not be empty.");
    }

    [Subject("Cachet integration initialization")]
    public class when_initializing_with_invalid_api_url_and_username_and_password : CachetIntegration_specs
    {
        Establish context = () => { };

        Because of = () => Exception = Catch.Exception(() => Configuration = CachetIntegrationConfiguration
            .Create(string.Empty, string.Empty, string.Empty)
            .Build());

        It should_fail = () => Exception.Should().BeOfType<ArgumentException>();

        It should_have_a_specific_reason = () => Exception.Message.Should().Contain("API URL can not be empty.");
    }


    [Subject("Cachet integration initialization")]
    public class when_initializing_with_valid_api_url_and_invalid_access_token : CachetIntegration_specs
    {
        Establish context = () => { };

        Because of = () => Exception = Catch.Exception(() => Configuration = CachetIntegrationConfiguration
            .Create(ApiUrl, string.Empty)
            .Build());

        It should_fail = () => Exception.Should().BeOfType<ArgumentException>();

        It should_have_a_specific_reason = () => Exception.Message.Should().Contain("Access token can not be empty.");
    }

    [Subject("Cachet integration initialization")]
    public class when_initializing_with_valid_api_url_and_invalid_username : CachetIntegration_specs
    {
        Establish context = () => { };

        Because of = () => Exception = Catch.Exception(() => Configuration = CachetIntegrationConfiguration
            .Create(ApiUrl, string.Empty, Password)
            .Build());

        It should_fail = () => Exception.Should().BeOfType<ArgumentException>();

        It should_have_a_specific_reason = () => Exception.Message.Should().Contain("Username can not be empty.");
    }

    [Subject("Cachet integration initialization")]
    public class when_initializing_with_valid_api_url_and_invalid_password : CachetIntegration_specs
    {
        Establish context = () => { };

        Because of = () => Exception = Catch.Exception(() => Configuration = CachetIntegrationConfiguration
            .Create(ApiUrl, Username, string.Empty)
            .Build());

        It should_fail = () => Exception.Should().BeOfType<ArgumentException>();

        It should_have_a_specific_reason = () => Exception.Message.Should().Contain("Password can not be empty.");
    }

    [Subject("Cachet integration initialization")]
    public class when_initializing_with_valid_api_url_and_access_token : CachetIntegration_specs
    {
        Establish context = () => Configuration = CachetIntegrationConfiguration
                .Create(ApiUrl, AccessToken)
                .Build();

        Because of = () => Integration = CachetIntegration.Create(Configuration);

        It should_initialize_cachet_integration = () => Integration.Should().NotBeNull();
    }

    [Subject("Cachet integration initialization")]
    public class when_initializing_with_valid_api_url_and_username_and_password : CachetIntegration_specs
    {
        Establish context = () => Configuration = CachetIntegrationConfiguration
                .Create(ApiUrl, Username, Password)
                .Build();

        Because of = () => Integration = CachetIntegration.Create(Configuration);

        It should_initialize_cachet_integration = () => Integration.Should().NotBeNull();
    }
}