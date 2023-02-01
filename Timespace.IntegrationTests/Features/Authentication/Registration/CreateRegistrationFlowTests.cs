using FluentAssertions;
using MediatR;
using NodaTime;
using NodaTime.Testing;
using Timespace.Api.Application.Common.Exceptions;
using Timespace.Api.Application.Features.Authentication.Registration.Commands;
using Timespace.Api.Application.Features.Authentication.Registration.Common.Entities;
using Timespace.Api.Application.Features.Authentication.Registration.Common.Exceptions;

namespace Timespace.IntegrationTests.Features.Authentication.Registration;

public class CreateRegistrationFlowTests : IntegrationTest
{
    [Fact]
    public async Task ShouldNotCreateRegistrationFlow_WhenExistingFlowExists()
    {
        GetService(out ISender sender);

        var email = "test@example.com";

        var flow = await sender.Send(new CreateRegistrationFlow.Command
        {
            Email = email,
            CaptchaToken = "irrelevant"
        });

        await FluentActions.Invoking(() => sender.Send(new CreateRegistrationFlow.Command
        {
            Email = email,
            CaptchaToken = "irrelevant"
        })).Should().ThrowAsync<DuplicateIdentifierException>();
    }
    
    [Fact]
    public async Task ShouldCreateRegistrationFlow_WhenExistingFlowExpires()
    {
        GetService(out ISender sender);

        var email = "test@example.com";

        var flow = await sender.Send(new CreateRegistrationFlow.Command
        {
            Email = email,
            CaptchaToken = "irrelevant"
        });

        GetService(out IClock clock);
        (clock as FakeClock)?.AdvanceHours(1);
        
        await FluentActions.Invoking(() => sender.Send(new CreateRegistrationFlow.Command
        {
            Email = email
        })).Should().NotThrowAsync<DuplicateIdentifierException>();
    }
    
    [Fact]
    public async Task ShouldNotCreateRegistrationFlow_WhenUserAlreadyExists()
    { 
        GetService(out ISender sender);

        var email = "test@example.com";
        
        await RegistrationFlowTestHelpers.RegisterUserAsync(sender, email);

        await FluentActions.Invoking(() => sender.Send(new CreateRegistrationFlow.Command
        {
            Email = email,
            CaptchaToken = "irrelevant"
        })).Should().ThrowAsync<DuplicateIdentifierException>();
    }

    [Fact]
    public async Task ShouldNotCreateRegistrationFlow_WithInvalidEmail()
    {
        GetService(out ISender sender);
        
        await FluentActions.Invoking(() => sender.Send(new CreateRegistrationFlow.Command
        {
            Email = "invalid-email",
            CaptchaToken = "irrelevant"
        })).Should().ThrowAsync<ValidationException>();
    }
    
    [Fact]
    public async Task ShouldCreateRegistrationFlow_WithValidEmail()
    {
        GetService(out ISender sender);

        var flow = await sender.Send(new CreateRegistrationFlow.Command
        {
            Email = "test@example.com",
            CaptchaToken = "irrelevant"
        });
        
        flow.FlowId.Should().NotBeEmpty();
        flow.NextStep.Should().Be(RegistrationFlowSteps.SetPersonalInformation);
    }
    
    public CreateRegistrationFlowTests(SharedFixture sharedFixture) : base(sharedFixture)
    {
    }
}