using FluentAssertions;
using Timespace.Api.Application.Features.Authentication.Login.Common.Entities;
using Timespace.Api.Application.Features.Users.Common.Entities.Credentials;

namespace Timespace.IntegrationTests.Features.Authentication.Login;

using static LoginFlowTestHelpers;

public class SetLoginFlowCredentialsTests : BaseTestFixture
{
    [Test]
    public async Task ShouldAdvanceToNextStep_WithMfaConfigured()
    {
        var token = await CreateUserWithPassword();
        
        SetUserAuthToken(token);
        
        await ConfigureMfa();
        
        SetUserAuthToken(null);
        
        var flow = await CreateLoginFlow(Constants.DefaultUserEmail);
        flow = await SetCredentialsForFlow(flow.FlowId);
        
        flow.FlowId.Should().NotBeEmpty();
        flow.NextStepAllowedMethods.Should().BeEquivalentTo(CredentialTypes.Totp);
        flow.NextStep.Should().Be(LoginFlowSteps.CompleteMfa);
        flow.SessionToken.Should().BeNull();
    }
    
    [Test]
    public async Task ShouldComplete_WithoutMfaConfigured()
    {
        await CreateUserWithPassword();
        var flow = await CreateLoginFlow(Constants.DefaultUserEmail);
        flow = await SetCredentialsForFlow(flow.FlowId);
        
        flow.FlowId.Should().NotBeEmpty();
        flow.NextStepAllowedMethods.Should().BeEmpty();
        flow.NextStep.Should().Be(LoginFlowSteps.None);
        flow.SessionToken.Should().NotBeNullOrEmpty();
    }
}