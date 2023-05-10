using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Options;
using NodaTime;
using NodaTime.Testing;
using Timespace.Api.Application.Features.Users.Authentication.Common.Exceptions;
using Timespace.Api.Application.Features.Users.Authentication.Registration.Commands;
using Timespace.Api.Application.Features.Users.Authentication.Registration.Queries;
using Timespace.Api.Infrastructure.Configuration;

namespace Timespace.IntegrationTests.Features.Authentication.Registration;

public class GetRegistrationFlowTests : IntegrationTest
{
    [Fact]
    public async Task ShouldRetrieveFlow_WithValidFlowId()
    {
        GetService(out ISender sender);

        var flow = await sender.Send(new CreateRegistrationFlow.Command
        {
            Email = "test@example.com",
            CaptchaToken = "irrelevant"
        });

        var flowRetrieval = await sender.Send(new GetRegistrationFlow.Query
        {
            FlowId = flow.FlowId
        });
        
        flowRetrieval.Should().NotBeNull();
        flowRetrieval.FlowId.Should().Be(flow.FlowId);
        flowRetrieval.NextStep.Should().Be(flow.NextStep);
        flowRetrieval.ExpiresAt.Should().Be(flow.ExpiresAt);
    }

    [Fact]
    public async Task ShouldThrowException_WithInvalidFlowId()
    {
        GetService(out ISender sender);

        await FluentActions.Invoking(() => sender.Send(new GetRegistrationFlow.Query
        {
            FlowId = Guid.NewGuid()
        })).Should().ThrowAsync<FlowNotFoundException>();
    }

    [Fact]
    public async Task ShouldThrowException_WhenFlowExpires()
    {
        GetService(out ISender sender);

        var flow = await sender.Send(new CreateRegistrationFlow.Command
        {
            Email = "test@example.com",
            CaptchaToken = "irrelevant"
        });
        
        GetService(out IClock clock);
        GetService(out IOptions<AuthenticationConfiguration> authConfig);
        if(clock is FakeClock fakeClock)
            fakeClock.AdvanceMinutes(authConfig.Value.RegistrationFlowTimeoutMinutes + 1);

        await FluentActions.Invoking(() => sender.Send(new GetRegistrationFlow.Query
        {
            FlowId = flow.FlowId
        })).Should().ThrowAsync<FlowExpiredException>();
    }

    public GetRegistrationFlowTests(SharedFixture sharedFixture) : base(sharedFixture)
    {
    }
}