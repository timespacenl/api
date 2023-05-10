using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using NodaTime.Testing;
using Timespace.Api.Application.Features.Users.Authentication.Common.Exceptions;
using Timespace.Api.Application.Features.Users.Authentication.Registration.Commands;
using Timespace.Api.Application.Features.Users.Authentication.Registration.Common.Exceptions;
using Timespace.Api.Infrastructure.Persistence;

namespace Timespace.IntegrationTests.Features.Authentication.Registration;

public class CompleteRegistrationFlowTests : IntegrationTest
{
    private async Task<SetCompanyInformation.Response> Setup(string email = "test@example.com")
    {
        GetService(out ISender sender);

        var flow = await sender.Send(new CreateRegistrationFlow.Command
        {
            Email = email,
            CaptchaToken = "irrelevant"
        });
        
        await sender.Send(new SetPersonalInformation.Command
        {
            FlowId = flow.FlowId,
            Body = new SetPersonalInformation.CommandBody
            {
                FirstName = "Test",
                LastName = "Test",
                PhoneNumber = "+380000000000",
            }
        });
        
        return await sender.Send(new SetCompanyInformation.Command
        {
            FlowId = flow.FlowId,
            Body = new SetCompanyInformation.CommandBody
            {
                CompanyName = "Test",
                CompanySize = 2,
                Industry = "Test"
            }
        });
    }
    
    [Fact]
    public async Task ShouldAdvanceFlow_WithValidData()
    {
        GetService(out ISender sender);
        var flow = await Setup();

        var completeRegistrationFlowResult = await sender.Send(new CompleteRegistrationFlow.Command
        {
            FlowId = flow.FlowId,
            Body = new CompleteRegistrationFlow.CommandBody()
            {
                Password = "Test1234!",
                AcceptTerms = true,
                MagicLink = false
            }
        });
        
        completeRegistrationFlowResult.Should().NotBeNull();
        completeRegistrationFlowResult.SessionToken.Should().NotBeNullOrWhiteSpace();
        
        GetService(out AppDbContext db);

        db.Identities.IgnoreQueryFilters().Should().HaveCount(1);
        db.IdentityCredentials.IgnoreQueryFilters().Should().HaveCount(1);
        db.IdentityIdentifiers.IgnoreQueryFilters().Should().HaveCount(1);
        db.Tenants.IgnoreQueryFilters().Should().HaveCount(1);
        db.Sessions.IgnoreQueryFilters().Should().HaveCount(1);
    }

    [Fact]
    public async Task ShouldThrowException_WithInvalidFlowId()
    {
        GetService(out ISender sender);
        
        await FluentActions.Awaiting(() => sender.Send(new CompleteRegistrationFlow.Command
        {
            FlowId = Guid.NewGuid(),
            Body = new CompleteRegistrationFlow.CommandBody()
            {
                Password = "Test1234!",
                AcceptTerms = true,
                MagicLink = false
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
            
        await FluentActions.Awaiting(() => sender.Send(new CompleteRegistrationFlow.Command
        {
            FlowId = flow.FlowId,
            Body = new CompleteRegistrationFlow.CommandBody()
            {
                Password = "Test1234!",
                AcceptTerms = true,
                MagicLink = false
            }
        })).Should().ThrowAsync<FlowExpiredException>();
    }

    [Fact]
    public async Task ShouldThrowException_WhenFlowStepInvalid()
    {
        GetService(out ISender sender);
        var flow = await Setup();

        await sender.Send(new CompleteRegistrationFlow.Command
        {
            FlowId = flow.FlowId,
            Body = new CompleteRegistrationFlow.CommandBody()
            {
                Password = "Test1234!",
                AcceptTerms = true,
                MagicLink = false
            }
        });
        
        await FluentActions.Awaiting(() => sender.Send(new CompleteRegistrationFlow.Command
        {
            FlowId = flow.FlowId,
            Body = new CompleteRegistrationFlow.CommandBody()
            {
                Password = "Test1234!",
                AcceptTerms = true,
                MagicLink = false
            }
        })).Should().ThrowAsync<InvalidFlowStepException>();
    }

    [Fact]
    public async Task ShouldThrowException_WithCredentialMismatch()
    {
        GetService(out ISender sender);
        var flow = await Setup();
        
        await FluentActions.Awaiting(() => sender.Send(new CompleteRegistrationFlow.Command
        {
            FlowId = flow.FlowId,
            Body = new CompleteRegistrationFlow.CommandBody()
            {
                Password = "Test1234!",
                AcceptTerms = true,
                MagicLink = true
            }
        })).Should().ThrowAsync<CredentialMismatchException>();
    }
    
    [Fact]
    public async Task ShouldThrowException_WithMissingCredential()
    {
        GetService(out ISender sender);
        var flow = await Setup();
        
        await FluentActions.Awaiting(() => sender.Send(new CompleteRegistrationFlow.Command
        {
            FlowId = flow.FlowId,
            Body = new CompleteRegistrationFlow.CommandBody()
            {
                Password = null,
                AcceptTerms = true,
                MagicLink = false
            }
        })).Should().ThrowAsync<MissingCredentialException>();
    }
    
    public CompleteRegistrationFlowTests(SharedFixture sharedFixture) : base(sharedFixture)
    {
    }
}