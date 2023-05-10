namespace Timespace.Api.Application.Features.Users.Authentication.Registration.Common;

public interface IRegistrationFlowResponse
{
    public Guid FlowId { get; }
    public string NextStep { get; }
    public Instant ExpiresAt { get; }
}