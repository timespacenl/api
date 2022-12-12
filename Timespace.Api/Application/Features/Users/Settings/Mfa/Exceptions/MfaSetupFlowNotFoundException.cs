using Timespace.Api.Infrastructure.Errors;

namespace Timespace.Api.Application.Features.Users.Settings.Mfa.Exceptions;

public class MfaSetupFlowNotFoundException : Exception, IBaseException
{
    public string Type => "mfa_setup_flow_invalid";
    public int StatusCode => StatusCodes.Status404NotFound;
    public string Title => "MFA setup flow invalid";
    public string? Detail => "Could not find a valid MFA setup flow with this id.";
    public Dictionary<string, object?> MapExtensions() => new();
}