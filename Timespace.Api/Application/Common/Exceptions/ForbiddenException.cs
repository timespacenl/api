using Timespace.Api.Infrastructure.Errors;

namespace Timespace.Api.Application.Common.Exceptions;

public class ForbiddenException : Exception, IBaseException
{
    public string Type => "forbidden";
    public int StatusCode => StatusCodes.Status403Forbidden;
    public string Title => "Forbidden";
    public string? Detail => null;
    public Dictionary<string, object?> MapExtensions() => new();
}