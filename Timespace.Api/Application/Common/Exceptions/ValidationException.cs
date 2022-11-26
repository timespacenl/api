using Timespace.Api.Infrastructure.Errors;

namespace Timespace.Api.Application.Common.Exceptions;

public class ValidationException : Exception, IBaseException
{
    public ValidationException(IReadOnlyDictionary<string, string[]> errorsDictionary)
    {
        Extensions["errors"] = errorsDictionary;
    }

    public string Type => "validation-error";
    public int StatusCode => StatusCodes.Status422UnprocessableEntity;
    public string Title => "Validation error";
    public string? Detail => null;
    public Dictionary<string, object?> Extensions { get; } = new();
}