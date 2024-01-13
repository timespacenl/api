using Timespace.Api.Infrastructure.Errors;

namespace Timespace.Api.Application.Common.Exceptions;

public class UnauthenticatedException : Exception, IBaseException
{
    public string Type => "unauthenticated";
    public int StatusCode => StatusCodes.Status401Unauthorized;
    public string Title => "Unauthenticated";
    public string? Detail => "You did not provide a valid session token or api key";
    public Dictionary<string, object?> MapExtensions() => new();
}