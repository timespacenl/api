using Timespace.Api.Infrastructure.Errors;

namespace Timespace.Api.Application.Features.Authentication.Registration.Common.Exceptions;

public class InvalidFlowStepException : Exception, IBaseException
{
    public InvalidFlowStepException(string nextStep)
    {
        NextStep = nextStep;
    }
    
    public string Type => "invalid_flow_step";
    public int StatusCode => StatusCodes.Status400BadRequest;
    public string Title => "Invalid flow step";
    public string? Detail => null;
    
    public string NextStep { get; }

    public Dictionary<string, object?> MapExtensions() => new()
    {
        { nameof(NextStep), NextStep }
    };
}

public class InvalidFlowStepErrorResponse
{
    public string Type => "invalid_flow_step";
    public int StatusCode => StatusCodes.Status400BadRequest;
    public string Title => "Invalid flow step";
    public string? Detail => null;
    public string NextStep => null!;
}