using Timespace.Api.Infrastructure.Errors;

namespace Timespace.Api.Application.Features.Users.Settings.Mfa.Exceptions;

public class MfaAlreadySetUpException : Exception, IBaseException
{
    public string Type => "mfa_already_set_up";
    public int StatusCode => StatusCodes.Status400BadRequest;
    public string Title => "MFA is already set up";
    public string? Detail => null;
    public Dictionary<string, object?> MapExtensions() => new();
}