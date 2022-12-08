using Timespace.Api.Infrastructure.Errors;

namespace Timespace.Api.Application.Common.Exceptions;

public class SessionExpiredException : Exception, IBaseException
{
    public string Type => "session_expired";
    public int StatusCode => StatusCodes.Status401Unauthorized;
    public string Title => "Session expired";
    public string? Detail => null;
    public Dictionary<string, object?> MapExtensions() => new();
}