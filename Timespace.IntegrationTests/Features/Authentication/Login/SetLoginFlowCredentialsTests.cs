using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NodaTime;
using NodaTime.Testing;
using Timespace.Api.Application.Features.Users.Authentication.Common.Exceptions;
using Timespace.Api.Application.Features.Users.Authentication.Login.Commands;
using Timespace.Api.Application.Features.Users.Authentication.Login.Common.Entities;
using Timespace.Api.Application.Features.Users.Authentication.Login.Exceptions;
using Timespace.Api.Application.Features.Users.Common.Entities.Credentials;
using Timespace.Api.Infrastructure.Configuration;
using Timespace.Api.Infrastructure.Persistence;
using Timespace.Api.Infrastructure.Services;

namespace Timespace.IntegrationTests.Features.Authentication.Login;

public class SetLoginFlowCredentialsTests : IntegrationTest
{
    [Fact]
    public async Task ShouldComplete_WithValidCredentials_WithoutMfa()
    {
        GetService(out ISender sender);

        var email = "test@example.com";
        var password = "TestPassword123!";
        
        await LoginFlowTestHelpers.RegisterUserAsync(sender, email, password);

        var flow = await sender.Send(new CreateLoginFlow.Command
        {
            Email = email,
            RememberMe = true
        });
        
        var setCredentialsResult = await sender.Send(new SetLoginFlowCredentials.Command
        {
            FlowId = flow.FlowId,
            Body = new SetLoginFlowCredentials.CommandBody
            {
                CredentialType = CredentialTypes.Password,
                CredentialValue = password
            }
        });

        setCredentialsResult.Should().NotBeNull();
        setCredentialsResult.SessionToken.Should().NotBeNullOrWhiteSpace();
        setCredentialsResult.NextStep.Should().Be(LoginFlowSteps.None);
        setCredentialsResult.AllowedMethodsForNextStep.Should().BeEmpty();
    }

    [Fact]
    public async Task ShouldAdvanceFlow_WithValidCredentials_WithMfa()
    {
        GetService(out ISender sender);

        var email = "test@example.com";
        var password = "TestPassword123!";
        
        var registrationResult = await LoginFlowTestHelpers.RegisterUserAsync(sender, email, password);

        GetService(out IAuthenticationTokenProvider authenticationTokenProvider);
        authenticationTokenProvider.AuthenticationToken = registrationResult.SessionToken;
        authenticationTokenProvider.AuthenticationTokenType = AuthenticationTokenType.UserSession;
        
        GetService(out IClock clock);
        await LoginFlowTestHelpers.ConfigureMfa(sender, clock);
        
        authenticationTokenProvider.AuthenticationToken = null;
        authenticationTokenProvider.AuthenticationTokenType = null;
        
        var flow = await sender.Send(new CreateLoginFlow.Command
        {
            Email = email,
            RememberMe = true
        });
        
        var setCredentialsResult = await sender.Send(new SetLoginFlowCredentials.Command
        {
            FlowId = flow.FlowId,
            Body = new SetLoginFlowCredentials.CommandBody
            {
                CredentialType = CredentialTypes.Password,
                CredentialValue = password
            }
        });
        
        setCredentialsResult.Should().NotBeNull();
        setCredentialsResult.SessionToken.Should().BeNull();
        setCredentialsResult.NextStep.Should().Be(LoginFlowSteps.CompleteMfa);
        setCredentialsResult.AllowedMethodsForNextStep.Should().BeEquivalentTo(new[] { CredentialTypes.Totp });
    }

    [Fact]
    public async Task ShouldThrowException_WithInvalidCredentials()
    {
        GetService(out ISender sender);

        var email = "test@example.com";
        var password = "TestPassword123!";
        
        await LoginFlowTestHelpers.RegisterUserAsync(sender, email, password);

        var flow = await sender.Send(new CreateLoginFlow.Command
        {
            Email = email,
            RememberMe = true
        });
        
        await FluentActions.Invoking(() => sender.Send(new SetLoginFlowCredentials.Command
        {
            FlowId = flow.FlowId,
            Body = new SetLoginFlowCredentials.CommandBody
            {
                CredentialType = CredentialTypes.Password,
                CredentialValue = "NotTheCorrectPassword"
            }
        })).Should().ThrowAsync<IncorrectCredentialValueException>();
    }

