using FluentAssertions;
using Timespace.Api.Application.Features.Authentication.Common.Exceptions;

namespace Timespace.IntegrationTests.Features.Authentication.Registration;

using static RegistrationFlowTestHelpers;

public class InvalidFlowStepTests : BaseTestFixture
{
    [Test]
    public async Task SetPersonalInformation_ShouldThrow_WhenFlowStepIsInvalid()
    {
        var flow1 = await SetEmail();
        
        await FluentActions.Invoking(() => SetPersonalInformation(flow1.FlowId))
            .Should()
            .NotThrowAsync<InvalidFlowStepException>();
        
        var flow2 = await SetEmail();
        await SetPersonalInformation(flow2.FlowId);
        
        await FluentActions.Invoking(() => SetPersonalInformation(flow2.FlowId))
            .Should()
            .ThrowAsync<InvalidFlowStepException>();
        
        var flow3 = await SetEmail();
        await SetPersonalInformation(flow3.FlowId);
        await SetCompanyInformation(flow3.FlowId);
        
        await FluentActions.Invoking(() => SetPersonalInformation(flow3.FlowId))
            .Should()
            .ThrowAsync<InvalidFlowStepException>();
        
        var flow4 = await SetEmail();
        await SetPersonalInformation(flow4.FlowId);
        await SetCompanyInformation(flow4.FlowId);
        await CompleteRegistrationFlow(flow4.FlowId);
        
        await FluentActions.Invoking(() => SetPersonalInformation(flow4.FlowId))
            .Should()
            .ThrowAsync<InvalidFlowStepException>();
    }
    
    [Test]
    public async Task SetCompanyInformation_ShouldThrow_WhenFlowStepIsInvalid()
    {
        var flow1 = await SetEmail();
        
        await FluentActions.Invoking(() => SetCompanyInformation(flow1.FlowId))
            .Should()
            .ThrowAsync<InvalidFlowStepException>();
        
        var flow2 = await SetEmail();
        await SetPersonalInformation(flow2.FlowId);
        
        await FluentActions.Invoking(() => SetCompanyInformation(flow2.FlowId))
            .Should()
            .NotThrowAsync<InvalidFlowStepException>();
        
        var flow3 = await SetEmail();
        await SetPersonalInformation(flow3.FlowId);
        await SetCompanyInformation(flow3.FlowId);
        
        await FluentActions.Invoking(() => SetCompanyInformation(flow3.FlowId))
            .Should()
            .ThrowAsync<InvalidFlowStepException>();
        
        var flow4 = await SetEmail();
        await SetPersonalInformation(flow4.FlowId);
        await SetCompanyInformation(flow4.FlowId);
        await CompleteRegistrationFlow(flow4.FlowId);
        
        await FluentActions.Invoking(() => SetCompanyInformation(flow4.FlowId))
            .Should()
            .ThrowAsync<InvalidFlowStepException>();
    }
    
    [Test]
    public async Task CompleteRegistrationFlow_ShouldThrow_WhenFlowStepIsInvalid()
    {
        var flow1 = await SetEmail();
        
        await FluentActions.Invoking(() => CompleteRegistrationFlow(flow1.FlowId))
            .Should()
            .ThrowAsync<InvalidFlowStepException>();
        
        var flow2 = await SetEmail();
        await SetPersonalInformation(flow2.FlowId);
        
        await FluentActions.Invoking(() => CompleteRegistrationFlow(flow2.FlowId))
            .Should()
            .ThrowAsync<InvalidFlowStepException>();
        
        var flow3 = await SetEmail();
        await SetPersonalInformation(flow3.FlowId);
        await SetCompanyInformation(flow3.FlowId);
        
        await FluentActions.Invoking(() => CompleteRegistrationFlow(flow3.FlowId))
            .Should()
            .NotThrowAsync<InvalidFlowStepException>();
        
        var flow4 = await SetEmail();
        await SetPersonalInformation(flow4.FlowId);
        await SetCompanyInformation(flow4.FlowId);
        await CompleteRegistrationFlow(flow4.FlowId);
        
        await FluentActions.Invoking(() => CompleteRegistrationFlow(flow4.FlowId))
            .Should()
            .ThrowAsync<InvalidFlowStepException>();
    }
}