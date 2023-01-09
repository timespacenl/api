using FluentAssertions;
using NodaTime;
using Timespace.Api.Application.Features.Authentication.Common.Exceptions;
using Timespace.Api.Application.Features.Authentication.Registration.Queries;

namespace Timespace.IntegrationTests.Features.Authentication.Registration;

using static RegistrationFlowTestHelpers;

public class FlowExpirationTests : BaseTestFixture
{
    [Test]
    public async Task SetPersonalInformation_ShouldThrow_WhenFlowIsExpired()
    {
        // Arrange
        var result = await SetEmail();
        
        AdvanceTime(Duration.FromMinutes(10));
        
        // Assert
        await FluentActions.Invoking(() => SetCompanyInformation(result.FlowId))
            .Should()
            .ThrowAsync<FlowExpiredException>();
    }
    
    [Test]
    public async Task SetCompanyInformation_ShouldThrow_WhenFlowIsExpired()
    {
        // Arrange
        var result = await SetEmail();

        await SetPersonalInformation(result.FlowId);
        
        AdvanceTime(Duration.FromMinutes(10));
        
        // Assert
        await FluentActions.Invoking(() => SetCompanyInformation(result.FlowId))
            .Should()
            .ThrowAsync<FlowExpiredException>();
    }
    
    [Test]
    public async Task CompleteRegistrationFlow_ShouldThrow_WhenFlowIsExpired()
    {
        // Arrange
        var result = await SetEmail();

        await SetPersonalInformation(result.FlowId);
        await SetCompanyInformation(result.FlowId);
        
        AdvanceTime(Duration.FromMinutes(10));
        
        // Assert
        await FluentActions.Invoking(() => CompleteRegistrationFlow(result.FlowId))
            .Should()
            .ThrowAsync<FlowExpiredException>();
    }
    
    [Test]
    public async Task GetFlow_ShouldThrow_WhenFlowIsExpired()
    {
        // Arrange
        var result = await SetEmail();

        var command = new GetRegistrationFlow.Query
        {
            FlowId = result.FlowId
        };
        
        AdvanceTime(Duration.FromMinutes(10));
        
        // Assert
        await FluentActions.Invoking(() => SendAsync(command))
            .Should()
            .ThrowAsync<FlowExpiredException>();
    }
}