    [Fact]
    public async Task ShouldThrowException_WithInvalidFlowId()
    {
        GetService(out ISender sender);

        await FluentActions.Invoking(() => sender.Send(new SetLoginFlowCredentials.Command
        {
            FlowId = Guid.NewGuid(),
            Body = new SetLoginFlowCredentials.CommandBody
            {
                CredentialType = CredentialTypes.Password,
                CredentialValue = "NotTheCorrectPassword"
            }
        })).Should().ThrowAsync<FlowNotFoundException>();
    }

    [Theory]
    [InlineData(CredentialTypes.Password, false)]
    [InlineData(CredentialTypes.MagicLink, true)]
    public async Task ShouldThrowException_WithInvalidCredentialType(string credentialType, bool shouldThrow)
    {
        GetService(out ISender sender);

        var email = "test@example.com";
        var password = "TestPassword123!";
        
        await LoginFlowTestHelpers.RegisterUserAsync(sender, email, password);

        var flow = await sender.Send(new CreateLoginFlow.Command
        {
            Email = email,
            RememberMe = true
        });

        var act = () => sender.Send(new SetLoginFlowCredentials.Command
        {
            FlowId = flow.FlowId,
            Body = new SetLoginFlowCredentials.CommandBody
            {
                CredentialType = credentialType,
                CredentialValue = "NotACorrectValue"
            }
        });
        
        if(!shouldThrow)
            await FluentActions.Invoking(act).Should().NotThrowAsync<CredentialTypeNotConfiguredException>();
        else
            await FluentActions.Invoking(act).Should().ThrowAsync<CredentialTypeNotConfiguredException>();
    }

    [Fact]
    public async Task ShouldThrowException_WithInvalidFlowStep()
    {
        GetService(out ISender sender);

        var email = "test@example.com";
        var password = "TestPassword123!";
        
        await LoginFlowTestHelpers.RegisterUserAsync(sender, email, password);

        var flow = await sender.Send(new CreateLoginFlow.Command
        {
            Email = email,
            RememberMe = true
        });
        
        await sender.Send(new SetLoginFlowCredentials.Command
        {
            FlowId = flow.FlowId,
            Body = new SetLoginFlowCredentials.CommandBody
            {
                CredentialType = CredentialTypes.Password,
                CredentialValue = password
            }
        });
        
        await FluentActions.Invoking(() => sender.Send(new SetLoginFlowCredentials.Command
        {
            FlowId = flow.FlowId,
            Body = new SetLoginFlowCredentials.CommandBody
            {
                CredentialType = CredentialTypes.Password,
                CredentialValue = password
            }
        })).Should().ThrowAsync<InvalidFlowStepException>();
    }

    [Fact]
    public async Task ShouldThrowException_WhenFlowExpires()
    {
        GetService(out ISender sender);

        var email = "test@example.com";
        var password = "TestPassword123!";
        
        await LoginFlowTestHelpers.RegisterUserAsync(sender, email, password);

        var flow = await sender.Send(new CreateLoginFlow.Command
        {
            Email = email,
            RememberMe = true
        });

        GetService(out IClock clock);
        GetService(out IOptions<AuthenticationConfiguration> authenticationConfiguration);
        if(clock is FakeClock fakeClock)
            fakeClock.AdvanceMinutes(authenticationConfiguration.Value.LoginFlowTimeoutMinutes + 1);
        
        await FluentActions.Invoking(() => sender.Send(new SetLoginFlowCredentials.Command
        {
            FlowId = flow.FlowId,
            Body = new SetLoginFlowCredentials.CommandBody
            {
                CredentialType = CredentialTypes.Password,
                CredentialValue = password
            }
        })).Should().ThrowAsync<FlowExpiredException>();
    }
    
    [Fact]
    public async Task ShouldThrowException_WhenCredentialDoesNotExist()
    {
        GetService(out ISender sender);

        var email = "test@example.com";
        var password = "TestPassword123!";
        
        await LoginFlowTestHelpers.RegisterUserAsync(sender, email, password);

        var flow = await sender.Send(new CreateLoginFlow.Command
        {
            Email = email,
            RememberMe = true
        });

        GetService(out AppDbContext db);
        await db.IdentityCredentials.IgnoreQueryFilters().ExecuteDeleteAsync();
        
        await FluentActions.Invoking(() => sender.Send(new SetLoginFlowCredentials.Command
        {
            FlowId = flow.FlowId,
            Body = new SetLoginFlowCredentials.CommandBody
            {
                CredentialType = CredentialTypes.Password,
                CredentialValue = password
            }
        })).Should().ThrowAsync<CredentialTypeNotConfiguredException>();
    }
    
    public SetLoginFlowCredentialsTests(SharedFixture sharedFixture) : base(sharedFixture)
    {
    }
}