using Timespace.Api.Application.Features.ExternalSourceGeneration.Generators.TypescriptApiClientGenerator.Models;

namespace Timespace.Api.Application.Features.ExternalSourceGeneration.Generators.TypescriptApiClientGenerator.Extensions;

public static class FluentValidatorExtensions
{
    public static string? GetZodValidator(this FluentValidator validator)
    {
        if(Constants.ZodFluentValidationValidatorMapping.TryGetValue(validator.ValidatorName, out var zodValidator))
        {
            return zodValidator.Mapping;
        }
        
        throw new NotImplementedException($"Validator {validator.ValidatorName} is not implemented in Zod validator mapping");
    }
    
    public static string GetParametersAsString(this List<FluentValidatorParameter> validatorParameters)
    {
        return string.Join(", ", validatorParameters.Select(x => x.Value).ToList());
    }
}