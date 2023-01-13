using FluentAssertions;
using MediatR;
using NodaTime;
using OtpNet;
using Timespace.Api.Application.Features.Authentication.Login.Commands;
using Timespace.Api.Application.Features.Authentication.Login.Common.Entities;
using Timespace.Api.Application.Features.Users.Common.Entities.Credentials;
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
        completeMfaResult.NextStepAllowedMethods.Should().BeEmpty();
    }

    public CompleteLoginFlowMfaTests(SharedFixture sharedFixture) : base(sharedFixture)
    {
    }
}