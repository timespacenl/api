using FluentAssertions;
using MediatR;
using NodaTime;
using NodaTime.Testing;
using Timespace.Api.Application.Features.Authentication.Common.Exceptions;
using Timespace.Api.Application.Features.Authentication.Registration.Commands;
using Timespace.Api.Application.Features.Authentication.Registration.Common.Entities;

namespace Timespace.IntegrationTests.Features.Authentication.Registration;

public class SetCompanyInformationTests : IntegrationTest
{
    private async Task<SetPersonalInformation.Response> Setup(string email = "test@example.com")
    {
        GetService(out ISender sender);

        var flow = await sender.Send(new CreateRegistrationFlow.Command
        {
            Email = email,
            CaptchaToken = "irrelevant"
        });
        
        return await sender.Send(new SetPersonalInformation.Command
        {
            FlowId = flow.FlowId,
            Body = new SetPersonalInformation.CommandBody
            {
                FirstName = "Test",
                LastName = "Test",
                PhoneNumber = "+380000000000",
            }
        });
    }

    [Fact]
    public async Task ShouldAdvanceFlow_WithValidData()
    {
        GetService(out ISender sender);
        var flow = await Setup();

        var setCompanyInformationFlowResult = await sender.Send(new SetCompanyInformation.Command
        {
            FlowId = flow.FlowId,
            Body = new SetCompanyInformation.CommandBody
            {
                CompanyName = "Test",
                CompanySize = 2,
                Industry = "Test"
            }
        });
        
        setCompanyInformationFlowResult.Should().NotBeNull();
        setCompanyInformationFlowResult.FlowId.Should().Be(flow.FlowId);
        setCompanyInformationFlowResult.NextStep.Should().Be(RegistrationFlowSteps.CompleteRegistrationFlow);
    }

    [Fact]
    public async Task ShouldThrowException_WithInvalidFlowId()
    {
        GetService(out ISender sender);
        
        await FluentActions.Awaiting(() => sender.Send(new SetCompanyInformation.Command
        {
            FlowId = Guid.NewGuid(),
            Body = new SetCompanyInformation.CommandBody
            {
                CompanyName = "Test",
                CompanySize = 2,
                Industry = "Test"
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
            
        await FluentActions.Awaiting(() => sender.Send(new SetCompanyInformation.Command
        {
            FlowId = flow.FlowId,
            Body = new SetCompanyInformation.CommandBody
            {
                CompanyName = "Test",
                CompanySize = 2,
                Industry = "Test"
            }
        })).Should().ThrowAsync<FlowExpiredException>();
    }

    [Fact]
    public async Task ShouldThrowException_WhenFlowStepInvalid()
    {
        GetService(out ISender sender);
        var flow = await Setup();

        await sender.Send(new SetCompanyInformation.Command
        {
            FlowId = flow.FlowId,
            Body = new SetCompanyInformation.CommandBody
            {
                CompanyName = "Test",
                CompanySize = 2,
                Industry = "Test"
            }
        });
        
        await FluentActions.Awaiting(() => sender.Send(new SetCompanyInformation.Command
        {
            FlowId = flow.FlowId,
            Body = new SetCompanyInformation.CommandBody
            {
                CompanyName = "Test",
                CompanySize = 2,
                Industry = "Test"
            }
        })).Should().ThrowAsync<InvalidFlowStepException>();
    }

    public SetCompanyInformationTests(SharedFixture sharedFixture) : base(sharedFixture)
    {
    }
}