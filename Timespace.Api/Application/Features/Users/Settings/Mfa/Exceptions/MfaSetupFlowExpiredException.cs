using Timespace.Api.Infrastructure.Errors;

namespace Timespace.Api.Application.Features.Users.Settings.Mfa.Exceptions;

public class MfaSetupFlowExpiredException : Exception, IBaseException
{
    public string Type => "mfa_setup_flow_expired";
    public int StatusCode => StatusCodes.Status400BadRequest;
    public string Title => "MFA setup flow expired";
    public string? Detail => null;
    public Dictionary<string, object?> MapExtensions() => new();
}