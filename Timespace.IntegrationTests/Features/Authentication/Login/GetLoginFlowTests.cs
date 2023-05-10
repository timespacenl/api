using FluentAssertions;
using FluentAssertions.NodaTime;
using MediatR;
using Microsoft.Extensions.Options;
using NodaTime;
using NodaTime.Testing;
using Timespace.Api.Application.Features.Users.Authentication.Common.Exceptions;
using Timespace.Api.Application.Features.Users.Authentication.Login.Commands;
using Timespace.Api.Application.Features.Users.Authentication.Login.Queries;
using Timespace.Api.Infrastructure.Configuration;

namespace Timespace.IntegrationTests.Features.Authentication.Login;

public class GetLoginFlowMfaTests : IntegrationTest
{
    [Fact]
    public async Task ShouldRetrieveFlow_WithValidFlowId()
    {
        GetService(out ISender sender);
        var email = "test@example.com";
        
        await LoginFlowTestHelpers.RegisterUserAsync(sender, email);

        var flow = await sender.Send(new CreateLoginFlow.Command
        {
            Email = email,
            RememberMe = true
        });

        var flowRetrieval = await sender.Send(new GetLoginFlow.Query
        {
            FlowId = flow.FlowId
        });
        
        flowRetrieval.Should().NotBeNull();
        flowRetrieval.FlowId.Should().Be(flow.FlowId);
        flowRetrieval.NextStep.Should().Be(flow.NextStep);
        flowRetrieval.ExpiresAt.Should().Be(flow.ExpiresAt);
        flowRetrieval.AllowedMethodsForNextStep.Should().BeEquivalentTo(flow.AllowedMethodsForNextStep);
    }

    [Fact]
    public async Task ShouldThrowException_WithInvalidFlowId()
    {
        GetService(out ISender sender);

        await FluentActions.Invoking(() => sender.Send(new GetLoginFlow.Query
        {
            FlowId = Guid.NewGuid()
        })).Should().ThrowAsync<FlowNotFoundException>();
    }

    [Fact]
    public async Task ShouldThrowException_WhenFlowExpires()
    {
        GetService(out ISender sender);

        var email = "test@example.com";
        
        await LoginFlowTestHelpers.RegisterUserAsync(sender, email);

        var flow = await sender.Send(new CreateLoginFlow.Command
        {
            Email = email,
            RememberMe = true
        });

        GetService(out IClock clock);
        GetService(out IOptions<AuthenticationConfiguration> authConfig);
        if(clock is FakeClock fakeClock)
            fakeClock.AdvanceMinutes(authConfig.Value.RegistrationFlowTimeoutMinutes + 1);

        await FluentActions.Invoking(() => sender.Send(new GetLoginFlow.Query
        {
            FlowId = flow.FlowId
        })).Should().ThrowAsync<FlowExpiredException>();
    }

    public GetLoginFlowMfaTests(SharedFixture sharedFixture) : base(sharedFixture)
    {
    }
}