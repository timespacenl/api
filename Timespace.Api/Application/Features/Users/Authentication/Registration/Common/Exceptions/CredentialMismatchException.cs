using Timespace.Api.Infrastructure.Errors;

namespace Timespace.Api.Application.Features.Users.Authentication.Registration.Common.Exceptions;

public class CredentialMismatchException : Exception, IBaseException
{
    public string Type => "credential_mismatch";
    public int StatusCode => StatusCodes.Status400BadRequest;
    public string Title => "Credential mismatch";
    public string? Detail => "You can only register one type of credential during registration";
    public Dictionary<string, object?> MapExtensions() => new();
}