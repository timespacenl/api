using FluentAssertions;
using MediatR;
using NodaTime;
using NodaTime.Testing;
using Timespace.Api.Application.Features.Authentication.Common.Exceptions;
using Timespace.Api.Application.Features.Authentication.Registration.Commands;
using Timespace.Api.Application.Features.Authentication.Registration.Common.Entities;

namespace Timespace.IntegrationTests.Features.Authentication.Registration;

public class SetPersonalInformationTests : IntegrationTest
{
    private async Task<CreateRegistrationFlow.Response> Setup(string email = "test@example.com")
    {
        GetService(out ISender sender);

        return await sender.Send(new CreateRegistrationFlow.Command
        {
            Email = email,
            CaptchaToken = "irrelevant"
        });
    }
    
    [Fact]
    public async Task ShouldAdvanceFlow_WithValidData()
    {
        GetService(out ISender sender);
        var flow = await Setup();

        var setPersonalInformationFlowResult = await sender.Send(new SetPersonalInformation.Command
        {
            FlowId = flow.FlowId,
            Body = new SetPersonalInformation.CommandBody
            {
                FirstName = "Test",
                LastName = "Test",
                PhoneNumber = "+380000000000",
            }
        });
        
        setPersonalInformationFlowResult.Should().NotBeNull();
        setPersonalInformationFlowResult.FlowId.Should().Be(flow.FlowId);
        setPersonalInformationFlowResult.NextStep.Should().Be(RegistrationFlowSteps.SetCompanyInformation);
    }

    [Fact]
    public async Task ShouldThrowException_WithInvalidFlowId()
    {
        GetService(out ISender sender);
        
        await FluentActions.Awaiting(() => sender.Send(new SetPersonalInformation.Command
        {
            FlowId = Guid.NewGuid(),
            Body = new SetPersonalInformation.CommandBody
            {
                FirstName = "Test",
                LastName = "Test",
                PhoneNumber = "+380000000000",
            }
        })).Should().ThrowAsync<FlowNotFoundException>();
    }

    [Fact]
    public async Task ShouldThrowException_WhenFlowExpires()
    {
        GetService(out ISender sender);
        var flow = await Setup();
        
        GetService(out IClock clock);
        
        if(clock is FakeClock fakeClock)
            fakeClock.AdvanceHours(1);
            
        await FluentActions.Awaiting(() => sender.Send(new SetPersonalInformation.Command
        {
            FlowId = flow.FlowId,
            Body = new SetPersonalInformation.CommandBody
            {
                FirstName = "Test",
                LastName = "Test",
                PhoneNumber = "+380000000000",
            }
        })).Should().ThrowAsync<FlowExpiredException>();
    }

    [Fact]
    public async Task ShouldThrowException_WhenFlowStepInvalid()
    {
        GetService(out ISender sender);
        var flow = await Setup();

        var setPersonalInformationFlowResult = await sender.Send(new SetPersonalInformation.Command
        {
            FlowId = flow.FlowId,
            Body = new SetPersonalInformation.CommandBody
            {
                FirstName = "Test",
                LastName = "Test",
                PhoneNumber = "+380000000000",
            }
        });
        
        await FluentActions.Awaiting(() => sender.Send(new SetPersonalInformation.Command
        {
            FlowId = flow.FlowId,
            Body = new SetPersonalInformation.CommandBody
            {
                FirstName = "Test",
                LastName = "Test",
                PhoneNumber = "+380000000000",
            }
        })).Should().ThrowAsync<InvalidFlowStepException>();
    }

    public SetPersonalInformationTests(SharedFixture sharedFixture) : base(sharedFixture)
    {
    }
}