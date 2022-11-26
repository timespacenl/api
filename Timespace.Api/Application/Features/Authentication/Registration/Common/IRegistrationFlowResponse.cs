namespace Timespace.Api.Application.Features.Authentication.Registration.Common;

public interface IRegistrationFlowResponse
{
    public Guid FlowId { get; }
    public string NextStep { get; }
}