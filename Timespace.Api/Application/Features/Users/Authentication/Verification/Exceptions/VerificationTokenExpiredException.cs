namespace Timespace.Api.Application.Features.Users.Authentication.Verification.Exceptions;

public class VerificationTokenExpiredException : Exception
{
    public string Type => "verification_token_expired";
    public int StatusCode => StatusCodes.Status400BadRequest;
    public string Title => "Verification token expired";
    public string? Detail => null;
    public Dictionary<string, object?> MapExtensions() => new();
}