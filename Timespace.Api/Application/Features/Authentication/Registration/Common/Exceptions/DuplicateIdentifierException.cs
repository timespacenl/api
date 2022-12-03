using Timespace.Api.Infrastructure.Errors;

namespace Timespace.Api.Application.Features.Authentication.Registration.Common.Exceptions;

public class DuplicateIdentifierException : Exception, IBaseException
{
    public string Type => "duplicate_identifier";
    public int StatusCode => StatusCodes.Status400BadRequest;
    public string Title => "Duplicate identifier";
    public string? Detail => null;
    public Dictionary<string, object?> MapExtensions() => new();
}