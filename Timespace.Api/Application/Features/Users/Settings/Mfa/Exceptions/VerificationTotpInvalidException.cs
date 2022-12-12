using Timespace.Api.Infrastructure.Errors;

namespace Timespace.Api.Application.Features.Users.Settings.Mfa.Exceptions;

public class VerificationTotpInvalidException : Exception, IBaseException
{
    public string Type => "verification_totp_invalid";
    public int StatusCode => StatusCodes.Status400BadRequest;
    public string Title => "Verification TOTP invalid";
    public string? Detail => null;
    public Dictionary<string, object?> MapExtensions() => new();
}