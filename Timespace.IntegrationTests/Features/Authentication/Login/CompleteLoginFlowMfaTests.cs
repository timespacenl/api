using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NodaTime;
using NodaTime.Testing;
using OtpNet;
using Timespace.Api.Application.Features.Authentication.Common.Exceptions;
using Timespace.Api.Application.Features.Authentication.Login.Commands;
using Timespace.Api.Application.Features.Authentication.Login.Common.Entities;
using Timespace.Api.Application.Features.Authentication.Login.Exceptions;
using Timespace.Api.Application.Features.Users.Common.Entities.Credentials;
using Timespace.Api.Infrastructure.Configuration;
using Timespace.Api.Infrastructure.Persistence;
using Timespace.Api.Infrastructure.Services;

namespace Timespace.IntegrationTests.Features.Authentication.Login;

public class CompleteLoginFlowMfaTests : IntegrationTest
{
    [Fact]
    public async Task ShouldCompleteFlow_WithValidTotp()
    {
        GetService(out ISender sender);

        var email = "test@example.com";
        var password = "TestPassword123!";
        
        var registrationResult = await LoginFlowTestHelpers.RegisterUserAsync(sender, email, password);

        GetService(out IAuthenticationTokenProvider authenticationTokenProvider);
        authenticationTokenProvider.AuthenticationToken = registrationResult.SessionToken;
        authenticationTokenProvider.AuthenticationTokenType = AuthenticationTokenType.UserSession;
        
        GetService(out IClock clock);
        var secret = await LoginFlowTestHelpers.ConfigureMfa(sender, clock);
        
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
        
        var otp = new Totp(Base32Encoding.ToBytes(secret));
        var code = otp.ComputeTotp(clock.GetCurrentInstant().ToDateTimeUtc());

        var completeMfaResult = await sender.Send(new CompleteLoginFlowMfa.Command
        {
            FlowId = flow.FlowId,
            Body = new CompleteLoginFlowMfa.CommandBody
            {
                CredentialType = CredentialTypes.Totp,
                CredentialValue = code
            }
        });
        
        completeMfaResult.Should().NotBeNull();
        completeMfaResult.SessionToken.Should().NotBeNullOrEmpty();
        completeMfaResult.NextStep.Should().Be(LoginFlowSteps.None);
        completeMfaResult.AllowedMethodsForNextStep.Should().BeEmpty();
    }

    [Fact]
    public async Task ShouldThrowException_WithInvalidTotp()
    {
        GetService(out ISender sender);

        var email = "test@example.com";
        var password = "TestPassword123!";
        
        var registrationResult = await LoginFlowTestHelpers.RegisterUserAsync(sender, email, password);

        GetService(out IAuthenticationTokenProvider authenticationTokenProvider);
        authenticationTokenProvider.AuthenticationToken = registrationResult.SessionToken;
        authenticationTokenProvider.AuthenticationTokenType = AuthenticationTokenType.UserSession;
        
        GetService(out IClock clock);
        var secret = await LoginFlowTestHelpers.ConfigureMfa(sender, clock);
        
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
        
        var otp = new Totp(Base32Encoding.ToBytes(secret));
        var code = otp.ComputeTotp(clock.GetCurrentInstant().ToDateTimeUtc());

        if(clock is FakeClock fakeClock)
            fakeClock.AdvanceMinutes(1);
        
        await FluentActions.Invoking(() => sender.Send(new CompleteLoginFlowMfa.Command
        {
            FlowId = flow.FlowId,
            Body = new CompleteLoginFlowMfa.CommandBody
            {
                CredentialType = CredentialTypes.Totp,
                CredentialValue = code
            }
        })).Should().ThrowAsync<IncorrectCredentialValueException>();
    }
    
    [Fact]
    public async Task ShouldThrowException_WithInvalidFlowId()
    {
        GetService(out ISender sender);

        await FluentActions.Invoking(() => sender.Send(new CompleteLoginFlowMfa.Command
        {
            FlowId = Guid.NewGuid(),
            Body = new CompleteLoginFlowMfa.CommandBody
            {
                CredentialType = CredentialTypes.Totp,
                CredentialValue = "123456"
            }
        })).Should().ThrowAsync<FlowNotFoundException>();
    }

