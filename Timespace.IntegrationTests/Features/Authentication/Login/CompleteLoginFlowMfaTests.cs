using FluentAssertions;
using OtpNet;
using Timespace.Api.Application.Features.Authentication.Login.Common.Entities;

namespace Timespace.IntegrationTests.Features.Authentication.Login;

using static LoginFlowTestHelpers;

public class CompleteLoginFlowMfaTests
{
    [Test]
    public async Task ShouldComplete_WithTotpMfaConfigured()
    {
        var token = await CreateUserWithPassword();
        
        SetUserAuthToken(token);
        
        await ConfigureMfa();
        
        SetUserAuthToken(null);
        
        var flow = await CreateLoginFlow(Constants.DefaultUserEmail);
        await SetCredentialsForFlow(flow.FlowId);

        var clock = GetClock();
        var totp = new Totp(Base32Encoding.ToBytes(CurrentMfaSecret));
        var code = totp.ComputeTotp(clock.GetCurrentInstant().ToDateTimeUtc());

        flow = await CompleteTotpMfaForFlow(flow.FlowId, code);
        
        flow.FlowId.Should().NotBeEmpty();
        flow.NextStepAllowedMethods.Should().BeEmpty();
        flow.NextStep.Should().Be(LoginFlowSteps.None);
        flow.SessionToken.Should().NotBeNullOrEmpty();
    }
}