using FluentAssertions;
using Timespace.Api.Application.Features.Authentication.Login.Common.Entities;
using Timespace.Api.Application.Features.Authentication.Login.Exceptions;
using Timespace.Api.Application.Features.Users.Common.Entities.Credentials;

namespace Timespace.IntegrationTests.Features.Authentication.Login;

using static LoginFlowTestHelpers;

public class CreateLoginFlowTests : BaseTestFixture
{
    [Test]
    public async Task ShouldCreateLoginFlow_WithValidEmail()
    {
        await CreateUserWithPassword();

        await FluentActions.Invoking(() => CreateLoginFlow("test@example.com")).Should()
            .NotThrowAsync<IdentifierNotFoundException>();
    }

    [Test]
    public async Task ShouldNotCreateLoginFlow_WithInvalidEmail()
    {
        await CreateUserWithPassword();

        await FluentActions.Invoking(() => CreateLoginFlow("invalid-email@example.com")).Should()
            .ThrowAsync<IdentifierNotFoundException>();
    }

    [Test]
    public async Task ShouldAdvanceToNextStep()
    {
        await CreateUserWithPassword();
        var flow = await CreateLoginFlow(Constants.DefaultUserEmail);

        flow.FlowId.Should().NotBeEmpty();
        flow.NextStepAllowedMethods.Should().BeEquivalentTo(CredentialTypes.Password);
        flow.NextStep.Should().Be(LoginFlowSteps.SetCredentials);
        flow.SessionToken.Should().BeNull();
    }
}