using FluentAssertions;
using Timespace.Api.Application.Features.Authentication.Registration.Commands;
using Timespace.Api.Application.Features.Authentication.Registration.Common.Entities;

namespace Timespace.IntegrationTests.Features.Authentication.Registration;

using static RegistrationFlowTestHelpers;

public class SetPersonalInformationTests : BaseTestFixture
{
    [Test]
    public async Task ShouldAdvanceStep()
    {
        // Arrange
        var result = await SetEmail();

        var setPersonalInformationCommand = new SetPersonalInformation.Command{
            FlowId = result.FlowId,
            Body = new SetPersonalInformation.CommandBody()
            {
                FirstName = "Duke",
                LastName = "Example",
                PhoneNumber = "123456789"
            }
        };
        
        // Act
        var setPersonalInformationResult = await SendAsync(setPersonalInformationCommand);
        
        var dbEntity = await FindAsync<RegistrationFlow>(result.FlowId);
        
        // Assert
        setPersonalInformationResult.Should().NotBeNull();
        setPersonalInformationResult.FlowId.Should().Be(result.FlowId);
        setPersonalInformationResult.NextStep.Should().Be(RegistrationFlowSteps.SetCompanyInformation);
        dbEntity.Should().NotBeNull();
        dbEntity!.FirstName.Should().NotBeNullOrEmpty();
        dbEntity.LastName.Should().NotBeNullOrEmpty();
        dbEntity.PhoneNumber.Should().NotBeNullOrEmpty();
    }
}