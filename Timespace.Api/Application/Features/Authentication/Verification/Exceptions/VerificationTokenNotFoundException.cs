using Timespace.Api.Infrastructure.Errors;

namespace Timespace.Api.Application.Features.Authentication.Verification.Exceptions;

public class VerificationTokenNotFoundException : Exception, IBaseException
{
    public string Type => "verification_token_not_found";
    public int StatusCode => StatusCodes.Status404NotFound;
    public string Title => "Verification token not found";
    public string? Detail => null;
    public Dictionary<string, object?> MapExtensions() => new();
}