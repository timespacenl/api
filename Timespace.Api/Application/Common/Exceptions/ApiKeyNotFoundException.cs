using Timespace.Api.Infrastructure.Errors;

namespace Timespace.Api.Application.Common.Exceptions;

public class ApiKeyNotFoundException : Exception, IBaseException
{
    public string Type => "api_key_not_found";
    public int StatusCode => StatusCodes.Status401Unauthorized;
    public string Title => "Api key not found";
    public string? Detail => null;
    public Dictionary<string, object?> MapExtensions() => new();
}