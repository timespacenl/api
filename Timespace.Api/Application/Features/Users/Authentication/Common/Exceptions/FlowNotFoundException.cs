using Timespace.Api.Infrastructure.Errors;

namespace Timespace.Api.Application.Features.Users.Authentication.Common.Exceptions;

public class FlowNotFoundException : Exception, IBaseException
{
    public string Type => "flow_not_found";
    public int StatusCode => StatusCodes.Status404NotFound;
    public string Title => "Flow not found";
    public string? Detail => "Flow with this id could not be found";
    public Dictionary<string, object?> MapExtensions() => new();
}