using Timespace.Api.Infrastructure.Errors;

namespace Timespace.Api.Application.Common.Exceptions;

public class SessionNotFoundException : Exception, IBaseException
{
    public string Type => "session_not_found";
    public int StatusCode => StatusCodes.Status401Unauthorized;
    public string Title => "Session not found";
    public string? Detail => null;
    public Dictionary<string, object?> MapExtensions() => new();
}