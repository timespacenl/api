using Timespace.Api.Infrastructure.Errors;

namespace Timespace.Api.Application.Features.Users.Authentication.Common.Exceptions;

public class FlowExpiredException : Exception, IBaseException
{
    public string Type => "flow_expired";
    public int StatusCode => StatusCodes.Status400BadRequest;
    public string Title => "Flow expired";
    public string? Detail => "Flow expired";
    public Dictionary<string, object?> MapExtensions() => new();
}