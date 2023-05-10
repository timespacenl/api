using Timespace.Api.Infrastructure.Errors;

namespace Timespace.Api.Application.Features.Users.Authentication.Login.Exceptions;

public class IncorrectCredentialValueException : Exception, IBaseException
{
    public string Type => "incorrect_credential_value";
    public int StatusCode => StatusCodes.Status400BadRequest;
    public string Title => "Incorrect credential value";
    public string? Detail => null;
    public Dictionary<string, object?> MapExtensions() => new();
}