    [Fact]
    public async Task ShouldThrowException_WhenFlowExpires()
    {
        GetService(out ISender sender);

        var email = "test@example.com";
        var password = "TestPassword123!";
        
        var registrationResult = await LoginFlowTestHelpers.RegisterUserAsync(sender, email, password);

        GetService(out IAuthenticationTokenProvider authenticationTokenProvider);
        authenticationTokenProvider.AuthenticationToken = registrationResult.SessionToken;
        authenticationTokenProvider.AuthenticationTokenType = AuthenticationTokenType.UserSession;
        
        GetService(out IClock clock);
        var secret = await LoginFlowTestHelpers.ConfigureMfa(sender, clock);
        
        authenticationTokenProvider.AuthenticationToken = null;
        authenticationTokenProvider.AuthenticationTokenType = null;
        
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

        GetService(out IOptions<AuthenticationConfiguration> authenticationConfiguration);
        if(clock is FakeClock fakeClock)
            fakeClock.AdvanceMinutes(authenticationConfiguration.Value.LoginFlowTimeoutMinutes + 1);
        
        await FluentActions.Invoking(() => sender.Send(new CompleteLoginFlowMfa.Command
        {
            FlowId = flow.FlowId,
            Body = new CompleteLoginFlowMfa.CommandBody
            {
                CredentialType = CredentialTypes.Totp,
                CredentialValue = "123456"
            }
        })).Should().ThrowAsync<FlowExpiredException>();
    }

    [Fact]
    public async Task ShouldThrowException_WithInvalidFlowStep()
    {
        GetService(out ISender sender);

        var email = "test@example.com";
        var password = "TestPassword123!";
        
        var registrationResult = await LoginFlowTestHelpers.RegisterUserAsync(sender, email, password);

        GetService(out IAuthenticationTokenProvider authenticationTokenProvider);
        authenticationTokenProvider.AuthenticationToken = registrationResult.SessionToken;
        authenticationTokenProvider.AuthenticationTokenType = AuthenticationTokenType.UserSession;
        
        GetService(out IClock clock);
        var secret = await LoginFlowTestHelpers.ConfigureMfa(sender, clock);
        
        authenticationTokenProvider.AuthenticationToken = null;
        authenticationTokenProvider.AuthenticationTokenType = null;
        
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
        
        var otp = new Totp(Base32Encoding.ToBytes(secret));
        var code = otp.ComputeTotp(clock.GetCurrentInstant().ToDateTimeUtc());

        await sender.Send(new CompleteLoginFlowMfa.Command
        {
            FlowId = flow.FlowId,
            Body = new CompleteLoginFlowMfa.CommandBody
            {
                CredentialType = CredentialTypes.Totp,
                CredentialValue = code
            }
        });

        await FluentActions.Invoking(() => sender.Send(new CompleteLoginFlowMfa.Command
        {
            FlowId = flow.FlowId,
            Body = new CompleteLoginFlowMfa.CommandBody
            {
                CredentialType = CredentialTypes.Totp,
                CredentialValue = code
            }
        })).Should().ThrowAsync<InvalidFlowStepException>();
    }

    [Fact]
    public async Task ShouldThrowException_WhenCredentialDoesNotExist()
    {
        GetService(out ISender sender);

        var email = "test@example.com";
        var password = "TestPassword123!";
        
        var registrationResult = await LoginFlowTestHelpers.RegisterUserAsync(sender, email, password);

        GetService(out IAuthenticationTokenProvider authenticationTokenProvider);
        authenticationTokenProvider.AuthenticationToken = registrationResult.SessionToken;
        authenticationTokenProvider.AuthenticationTokenType = AuthenticationTokenType.UserSession;
        
        GetService(out IClock clock);
        var secret = await LoginFlowTestHelpers.ConfigureMfa(sender, clock);
        
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
        
        GetService(out AppDbContext db);
        await db.IdentityCredentials.IgnoreQueryFilters().ExecuteDeleteAsync();

        await FluentActions.Invoking(() => sender.Send(new CompleteLoginFlowMfa.Command
        {
            FlowId = flow.FlowId,
            Body = new CompleteLoginFlowMfa.CommandBody
            {
                CredentialType = CredentialTypes.Totp,
                CredentialValue = "123456"
            }
        })).Should().ThrowAsync<CredentialTypeNotConfiguredException>();
    }

    [Fact]
    public async Task ShouldThrowException_WithNonConfiguredCredentialType()
    {
        GetService(out ISender sender);

        var email = "test@example.com";
        var password = "TestPassword123!";
        
        var registrationResult = await LoginFlowTestHelpers.RegisterUserAsync(sender, email, password);

        GetService(out IAuthenticationTokenProvider authenticationTokenProvider);
        authenticationTokenProvider.AuthenticationToken = registrationResult.SessionToken;
        authenticationTokenProvider.AuthenticationTokenType = AuthenticationTokenType.UserSession;
        
        GetService(out IClock clock);
        var secret = await LoginFlowTestHelpers.ConfigureMfa(sender, clock);
        
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

        await FluentActions.Invoking(() => sender.Send(new CompleteLoginFlowMfa.Command
        {
            FlowId = flow.FlowId,
            Body = new CompleteLoginFlowMfa.CommandBody
            {
                CredentialType = CredentialTypes.LookupSecret,
                CredentialValue = ""
            }
        })).Should().ThrowAsync<CredentialTypeNotConfiguredException>();
    }
    
    public CompleteLoginFlowMfaTests(SharedFixture sharedFixture) : base(sharedFixture)
    {
    }
}