using Timespace.Api.Infrastructure.Errors;

namespace Timespace.Api.Application.Common.Exceptions;

public class ApiKeyExpiredException : Exception, IBaseException
{
    public string Type => "api_key_expired";
    public int StatusCode => StatusCodes.Status401Unauthorized;
    public string Title => "Api key expired";
    public string? Detail => null;
    public Dictionary<string, object?> MapExtensions() => new();
}