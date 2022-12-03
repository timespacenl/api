using Timespace.Api.Infrastructure.Errors;

namespace Timespace.Api.Application.Features.Authentication.Login.Exceptions;

public class IdentifierNotFoundException : Exception, IBaseException
{
    public string Type => "identifier-not-found";
    public int StatusCode => StatusCodes.Status404NotFound;
    public string Title => "Identifier not found";
    public string? Detail => null;
    public Dictionary<string, object?> MapExtensions() => new();
}