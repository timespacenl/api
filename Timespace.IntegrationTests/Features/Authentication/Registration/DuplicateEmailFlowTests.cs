using FluentAssertions;
using Timespace.Api.Application.Features.Authentication.Registration.Common.Exceptions;

namespace Timespace.IntegrationTests.Features.Authentication.Registration;

using static RegistrationFlowTestHelpers;

public class DuplicateEmailFlowTests : BaseTestFixture
{
    [Test]
    public async Task ShouldThrow_WithExistingUser()
    {
        var result = await SetEmail(false);
        await SetPersonalInformation(result.FlowId);
        await SetCompanyInformation(result.FlowId);
        await CompleteRegistrationFlow(result.FlowId);

        await FluentActions.Invoking(() => SetEmail(false)).Should().ThrowAsync<DuplicateIdentifierException>();
    }
    
    [Test]
    public async Task ShouldThrow_WithExistingFlow()
    {
        var result = await SetEmail(false);

        await FluentActions.Invoking(() => SetEmail(false)).Should().ThrowAsync<DuplicateIdentifierException>();
    }
}