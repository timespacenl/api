using Timespace.Api.Infrastructure.Errors;

namespace Timespace.Api.Application.Features.Users.Authentication.Login.Exceptions;

public class CredentialTypeNotConfiguredException : Exception, IBaseException
{
    public string Type => "credential_type_not_configured";
    public int StatusCode => StatusCodes.Status400BadRequest;
    public string Title => "Credential type not configured";
    public string? Detail => "";
    public Dictionary<string, object?> MapExtensions() => new();
}