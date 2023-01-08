using Timespace.Api.Infrastructure.Errors;

namespace Timespace.Api.Application.Common.Exceptions;

public class InternalServerErrorException : Exception, IBaseException
{
    public string Type => "internal_server_error";
    public int StatusCode => StatusCodes.Status500InternalServerError;
    public string Title => "Internal server error";
    public string? Detail => null;
    public Dictionary<string, object?> MapExtensions() => new();
}