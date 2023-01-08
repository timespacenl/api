using Timespace.Api.Infrastructure.Errors;

namespace Timespace.Api.Application.Features.Modules.Employees.Exceptions;

public class EmployeeNotFoundException : Exception, IBaseException
{
    public string Type => "employee_not_found";
    public int StatusCode => StatusCodes.Status404NotFound;
    public string Title => "";
    public string? Detail => null;
    public Dictionary<string, object?> MapExtensions() => new();
}