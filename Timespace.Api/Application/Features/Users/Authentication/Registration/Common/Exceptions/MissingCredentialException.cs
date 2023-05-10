using Timespace.Api.Infrastructure.Errors;

namespace Timespace.Api.Application.Features.Users.Authentication.Registration.Common.Exceptions;

public class MissingCredentialException : Exception, IBaseException
{
    public string Type => "missing_credential";
    public int StatusCode => StatusCodes.Status400BadRequest;
    public string Title => "Missing credential";
    public string? Detail => "You need to provide atleast one credential to complete registration";
    public Dictionary<string, object?> MapExtensions() => new();
}