using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NodaTime;
using NodaTime.Testing;
using Timespace.Api.Application.Features.Authentication.Registration.Commands;
using Timespace.Api.Application.Features.Authentication.Verification;
using Timespace.Api.Application.Features.Authentication.Verification.Exceptions;
using Timespace.Api.Infrastructure.Configuration;
using Timespace.Api.Infrastructure.Persistence;

namespace Timespace.IntegrationTests.Features.Authentication.Verification;

public class VerificationTests : IntegrationTest
{
    private async Task Setup(ISender sender, string email = "test@example.com", string password = "Test1234!")
    {
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
                PhoneNumber = "06 12345678"
            }
        });

        await sender.Send(new SetCompanyInformation.Command
        {
            FlowId = flow.FlowId,
            Body = new SetCompanyInformation.CommandBody
            {
                CompanyName = "Test",
                CompanySize = 1,
                Industry = "Test"
            }
        });

        await sender.Send(new CompleteRegistrationFlow.Command
        {
            FlowId = flow.FlowId,
            Body = new CompleteRegistrationFlow.CommandBody
            {
                AcceptTerms = true,
                MagicLink = false,
                Password = password
            }
        });
    }
    
    [Fact]
    public async Task ShouldVerifyEmail_WithCorrectVerificationToken()
    {
        GetService(out ISender sender);
        await Setup(sender);
        
        GetService(out AppDbContext db);

        var verificationToken = await db.Verifications.FirstOrDefaultAsync();

        verificationToken.Should().NotBeNull();
        
        var response = await sender.Send(new VerifyEmail.Command
        {
            VerificationToken = verificationToken!.VerificationToken
        });
        
        response.Success.Should().BeTrue();
    }

    [Fact]
    public async Task ShouldThrowException_WithInvalidVerificationToken()
    {
        GetService(out ISender sender);

        await FluentActions.Invoking(() => sender.Send(new VerifyEmail.Command
        {
            VerificationToken = "invalid"
        })).Should().ThrowAsync<VerificationTokenNotFoundException>();
    }

    [Fact]
    public async Task ShouldThrowException_WithExpiredVerificationToken()
    {
        GetService(out ISender sender);
        await Setup(sender);
        
        GetService(out AppDbContext db);

        var verificationToken = await db.Verifications.FirstOrDefaultAsync();

        verificationToken.Should().NotBeNull();
        
        GetService(out IClock clock);
        GetService(out IOptions<AuthenticationConfiguration> authConfiguration);
        if(clock is FakeClock fakeClock)
            fakeClock.AdvanceMinutes(authConfiguration.Value.VerificationTokenTimeoutMinutes + 1);
        
        await FluentActions.Invoking(() => sender.Send(new VerifyEmail.Command
        {
            VerificationToken = verificationToken!.VerificationToken
        })).Should().ThrowAsync<VerificationTokenExpiredException>();
    }
    
    public VerificationTests(SharedFixture sharedFixture) : base(sharedFixture)
    {
    }
}