using FluentAssertions;
using NodaTime;
using Timespace.Api.Application.Features.Authentication.Registration.Commands;
using Timespace.Api.Application.Features.Authentication.Registration.Common.Entities;

namespace Timespace.IntegrationTests.Features.Authentication.Registration;

using static Helpers;

public class CreateRegistrationFlowTests : BaseTestFixture
{
    [Test]
    public async Task ShouldCreateFlow_WithValidEmail()
    {
        var result = await SetEmail();
        
        var dbEntity = await FindAsync<RegistrationFlow>(result.FlowId);
        
        dbEntity.Should().NotBeNull();
        dbEntity!.Email.Should().NotBeNullOrEmpty();
        dbEntity.NextStep.Should().Be(RegistrationFlowSteps.SetPersonalInformation);
    }
}