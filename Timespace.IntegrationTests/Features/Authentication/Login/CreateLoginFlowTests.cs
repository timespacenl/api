using FluentAssertions;
using FluentAssertions.NodaTime;
using MediatR;
using Microsoft.Extensions.Options;
using NodaTime;
using Timespace.Api.Application.Features.Users.Authentication.Login.Commands;
using Timespace.Api.Application.Features.Users.Authentication.Login.Common.Entities;
using Timespace.Api.Application.Features.Users.Authentication.Login.Exceptions;
using Timespace.Api.Infrastructure.Configuration;

namespace Timespace.IntegrationTests.Features.Authentication.Login;

public class CreateLoginFlowTests : IntegrationTest
{
    [Fact]
    public async Task ShouldCreateLoginFlow_WithValidEmail()
    {
        GetService(out ISender sender);
        GetService(out IClock clock);
        GetService(out IOptions<AuthenticationConfiguration> authenticationConfiguration);
        
        var email = "test@example.com";
        
        await LoginFlowTestHelpers.RegisterUserAsync(sender, email);

        var loginFlow = await sender.Send(new CreateLoginFlow.Command
        {
            Email = email,
            RememberMe = true
        });
        
        loginFlow.Should().NotBeNull();
        loginFlow.FlowId.Should().NotBeEmpty();
        loginFlow.SessionToken.Should().BeNull();
        loginFlow.NextStep.Should().Be(LoginFlowSteps.SetCredentials);
        loginFlow.ExpiresAt.Should().BeCloseTo(clock.GetCurrentInstant().Plus(Duration.FromMinutes(authenticationConfiguration.Value.LoginFlowTimeoutMinutes)), Duration.FromSeconds(5));
    }

    [Fact]
    public async Task ShouldThrowException_WithInvalidEmail()
    {
        GetService(out ISender sender);

        await FluentActions.Invoking(() => sender.Send(new CreateLoginFlow.Command
        {
            Email = "email@nonexistent.com",
            RememberMe = true
        })).Should().ThrowAsync<IdentifierNotFoundException>();
    }
    
    public CreateLoginFlowTests(SharedFixture sharedFixture) : base(sharedFixture)
    {
    }
}